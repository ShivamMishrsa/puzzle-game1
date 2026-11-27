using System.Collections.Generic;

namespace ColorGateBlast
{
    /// <summary>
    /// Pure model of the destructible monument. Every part has HP, a colour affinity (only turrets of that colour
    /// hurt it) and may be hidden behind other parts (see <see cref="PartSpec.hiddenByMask"/>).
    ///
    /// Brass screw-bolts: a part with <see cref="PartSpec.hasScrew"/> is pinned down. While the screw is attached the
    /// plate cannot be damaged (shots are deflected); a turret of the SCREW's colour hits the screw instead, which
    /// unscrews and frees the plate. Targets are encoded as ints: a part index (damage the part) or
    /// <see cref="ScrewBase"/> + part index (unscrew that part's screw).
    ///
    /// Destroying a TNT blasts every part whose layer is not above its own within <see cref="MonumentLayout.BlastRadius"/>,
    /// chaining into any other TNT it reaches.
    /// </summary>
    public sealed class MonumentModel
    {
        public const int ScrewBase = 1000;

        public readonly PartSpec[] Parts;
        public readonly int[] MaxHp;
        public readonly int[] Hp;
        public readonly bool[] Screw;
        public int AliveCount;

        private readonly float[] px;
        private readonly float[] py;

        public MonumentModel(LevelDefinition def)
        {
            Parts = def.parts.ToArray();
            int n = Parts.Length;
            MaxHp = new int[n];
            Hp = new int[n];
            Screw = new bool[n];
            px = new float[n];
            py = new float[n];
            for (int i = 0; i < n; i++)
            {
                MaxHp[i] = def.PartHp(i);
                Hp[i] = MaxHp[i];
                Screw[i] = Parts[i].hasScrew;
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
            Screw = (bool[])o.Screw.Clone();
            AliveCount = o.AliveCount;
        }

        public MonumentModel Clone() { return new MonumentModel(this); }

        public bool IsAlive(int i) { return Hp[i] > 0; }

        public static bool IsScrewCode(int code) { return code >= ScrewBase; }
        public static int PartOf(int code) { return code >= ScrewBase ? code - ScrewBase : code; }

        /// <summary>True while another still-alive part covers this one (gears inside a casing, the cassette behind its door...).</summary>
        public bool IsHidden(int i)
        {
            int mask = Parts[i].hiddenByMask;
            for (int j = 0; mask != 0 && j < Parts.Length; j++)
                if (((mask >> j) & 1) != 0 && Hp[j] > 0) return true;
            return false;
        }

        /// <summary>Alive, uncovered and not pinned by a screw: shots of its colour can damage it.</summary>
        public bool IsTargetable(int i) { return Hp[i] > 0 && !Screw[i] && !IsHidden(i); }

        /// <summary>Alive, uncovered and still pinned: a turret of the screw colour can unscrew it.</summary>
        public bool IsScrewTargetable(int i) { return Hp[i] > 0 && Screw[i] && !IsHidden(i); }

        /// <summary>
        /// The target a turret of this colour shoots next (lowest layer first; screws before plates; then lowest index),
        /// or -1. See the class comment for the encoding.
        /// </summary>
        public int FindTarget(int color)
        {
            int best = -1;
            int bestLayer = int.MaxValue;
            bool bestScrew = false;
            for (int i = 0; i < Parts.Length; i++)
            {
                if (Hp[i] <= 0 || IsHidden(i)) continue;
                int layer = Parts[i].layer;
                bool screwCandidate = Screw[i] && (int)Parts[i].screwColor == color;
                bool plateCandidate = !Screw[i] && (int)Parts[i].color == color;
                if (!screwCandidate && !plateCandidate) continue;
                bool better = best < 0 || layer < bestLayer || (layer == bestLayer && screwCandidate && !bestScrew);
                if (better)
                {
                    best = screwCandidate ? ScrewBase + i : i;
                    bestLayer = layer;
                    bestScrew = screwCandidate;
                }
            }
            return best;
        }

        /// <summary>Parts of this colour still alive plus screws of this colour still attached: used to retire useless turrets.</summary>
        public int AliveOfColor(int color)
        {
            int n = 0;
            for (int i = 0; i < Parts.Length; i++)
            {
                if (Hp[i] <= 0) continue;
                if ((int)Parts[i].color == color) n++;
                if (Screw[i] && (int)Parts[i].screwColor == color) n++;
            }
            return n;
        }

        /// <summary>A pinned plate of this colour exists (a turret of that colour is being deflected).</summary>
        public int FindDeflectedPlate(int color)
        {
            for (int i = 0; i < Parts.Length; i++)
                if (Hp[i] > 0 && Screw[i] && !IsHidden(i) && (int)Parts[i].color == color) return i;
            return -1;
        }

        /// <summary>Applies one shot at an encoded target. Unscrewed parts are appended to <paramref name="unscrewed"/>.</summary>
        public void HitTarget(int code, List<int> destroyed, List<int> detonated, List<int> unscrewed)
        {
            int part = PartOf(code);
            if (IsScrewCode(code))
            {
                if (Screw[part])
                {
                    Screw[part] = false;
                    if (unscrewed != null) unscrewed.Add(part);
                }
                return;
            }
            Hit(part, destroyed, detonated);
        }

        /// <summary>
        /// Clockwork piercing: after damaging <paramref name="part"/> the same shot also damages one part hidden directly
        /// behind it. Returns that part, or -1.
        /// </summary>
        public int PierceBehind(int part, List<int> destroyed, List<int> detonated)
        {
            for (int j = 0; j < Parts.Length; j++)
            {
                if (Hp[j] > 0 && ((Parts[j].hiddenByMask >> part) & 1) != 0)
                {
                    Hit(j, destroyed, detonated);
                    return j;
                }
            }
            return -1;
        }

        /// <summary>
        /// Deals one point of damage to a part. Appends every part destroyed (including blast victims) to
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
            for (int i = 0; i < Hp.Length; i++) key.Add((char)(Hp[i] + 1 + (Screw[i] ? 64 : 0)));
        }
    }
}
