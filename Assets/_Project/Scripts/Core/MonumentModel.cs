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
        /// <summary>Remaining screws per part: bit k set = screw k (colour <see cref="PartSpec.ScrewColorAt"/>) still attached.</summary>
        public readonly int[] ScrewMask;
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
            ScrewMask = new int[n];
            px = new float[n];
            py = new float[n];
            for (int i = 0; i < n; i++)
            {
                MaxHp[i] = def.PartHp(i);
                Hp[i] = MaxHp[i];
                Screw[i] = Parts[i].hasScrew;
                ScrewMask[i] = Parts[i].ScrewTotal == 0 ? 0 : ((1 << Parts[i].ScrewTotal) - 1);
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
            ScrewMask = (int[])o.ScrewMask.Clone();
            AliveCount = o.AliveCount;
        }

        public MonumentModel Clone() { return new MonumentModel(this); }

        public bool IsAlive(int i) { return Hp[i] > 0; }

        public static bool IsScrewCode(int code) { return code >= ScrewBase; }
        public static int PartOf(int code) { return code >= ScrewBase ? (code - ScrewBase) % 128 : code; }
        /// <summary>Which of a part's coupled screws a screw code points at (0..2).</summary>
        public static int ScrewIndexOf(int code) { return code >= ScrewBase ? (code - ScrewBase) / 128 : 0; }

        /// <summary>True while the part still has an attached screw of this colour.</summary>
        public bool HasScrewOfColor(int i, int color)
        {
            for (int k = 0; k < 3; k++) if (((ScrewMask[i] >> k) & 1) != 0 && (int)Parts[i].ScrewColorAt(k) == color) return true;
            return false;
        }

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
        /// The target a turret of this colour shoots next, or -1. Turrets UNSCREW first: any visible brass screw of
        /// their colour is hit before plates (lowest layer, then lowest index); otherwise the plate with the lowest
        /// layer and index. See the class comment for the encoding.
        /// </summary>
        public int FindTarget(int color)
        {
            int bestScrew = -1, bestScrewLayer = int.MaxValue;
            int bestPlate = -1, bestPlateLayer = int.MaxValue;
            for (int i = 0; i < Parts.Length; i++)
            {
                if (Hp[i] <= 0 || IsHidden(i)) continue;
                int layer = Parts[i].layer;
                if (Screw[i] && layer < bestScrewLayer)
                {
                    for (int k = 0; k < 3; k++)
                        if (((ScrewMask[i] >> k) & 1) != 0 && (int)Parts[i].ScrewColorAt(k) == color)
                        {
                            bestScrew = i + 128 * k;
                            bestScrewLayer = layer;
                            break;
                        }
                }
                if (!Screw[i] && (int)Parts[i].color == color && layer < bestPlateLayer)
                {
                    bestPlate = i;
                    bestPlateLayer = layer;
                }
            }
            if (bestScrew >= 0) return ScrewBase + bestScrew;
            return bestPlate;
        }

        /// <summary>Parts of this colour still alive plus screws of this colour still attached: used to retire useless turrets.</summary>
        public int AliveOfColor(int color)
        {
            int n = 0;
            for (int i = 0; i < Parts.Length; i++)
            {
                if (Hp[i] <= 0) continue;
                if ((int)Parts[i].color == color) n++;
                for (int k = 0; k < 3; k++) if (((ScrewMask[i] >> k) & 1) != 0 && (int)Parts[i].ScrewColorAt(k) == color) n++;
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
                int k = ScrewIndexOf(code);
                if (Screw[part] && ((ScrewMask[part] >> k) & 1) != 0)
                {
                    ScrewMask[part] &= ~(1 << k);
                    Screw[part] = ScrewMask[part] != 0;
                    if (unscrewed != null) unscrewed.Add(part + 128 * k);
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
            for (int i = 0; i < Hp.Length; i++) key.Add((char)(Hp[i] + 1 + ScrewMask[i] * 32));
        }
    }
}
