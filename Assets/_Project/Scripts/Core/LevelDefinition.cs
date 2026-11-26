using System;
using System.Collections.Generic;

// NOTE: everything under Scripts/Core is deliberately free of UnityEngine so the exact same
// rules code runs in the game, in the Editor level solver, and in standalone test harnesses.
// It is also kept C# 5 compatible on purpose.

namespace ColorGateBlast
{
    [Serializable]
    public struct Int2
    {
        public int x;
        public int y;

        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Int2 operator +(Int2 a, Int2 b) { return new Int2(a.x + b.x, a.y + b.y); }
        public static Int2 operator -(Int2 a, Int2 b) { return new Int2(a.x - b.x, a.y - b.y); }
        public static bool operator ==(Int2 a, Int2 b) { return a.x == b.x && a.y == b.y; }
        public static bool operator !=(Int2 a, Int2 b) { return a.x != b.x || a.y != b.y; }
        public override bool Equals(object obj) { return obj is Int2 && this == (Int2)obj; }
        public override int GetHashCode() { return (x * 397) ^ y; }
        public override string ToString() { return "(" + x + "," + y + ")"; }
    }

    public static class Dirs
    {
        public static readonly Side[] AllSides = { Side.Top, Side.Right, Side.Bottom, Side.Left };

        public static Int2 Of(Side s)
        {
            switch (s)
            {
                case Side.Top: return new Int2(0, 1);
                case Side.Right: return new Int2(1, 0);
                case Side.Bottom: return new Int2(0, -1);
                default: return new Int2(-1, 0);
            }
        }
    }

    /// <summary>A polyomino block on the inner grid.</summary>
    [Serializable]
    public class BlockSpec
    {
        public BlockColor color;
        public BlockKind kind;
        public MoveAxis axis;
        /// <summary>Ice only: number of blocks that must have left the board before this block thaws.</summary>
        public int iceThawExits = 1;
        /// <summary>Grid position of the shape's (0,0) cell. x = right, y = up.</summary>
        public Int2 origin;
        /// <summary>Cells relative to origin, normalised so min x = min y = 0.</summary>
        public Int2[] cells;

        public int TileCount { get { return cells == null ? 0 : cells.Length; } }

        public int ExtentX()
        {
            int m = 0;
            for (int i = 0; i < cells.Length; i++) if (cells[i].x > m) m = cells[i].x;
            return m + 1;
        }

        public int ExtentY()
        {
            int m = 0;
            for (int i = 0; i < cells.Length; i++) if (cells[i].y > m) m = cells[i].y;
            return m + 1;
        }

        public BlockSpec Clone()
        {
            BlockSpec b = new BlockSpec();
            b.color = color;
            b.kind = kind;
            b.axis = axis;
            b.iceThawExits = iceThawExits;
            b.origin = origin;
            b.cells = (Int2[])cells.Clone();
            return b;
        }
    }

    /// <summary>A recessed coloured gate on the border of the grid.</summary>
    [Serializable]
    public class GateSpec
    {
        public Side side;
        /// <summary>First cell index along the side (x for Top/Bottom, y for Left/Right).</summary>
        public int start;
        public int length = 1;
        public BlockColor color;
        /// <summary>Closed until a Key block has left the board.</summary>
        public bool locked;

        public GateSpec Clone()
        {
            GateSpec g = new GateSpec();
            g.side = side;
            g.start = start;
            g.length = length;
            g.color = color;
            g.locked = locked;
            return g;
        }
    }

    /// <summary>One cell of the Target Core. layers[0] is the outer shield, later layers hide beneath it.</summary>
    [Serializable]
    public class BrickSpec
    {
        public int x;
        public int y;
        public BlockColor[] layers;

        public BrickSpec Clone()
        {
            BrickSpec b = new BrickSpec();
            b.x = x;
            b.y = y;
            b.layers = (BlockColor[])layers.Clone();
            return b;
        }
    }

    /// <summary>Plain-data description of a complete level (no Unity types).</summary>
    [Serializable]
    public class LevelDefinition
    {
        public int levelNumber = 1;
        public Difficulty difficulty;
        public LevelTier tier;
        public int seed;

        public int gridWidth = 5;
        public int gridHeight = 5;
        public int coreWidth = 3;
        public int coreHeight = 3;
        public int maxTrackSlots = 5;
        public int colorCount = 2;

        /// <summary>Moves the player may spend (solver par + slack).</summary>
        public int moveLimit = 10;
        /// <summary>Solver's solution length (informational).</summary>
        public int parMoves = 5;

        public List<BlockSpec> blocks = new List<BlockSpec>();
        public List<GateSpec> gates = new List<GateSpec>();
        public List<BrickSpec> bricks = new List<BrickSpec>();

        public int TotalBlockTiles()
        {
            int n = 0;
            for (int i = 0; i < blocks.Count; i++) n += blocks[i].TileCount;
            return n;
        }

        public int TotalBrickLayers()
        {
            int n = 0;
            for (int i = 0; i < bricks.Count; i++) n += bricks[i].layers.Length;
            return n;
        }

        public LevelDefinition Clone()
        {
            LevelDefinition d = new LevelDefinition();
            d.levelNumber = levelNumber;
            d.difficulty = difficulty;
            d.tier = tier;
            d.seed = seed;
            d.gridWidth = gridWidth;
            d.gridHeight = gridHeight;
            d.coreWidth = coreWidth;
            d.coreHeight = coreHeight;
            d.maxTrackSlots = maxTrackSlots;
            d.colorCount = colorCount;
            d.moveLimit = moveLimit;
            d.parMoves = parMoves;
            for (int i = 0; i < blocks.Count; i++) d.blocks.Add(blocks[i].Clone());
            for (int i = 0; i < gates.Count; i++) d.gates.Add(gates[i].Clone());
            for (int i = 0; i < bricks.Count; i++) d.bricks.Add(bricks[i].Clone());
            return d;
        }

        /// <summary>Returns null when the level is structurally valid, otherwise a description of the problem.</summary>
        public string Validate()
        {
            if (gridWidth < 2 || gridHeight < 2) return "grid too small";
            bool[] occ = new bool[gridWidth * gridHeight];
            for (int i = 0; i < blocks.Count; i++)
            {
                BlockSpec b = blocks[i];
                if (b.cells == null || b.cells.Length == 0) return "block " + i + " has no cells";
                for (int c = 0; c < b.cells.Length; c++)
                {
                    Int2 p = b.origin + b.cells[c];
                    if (p.x < 0 || p.y < 0 || p.x >= gridWidth || p.y >= gridHeight) return "block " + i + " out of grid";
                    if (occ[p.y * gridWidth + p.x]) return "blocks overlap at " + p;
                    occ[p.y * gridWidth + p.x] = true;
                }
            }
            for (int g = 0; g < gates.Count; g++)
            {
                GateSpec gt = gates[g];
                int span = (gt.side == Side.Top || gt.side == Side.Bottom) ? gridWidth : gridHeight;
                if (gt.start < 0 || gt.length < 1 || gt.start + gt.length > span) return "gate " + g + " out of range";
            }
            bool[] bocc = new bool[coreWidth * coreHeight];
            for (int i = 0; i < bricks.Count; i++)
            {
                BrickSpec b = bricks[i];
                if (b.x < 0 || b.y < 0 || b.x >= coreWidth || b.y >= coreHeight) return "brick " + i + " out of core";
                if (bocc[b.y * coreWidth + b.x]) return "bricks overlap";
                bocc[b.y * coreWidth + b.x] = true;
                if (b.layers == null || b.layers.Length == 0) return "brick " + i + " has no layers";
            }
            return null;
        }
    }
}
