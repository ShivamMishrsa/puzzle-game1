using System.Collections.Generic;

namespace ColorGateBlast
{
    /// <summary>A turret waiting on the conveyor because it has ammo left but nothing it can currently hit.</summary>
    public sealed class TurretInfo
    {
        public int color;
        public int ammo;
    }

    /// <summary>
    /// Instantaneous abstraction of the whole game used by the level solver:
    /// board + target core + the turrets that are still parked on the track.
    /// Exiting a block spawns a turret with ammo = tile count, which immediately fires at every brick it can
    /// reach; leftover ammo keeps the turret parked (occupying a track slot) until other turrets expose
    /// bricks of its colour.
    /// </summary>
    public sealed class PuzzleState
    {
        public readonly LevelDefinition Def;
        public readonly BoardModel Board;
        public readonly CoreModel Core;
        public readonly List<TurretInfo> Parked;

        public PuzzleState(LevelDefinition def)
        {
            Def = def;
            Board = new BoardModel(def);
            Core = new CoreModel(def);
            Parked = new List<TurretInfo>();
        }

        private PuzzleState(PuzzleState o)
        {
            Def = o.Def;
            Board = o.Board.Clone();
            Core = o.Core.Clone();
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

        public bool TrackFull { get { return Parked.Count >= Def.maxTrackSlots; } }

        public bool IsWon { get { return Board.ExitedCount == Board.BlockCount && Core.RemainingLayers == 0; } }

        public void GenerateMoves(List<Move> moves)
        {
            Board.GenerateMoves(moves, !TrackFull);
        }

        public void Apply(Move m)
        {
            if (!m.exit)
            {
                Board.Commit(m.block, m.to);
                return;
            }

            BlockSpec spec = Def.blocks[m.block];
            Board.ExitBlock(m.block);
            TurretInfo t = new TurretInfo();
            t.color = (int)spec.color;
            t.ammo = spec.TileCount;
            Parked.Add(t);
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
                        int b = Core.FindHit(t.color);
                        if (b < 0) break;
                        Core.Hit(b);
                        t.ammo--;
                        progress = true;
                    }
                }
            }
            for (int i = Parked.Count - 1; i >= 0; i--)
            {
                TurretInfo t = Parked[i];
                if (t.ammo <= 0 || Core.RemainingOfColor(t.color) == 0) Parked.RemoveAt(i);
            }
        }

        public string Key()
        {
            List<char> k = new List<char>(Board.BlockCount + Core.Bricks.Length + 8);
            Board.AppendKey(k);
            Core.AppendKey(k);
            List<int> p = new List<int>(Parked.Count);
            for (int i = 0; i < Parked.Count; i++) p.Add(Parked[i].color * 100 + Parked[i].ammo);
            p.Sort();
            for (int i = 0; i < p.Count; i++) k.Add((char)(p[i] + 1));
            return new string(k.ToArray());
        }
    }
}
