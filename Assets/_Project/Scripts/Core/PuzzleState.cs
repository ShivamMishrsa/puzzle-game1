using System.Collections.Generic;

namespace ColorGateBlast
{
    /// <summary>A turret parked on the belt: it has ammo but nothing it can currently hit.</summary>
    public sealed class TurretInfo
    {
        public int color;
        public int ammo;
    }

    /// <summary>
    /// Instantaneous abstraction of the whole game used by the level solver: tray + monument + the turrets still
    /// parked on the belt. A block leaving the tray spawns a turret with ammo = tile count that immediately shoots
    /// everything it can; leftover ammo keeps it parked (occupying a slot). A turret of a colour that is already
    /// parked MERGES into it (one slot, combined ammo) - the Mega-Turret rule.
    /// </summary>
    public sealed class PuzzleState
    {
        public readonly LevelDefinition Def;
        public readonly BoardModel Board;
        public readonly MonumentModel Monument;
        public readonly List<TurretInfo> Parked;

        public PuzzleState(LevelDefinition def)
        {
            Def = def;
            Board = new BoardModel(def);
            Monument = new MonumentModel(def);
            Parked = new List<TurretInfo>();
        }

        /// <summary>Snapshot of a LIVE game (used by the hint system). All arguments are cloned.</summary>
        public PuzzleState(LevelDefinition def, BoardModel board, MonumentModel monument, List<TurretInfo> parked)
        {
            Def = def;
            Board = board.Clone();
            Monument = monument.Clone();
            Parked = new List<TurretInfo>(parked.Count);
            for (int i = 0; i < parked.Count; i++)
            {
                TurretInfo t = new TurretInfo();
                t.color = parked[i].color;
                t.ammo = parked[i].ammo;
                Parked.Add(t);
            }
        }

        private PuzzleState(PuzzleState o)
        {
            Def = o.Def;
            Board = o.Board.Clone();
            Monument = o.Monument.Clone();
            Parked = new List<TurretInfo>(o.Parked.Count);
            for (int i = 0; i < o.Parked.Count; i++)
            {
                TurretInfo t = new TurretInfo();
                t.color = o.Parked[i].color;
                t.ammo = o.Parked[i].ammo;
                Parked.Add(t);
            }
        }

        public PuzzleState Clone() { return new PuzzleState(this); }

        public bool IsWon { get { return Board.RemainingMovable == 0 && Monument.AliveCount == 0; } }

        private bool HasParked(int color)
        {
            for (int i = 0; i < Parked.Count; i++) if (Parked[i].color == color) return true;
            return false;
        }

        /// <summary>A block of this colour may leave if there is a free slot, or if its turret would merge.</summary>
        public bool CanExit(int color)
        {
            return Def.UnlimitedTrack || Parked.Count < Def.maxTrackSlots || HasParked(color);
        }

        public void GenerateMoves(List<Move> moves)
        {
            bool[] allowed = new bool[4];
            for (int c = 0; c < 4; c++) allowed[c] = CanExit(c);
            Board.GenerateMoves(moves, allowed);
        }

        public void Apply(Move m)
        {
            if (!m.exit)
            {
                Board.Commit(m.block, m.to);
                return;
            }

            BlockInfo b = Board.Blocks[m.block];
            Board.ExitBlock(m.block, null);
            int color = (int)b.color;
            bool merged = false;
            for (int i = 0; i < Parked.Count; i++)
            {
                if (Parked[i].color == color) { Parked[i].ammo += b.Tiles; merged = true; break; }
            }
            if (!merged)
            {
                TurretInfo t = new TurretInfo();
                t.color = color;
                t.ammo = b.Tiles;
                Parked.Add(t);
            }
            Resolve();
        }

        /// <summary>Let every turret fire until nothing more can be hit; drop finished / useless turrets.</summary>
        private void Resolve()
        {
            bool progress = true;
            while (progress)
            {
                progress = false;
                for (int i = 0; i < Parked.Count; i++)
                {
                    TurretInfo t = Parked[i];
                    while (t.ammo > 0)
                    {
                        int target = Monument.FindTarget(t.color);
                        if (target < 0) break;
                        Monument.Hit(target, null, null);
                        t.ammo--;
                        progress = true;
                    }
                }
            }
            for (int i = Parked.Count - 1; i >= 0; i--)
            {
                TurretInfo t = Parked[i];
                if (t.ammo <= 0 || Monument.AliveOfColor(t.color) == 0) Parked.RemoveAt(i);
            }
        }

        public string Key()
        {
            List<char> k = new List<char>(Board.BlockCount + Monument.Hp.Length + 8);
            Board.AppendKey(k);
            Monument.AppendKey(k);
            List<int> p = new List<int>(Parked.Count);
            for (int i = 0; i < Parked.Count; i++) p.Add(Parked[i].color * 100 + Parked[i].ammo);
            p.Sort();
            for (int i = 0; i < p.Count; i++) k.Add((char)(p[i] + 1));
            return new string(k.ToArray());
        }
    }
}
