using System.Collections.Generic;

namespace ColorGateBlast
{
    /// <summary>
    /// Pure model of the Retro Alarm Clock. Every part has HP, a colour affinity (only turrets of that colour hurt
    /// it) and a layer (a part is only targetable once every part on a lower layer is gone, e.g. the copper gears
    /// hide inside the casing). Destroying a TNT box blasts every outer-layer part within
    /// <see cref="MonumentLayout.BlastRadius"/>, chaining into any other TNT it reaches.
    /// </summary>
    public sealed class MonumentModel
    {
        public readonly PartSpec[] Parts;
        public readonly int[] MaxHp;
        public readonly int[] Hp;
        public int AliveCount;

        private readonly float[] px;
        private readonly float[] py;

        public MonumentModel(LevelDefinition def)
        {
            Parts = def.parts.ToArray();
            int n = Parts.Length;
            MaxHp = new int[n];
            Hp = new int[n];
            px = new float[n];
            py = new float[n];
            for (int i = 0; i < n; i++)
            {
                MaxHp[i] = def.PartHp(i);
                Hp[i] = MaxHp[i];
                MonumentLayout.Position(def.monument, Parts[i].type, Parts[i].slot, out px[i], out py[i]);
            }
            AliveCount = n;
        }

        private MonumentModel(MonumentModel o)
        {
            Parts = o.Parts;
            MaxHp = o.MaxHp;
            px = o.px;
            py = o.py;
            Hp = (int[])o.Hp.Clone();
            AliveCount = o.AliveCount;
        }

        public MonumentModel Clone() { return new MonumentModel(this); }

        public bool IsAlive(int i) { return Hp[i] > 0; }

        /// <summary>Alive and not hidden behind a still-alive part (see <see cref="PartSpec.hiddenByMask"/>).</summary>
        public bool IsTargetable(int i)
        {
            if (Hp[i] <= 0) return false;
            int mask = Parts[i].hiddenByMask;
            for (int j = 0; mask != 0 && j < Parts.Length; j++)
                if (((mask >> j) & 1) != 0 && Hp[j] > 0) return false;
            return true;
        }

        /// <summary>The part a turret of this colour shoots next (lowest layer, then lowest index), or -1.</summary>
        public int FindTarget(int color)
        {
            int best = -1;
            for (int i = 0; i < Parts.Length; i++)
            {
                if ((int)Parts[i].color != color || !IsTargetable(i)) continue;
                if (best < 0 || Parts[i].layer < Parts[best].layer) best = i;
            }
            return best;
        }

        public int AliveOfColor(int color)
        {
            int n = 0;
            for (int i = 0; i < Parts.Length; i++) if (Hp[i] > 0 && (int)Parts[i].color == color) n++;
            return n;
        }

        /// <summary>
        /// Deals one point of damage. Appends every part destroyed (including blast victims) to
        /// <paramref name="destroyed"/> and every TNT that detonated to <paramref name="detonated"/> (either may be null).
        /// </summary>
        public void Hit(int i, List<int> destroyed, List<int> detonated)
        {
            if (Hp[i] <= 0) return;
            Hp[i]--;
            if (Hp[i] == 0) Destroy(i, destroyed, detonated);
        }

        private void Destroy(int i, List<int> destroyed, List<int> detonated)
        {
            Hp[i] = 0;
            AliveCount--;
            if (destroyed != null) destroyed.Add(i);
            if (Parts[i].type != PartType.Tnt) return;

            if (detonated != null) detonated.Add(i);
            float r2 = MonumentLayout.BlastRadius * MonumentLayout.BlastRadius;
            for (int j = 0; j < Parts.Length; j++)
            {
                if (Hp[j] <= 0 || Parts[j].layer > Parts[i].layer) continue;
                float dx = px[j] - px[i], dy = py[j] - py[i];
                if (dx * dx + dy * dy <= r2) Destroy(j, destroyed, detonated);
            }
        }

        public void AppendKey(List<char> key)
        {
            for (int i = 0; i < Hp.Length; i++) key.Add((char)(Hp[i] + 1));
        }
    }
}
