using System;
using System.Collections.Generic;

// Everything under Scripts/Core is free of UnityEngine so the exact same rules run in the game, in the
// Editor level solver and in standalone test harnesses. Kept C# 5 compatible on purpose.

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
        public static readonly GateDirection[] All = { GateDirection.Up, GateDirection.Down, GateDirection.Left, GateDirection.Right };

        public static Int2 Of(GateDirection d)
        {
            switch (d)
            {
                case GateDirection.Up: return new Int2(0, 1);
                case GateDirection.Down: return new Int2(0, -1);
                case GateDirection.Left: return new Int2(-1, 0);
                default: return new Int2(1, 0);
            }
        }

        public static GateDirection From(Int2 dir)
        {
            if (dir.y > 0) return GateDirection.Up;
            if (dir.y < 0) return GateDirection.Down;
            return dir.x < 0 ? GateDirection.Left : GateDirection.Right;
        }
    }

    /// <summary>One block on the tray as authored in a LevelData asset.</summary>
    [Serializable]
    public struct Placement
    {
        public BlockColor color;
        public BlockShape shape;
        public int width;
        public int height;
        /// <summary>L shapes only: 0..3 picks the missing corner.</summary>
        public int rotation;
        /// <summary>Grid cell of the shape's lower-left corner (x right, y up).</summary>
        public int x;
        public int y;
        /// <summary>Directional-arrow block: may only slide along this axis.</summary>
        public MoveAxis arrow;
        /// <summary>Welded in place: cannot be moved (gray blocks are always locked).</summary>
        public bool locked;
        /// <summary>Frosted ice shell: 0 none, 1 or 2 thaw hits left. Frozen blocks cannot be dragged.</summary>
        public int iceLayer;
        /// <summary>Dual-layer "peeled" block: when it exits it respawns as a 1x1 block of <see cref="innerColor"/>.</summary>
        public bool hasInner;
        public BlockColor innerColor;

        /// <summary>True while the placement still has to be given a position (x &lt; 0) - level authoring helper.</summary>
        public bool Unplaced { get { return x < 0 || y < 0; } }

        public Int2[] Cells()
        {
            List<Int2> c = new List<Int2>();
            if (shape == BlockShape.L)
            {
                c.Add(new Int2(0, 0)); c.Add(new Int2(1, 0)); c.Add(new Int2(0, 1)); c.Add(new Int2(1, 1));
                Int2 drop;
                switch (((rotation % 4) + 4) % 4)
                {
                    case 0: drop = new Int2(1, 1); break;
                    case 1: drop = new Int2(0, 1); break;
                    case 2: drop = new Int2(0, 0); break;
                    default: drop = new Int2(1, 0); break;
                }
                c.Remove(drop);
            }
            else
            {
                int w = Math.Max(1, width), h = Math.Max(1, height);
                for (int dy = 0; dy < h; dy++)
                    for (int dx = 0; dx < w; dx++)
                        c.Add(new Int2(dx, dy));
            }
            return c.ToArray();
        }
    }

    /// <summary>Runtime view of a Placement (cells resolved).</summary>
    public sealed class BlockInfo
    {
        public BlockColor color;
        public MoveAxis axis;
        public bool immovable;
        public Int2 origin;
        public Int2[] cells;
        public int iceLayer;
        public bool hasInner;
        public BlockColor innerColor;
        /// <summary>Index of the 1x1 block that respawns when this (dual-layer) block exits, or -1.</summary>
        public int companion = -1;
        /// <summary>True for the hidden 1x1 blocks created for dual-layer blocks (dormant until their parent exits).</summary>
        public bool isCompanion;
        public int companionOf = -1;

        public int Tiles { get { return cells.Length; } }
        public bool IsObstacle { get { return immovable || color == BlockColor.Gray; } }

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
    }

    /// <summary>A glowing coloured gate on the tray border.</summary>
    [Serializable]
    public struct GateSpec
    {
        public GateDirection direction;
        /// <summary>First cell index along the edge (x for Up/Down, y for Left/Right).</summary>
        public int start;
        public int length;
        public BlockColor color;
        /// <summary>Mechanical shutter: every block exit anywhere toggles all shutter gates open/closed.</summary>
        public bool isShutterGate;
        /// <summary>Initial shutter state (green LED = open, amber LED = closed).</summary>
        public bool startsOpen;
    }

    /// <summary>One destructible piece of the alarm clock.</summary>
    [Serializable]
    public struct PartSpec
    {
        public PartType type;
        /// <summary>Which copy (Bells 0/1, Gears 0..2, TNT 0..2).</summary>
        public int slot;
        /// <summary>Colour of the turrets that can damage it.</summary>
        public BlockColor color;
        public int hp;
        /// <summary>Disassembly stage (0 = outer shell). A detonating TNT also destroys every part within range whose layer is not above its own.</summary>
        public int layer;
        /// <summary>Bit i set = this part cannot be hit until the part at index i of the level's part list is destroyed.</summary>
        public int hiddenByMask;
    }

    /// <summary>Where each monument part sits (monument-local x/y, used for the TNT blast radius).</summary>
    public static class MonumentLayout
    {
        public const float BlastRadius = 1.5f;

        public static void Position(MonumentKind kind, PartType type, int slot, out float x, out float y)
        {
            if (kind == MonumentKind.CassettePlayer)
            {
                switch (type)
                {
                    case PartType.Button: x = slot == 0 ? -0.9f : 0.9f; y = 2.0f; return;
                    case PartType.Gear:
                        if (slot == 0) { x = -1.0f; y = 0.7f; }
                        else if (slot == 1) { x = 1.0f; y = 0.7f; }
                        else { x = 0.0f; y = 1.5f; }
                        return;
                    case PartType.Pcb: x = 0.0f; y = 0.5f; return;
                    case PartType.Tnt:
                        if (slot == 0) { x = 0.0f; y = 1.0f; } else { x = 1.2f; y = 0.5f; }
                        return;
                    default: x = 0.0f; y = 1.05f; return; // Casing, Door, Cassette
                }
            }
            Position(type, slot, out x, out y);
        }

        public static void Position(PartType type, int slot, out float x, out float y)
        {
            switch (type)
            {
                case PartType.Bell: x = slot == 0 ? -0.75f : 0.75f; y = 2.55f; return;
                case PartType.Gear:
                    if (slot == 0) { x = -0.35f; y = 1.35f; }
                    else if (slot == 1) { x = 0.4f; y = 1.2f; }
                    else { x = 0.0f; y = 1.95f; }
                    return;
                case PartType.Tnt:
                    if (slot == 0) { x = 0.0f; y = 0.7f; }
                    else if (slot == 1) { x = -0.95f; y = 1.7f; }
                    else { x = 0.95f; y = 1.7f; }
                    return;
                default: x = 0.0f; y = 1.5f; return; // Casing, Hand, Glass
            }
        }
    }

    /// <summary>Plain-data description of a level (no Unity types).</summary>
    [Serializable]
    public class LevelDefinition
    {
        public int levelID = 1;
        public string title = "";
        public LevelTier tier;
        public Int2 gridSize = new Int2(5, 5);
        public int moveLimit = 10;
        /// <summary>Max simultaneous turrets on the belt. 0 = unlimited.</summary>
        public int maxTrackSlots = 5;
        /// <summary>Solver's solution length (informational).</summary>
        public int parMoves;
        /// <summary>Multiplier applied to every part's HP.</summary>
        public float targetHpScale = 1f;
        /// <summary>Multiplier for how much debris (screws, springs, fragments) parts shed.</summary>
        public float debrisSpawnRate = 1f;
        public string hint = "";
        public string reward = "";
        /// <summary>Which 3D monument is disassembled (alarm clock for 1-10, cassette player for 11-20).</summary>
        public MonumentKind monument = MonumentKind.AlarmClock;

        public List<Placement> placements = new List<Placement>();
        public List<GateSpec> gates = new List<GateSpec>();
        public List<PartSpec> parts = new List<PartSpec>();

        public int GridWidth { get { return gridSize.x; } }
        public int GridHeight { get { return gridSize.y; } }
        public bool UnlimitedTrack { get { return maxTrackSlots <= 0; } }

        public int PartHp(int i) { return Math.Max(1, (int)Math.Ceiling(parts[i].hp * targetHpScale - 0.0001)); }

        /// <summary>
        /// Resolves placements into runtime blocks. Dual-layer blocks get a hidden 1x1 "companion" appended after all
        /// authored blocks; it stays dormant until its parent exits and then drops into the parent's grid slot.
        /// </summary>
        public BlockInfo[] BuildBlocks()
        {
            List<BlockInfo> r = new List<BlockInfo>();
            for (int i = 0; i < placements.Count; i++)
            {
                Placement p = placements[i];
                BlockInfo b = new BlockInfo();
                b.color = p.color;
                b.axis = p.arrow;
                b.immovable = p.locked || p.color == BlockColor.Gray;
                b.origin = new Int2(p.x, p.y);
                b.cells = p.Cells();
                b.iceLayer = b.immovable ? 0 : Math.Max(0, Math.Min(2, p.iceLayer));
                b.hasInner = p.hasInner && !b.immovable;
                b.innerColor = p.innerColor;
                r.Add(b);
            }
            int authored = r.Count;
            for (int i = 0; i < authored; i++)
            {
                if (!r[i].hasInner) continue;
                BlockInfo c = new BlockInfo();
                c.color = r[i].innerColor;
                c.cells = new Int2[] { new Int2(0, 0) };
                c.isCompanion = true;
                c.companionOf = i;
                r[i].companion = r.Count;
                r.Add(c);
            }
            return r.ToArray();
        }

        /// <summary>Total ammo the player can ever produce for a colour (tile count of its movable blocks).</summary>
        public int TotalAmmo(BlockColor color)
        {
            int n = 0;
            BlockInfo[] bl = BuildBlocks();
            for (int i = 0; i < bl.Length; i++) if (bl[i].color == color && !bl[i].IsObstacle) n += bl[i].Tiles;
            return n;
        }

        public int TotalHp(BlockColor color)
        {
            int n = 0;
            for (int i = 0; i < parts.Count; i++) if (parts[i].color == color) n += PartHp(i);
            return n;
        }

        /// <summary>Returns null when the level is structurally valid, otherwise a description of the problem.</summary>
        public string Validate()
        {
            int w = gridSize.x, h = gridSize.y;
            if (w < 2 || h < 2) return "grid too small";
            bool[] occ = new bool[w * h];
            BlockInfo[] blocks = BuildBlocks();
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i].isCompanion) continue; // dormant: has no position until its parent exits
                if (i < placements.Count && placements[i].Unplaced) return "block " + i + " is unplaced";
                for (int c = 0; c < blocks[i].cells.Length; c++)
                {
                    Int2 p = blocks[i].origin + blocks[i].cells[c];
                    if (p.x < 0 || p.y < 0 || p.x >= w || p.y >= h) return "block " + i + " out of grid";
                    if (occ[p.y * w + p.x]) return "blocks overlap at " + p;
                    occ[p.y * w + p.x] = true;
                }
            }
            for (int g = 0; g < gates.Count; g++)
            {
                GateSpec gt = gates[g];
                if (gt.color == BlockColor.Gray) return "gate " + g + " cannot be gray";
                int span = (gt.direction == GateDirection.Up || gt.direction == GateDirection.Down) ? w : h;
                if (gt.start < 0 || gt.length < 1 || gt.start + gt.length > span) return "gate " + g + " out of range";
            }
            if (parts.Count == 0) return "no monument parts";
            for (int i = 0; i < parts.Count; i++)
                if (parts[i].color == BlockColor.Gray) return "monument part " + i + " cannot be gray";
            return null;
        }
    }
}
