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
        /// <summary>Carries a brass key: leaving through a gate unlocks every padlocked gate with the same lockId.</summary>
        public bool hasBrassKey;
        public int lockId;
        /// <summary>The respawned inner core carries the brass key (opens lock <see cref="lockId"/>).</summary>
        public bool innerHasKey;
        /// <summary>Welded twin-linked pair: exactly two placements share the same non-zero id and slide as one rigid unit.</summary>
        public int linkGroup;
        /// <summary>Colour-shifting prism block: colours it cycles through (first = starting colour). Advances by one on every block exit.</summary>
        public BlockColor[] cycle;
        /// <summary>Crated mystery block: cannot leave a gate until an orthogonally adjacent block has left (then the crate bursts open).</summary>
        public bool crate;

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
        public bool hasKey;
        public int lockId;
        public bool innerKey;
        /// <summary>Index of the welded partner block, or -1.</summary>
        public int partner = -1;
        public int linkGroup;
        public bool crate;
        /// <summary>Prism colour cycle (null when this is an ordinary block).</summary>
        public BlockColor[] cycle;
        public bool IsPrism { get { return cycle != null && cycle.Length > 1; } }
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
        /// <summary>Sealed with chains and a brass padlock until the key block with the same lockId has left.</summary>
        public bool isPadlocked;
        public int lockId;
        /// <summary>Clockwork piercing gate: turrets spawned here pierce the outer shell AND the layer behind it.</summary>
        public bool isPiercingGate;
        /// <summary>Steam pressure gate: every N player moves the gate flips between Normal and Pressurized (an exit while pressurized gives the turret +2 bonus ammo). 0 = normal gate.</summary>
        public int steamInterval;
    }

    /// <summary>Typewriter carriage row: after every gate exit, blocks lying completely inside this row slide one tile (dir +1 = right, -1 = left, bouncing at the edge).</summary>
    [Serializable]
    public struct CarriageSpec
    {
        public int row;
        public int dir;
    }

    /// <summary>Electromagnetic repulsor coil: a block that comes to rest on, or orthogonally next to, this tile is pushed one tile away.</summary>
    [Serializable]
    public struct RepulsorSpec
    {
        public Int2 cell;
    }

    /// <summary>One-way ratchet turnstile: a block can only slide ONTO this tile while moving in <see cref="dir"/>.</summary>
    [Serializable]
    public struct TurnstileSpec
    {
        public Int2 cell;
        public GateDirection dir;
    }

    /// <summary>Electromagnetic floor pad: after a block comes to rest next to it (on the side opposite <see cref="pull"/>) it is pulled one tile onto the pad for free.</summary>
    [Serializable]
    public struct MagnetSpec
    {
        public Int2 cell;
        /// <summary>Direction the pad pulls blocks in.</summary>
        public GateDirection pull;
    }

    /// <summary>A pair of teleporter pads on the tray floor. Only 1x1 blocks warp; larger blocks roll over them.</summary>
    [Serializable]
    public struct PortalSpec
    {
        /// <summary>0 = Pair A (cyan), 1 = Pair B (violet).</summary>
        public int pair;
        public Int2 a;
        public Int2 b;
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
        /// <summary>Pinned down by a slotted brass screw: shots at the plate are deflected until a turret of <see cref="screwColor"/> unscrews it.</summary>
        public bool hasScrew;
        public BlockColor screwColor;
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
            if (kind == MonumentKind.RotaryPhone)
            {
                switch (type)
                {
                    case PartType.Handset: x = slot == 0 ? -1.3f : 1.3f; y = 2.4f; return;
                    case PartType.Cradle: x = 0.0f; y = 2.1f; return;
                    case PartType.Casing: x = 0.0f; y = 0.9f; return;
                    case PartType.Dial: x = 0.0f; y = 1.5f; return;
                    case PartType.Ringer: x = slot == 0 ? -0.7f : 0.7f; y = 1.35f; return;
                    case PartType.Coil: x = 0.0f; y = 1.0f; return;
                    case PartType.Tnt:
                        if (slot == 0) { x = 0.0f; y = 1.0f; } else { x = 1.1f; y = 0.8f; }
                        return;
                    default: x = 0.0f; y = 1.3f; return;
                }
            }
            if (kind == MonumentKind.EspressoMachine)
            {
                switch (type)
                {
                    case PartType.Boiler: x = 0.0f; y = slot == 0 ? 2.1f : 3.05f; return;
                    case PartType.Gauge: x = -0.55f; y = 1.75f; return;
                    case PartType.Lever: x = 1.3f; y = 2.1f; return;
                    case PartType.GroupHead: x = 0.0f; y = 0.95f; return;
                    case PartType.SightTube: x = -1.25f; y = 1.5f; return;
                    case PartType.DripGrid: x = 0.0f; y = 0.4f; return;
                    case PartType.Coil: x = 0.0f; y = 1.7f; return;
                    case PartType.Valve:
                        if (slot == 0) { x = -0.55f; y = 1.35f; }
                        else if (slot == 1) { x = 0.55f; y = 1.35f; }
                        else { x = 0.0f; y = 2.05f; }
                        return;
                    case PartType.Tnt:
                        if (slot == 0) { x = 0.0f; y = 1.5f; } else { x = 0.8f; y = 1.9f; }
                        return;
                    default: x = 0.0f; y = 1.5f; return;
                }
            }
            if (kind == MonumentKind.Orrery)
            {
                switch (type)
                {
                    case PartType.Ring: x = 0.0f; y = 3.0f - 0.4f * slot; return;
                    case PartType.Planet:
                        if (slot == 0) { x = -0.9f; y = 2.1f; } else if (slot == 1) { x = 0.9f; y = 2.4f; } else if (slot == 2) { x = 0.0f; y = 1.5f; } else { x = 0.5f; y = 2.9f; }
                        return;
                    case PartType.Cage: x = 0.0f; y = 2.05f; return;
                    case PartType.Tnt:
                        if (slot == 0) { x = 0.0f; y = 2.1f; } else { x = 0.45f; y = 1.9f; }
                        return;
                    default: x = 0.0f; y = 2.0f; return;
                }
            }
            if (kind == MonumentKind.Gramophone)
            {
                switch (type)
                {
                    case PartType.Horn: x = slot == 0 ? 0.0f : -0.4f; y = slot == 0 ? 2.5f : 3.1f; return;
                    case PartType.Bracket: x = 0.0f; y = 1.9f; return;
                    case PartType.Record: x = slot == 0 ? 0.0f : 0.5f; y = 1.3f; return;
                    case PartType.Governor: x = 1.1f; y = 1.0f; return;
                    case PartType.Cabinet: x = slot == 0 ? 0.0f : -0.8f; y = 0.55f; return;
                    case PartType.Mainspring: x = 0.5f; y = 0.8f; return;
                    case PartType.Tnt:
                        if (slot == 0) { x = 0.0f; y = 0.9f; } else { x = -0.9f; y = 0.8f; }
                        return;
                    default: x = 0.0f; y = 1.5f; return;
                }
            }
            if (kind == MonumentKind.Typewriter)
            {
                switch (type)
                {
                    case PartType.Hood: x = 0.0f; y = 2.4f; return;
                    case PartType.Platen: x = 0.0f; y = 2.95f; return;
                    case PartType.Lever: x = -1.45f; y = 2.1f; return;
                    case PartType.Keys: x = 0.0f; y = 0.75f; return;
                    case PartType.Typebar: x = 0.0f; y = 1.7f; return;
                    case PartType.Ribbon: x = slot == 0 ? -1.0f : 1.0f; y = 2.0f; return;
                    case PartType.Rack: x = 0.0f; y = 1.15f; return;
                    case PartType.Spring:
                        if (slot == 0) { x = -0.7f; y = 1.45f; } else if (slot == 1) { x = 0.7f; y = 1.45f; } else { x = 0.0f; y = 1.9f; }
                        return;
                    case PartType.Tnt:
                        if (slot == 0) { x = 0.0f; y = 1.3f; } else { x = 0.8f; y = 1.7f; }
                        return;
                    default: x = 0.0f; y = 1.5f; return;
                }
            }
            if (kind == MonumentKind.TlrCamera)
            {
                switch (type)
                {
                    case PartType.Body: x = 0.0f; y = 1.45f; return;
                    case PartType.Hood: x = 0.0f; y = 2.85f; return;
                    case PartType.Lens: x = 0.0f; y = slot == 0 ? 2.1f : 1.0f; return;
                    case PartType.Iris: x = 0.0f; y = 1.0f; return;
                    case PartType.Crank: x = 1.3f; y = 1.6f; return;
                    case PartType.Spool: x = slot == 0 ? -0.65f : 0.65f; y = 1.3f; return;
                    case PartType.ShutterGear: x = 0.0f; y = 1.7f; return;
                    case PartType.Tnt:
                        if (slot == 0) { x = 0.0f; y = 0.8f; } else { x = 0.8f; y = 1.1f; }
                        return;
                    default: x = 0.0f; y = 1.5f; return;
                }
            }
            if (kind == MonumentKind.PocketWatch)
            {
                switch (type)
                {
                    case PartType.Button: x = 0.0f; y = 3.0f; return;
                    case PartType.BalanceWheel: x = 0.0f; y = 1.4f; return;
                    case PartType.Gear:
                        if (slot == 0) { x = -0.7f; y = 1.0f; }
                        else if (slot == 1) { x = 0.6f; y = 1.9f; }
                        else { x = 0.0f; y = 1.0f; }
                        return;
                    case PartType.Ruby:
                        if (slot == 0) { x = -0.5f; y = 1.8f; }
                        else if (slot == 1) { x = 0.5f; y = 1.2f; }
                        else { x = 0.0f; y = 0.9f; }
                        return;
                    case PartType.Tnt:
                        if (slot == 0) { x = 0.0f; y = 1.5f; } else { x = 0.9f; y = 1.9f; }
                        return;
                    default: x = 0.0f; y = 1.5f; return; // Casing, Glass, Dial, Hand
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
        public List<PortalSpec> portals = new List<PortalSpec>();
        public List<MagnetSpec> magnets = new List<MagnetSpec>();
        public List<CarriageSpec> carriages = new List<CarriageSpec>();
        public List<TurnstileSpec> turnstiles = new List<TurnstileSpec>();
        public List<RepulsorSpec> repulsors = new List<RepulsorSpec>();
        /// <summary>Merged (Mega) turrets punch through the layer behind their target, like a clockwork piercing gate (levels 81+).</summary>
        public bool megaPierces;
        /// <summary>Countdown in seconds (0 = untimed).</summary>
        public int timeLimit;
        /// <summary>Level starts with the monument spin-hint (a 360 icon) visible.</summary>
        public bool showRotateHint;

        public int GridWidth { get { return gridSize.x; } }
        public int GridHeight { get { return gridSize.y; } }
        public bool UnlimitedTrack { get { return maxTrackSlots <= 0; } }

        public DifficultyClass Difficulty
        {
            get
            {
                if (levelID <= 20) return DifficultyClass.Easy;
                if (levelID <= 40) return DifficultyClass.Medium;
                return tier == LevelTier.Normal ? DifficultyClass.Hard : DifficultyClass.Extreme;
            }
        }

        /// <summary>Hard and extreme levels allow exactly the shortest solution - not a single spare move.</summary>
        public bool ExactMoves { get { return Difficulty >= DifficultyClass.Hard; } }

        /// <summary>Clock for this level: class base time (30 / 45 / 60 / 120 s), stretched so a long solution still fits (2 s per move + 10 s).</summary>
        public int ComputeTimeLimit(int par)
        {
            int baseSec;
            if (levelID >= 71) baseSec = 60; // chapter 8 onward: a flat minute for everything
            else switch (Difficulty)
            {
                case DifficultyClass.Easy: baseSec = 30; break;
                case DifficultyClass.Medium: baseSec = 45; break;
                case DifficultyClass.Hard: baseSec = 60; break;
                default: baseSec = 120; break;
            }
            int need = levelID >= 81 ? (int)Math.Ceiling(par * 2.5) + 10 : par * 2 + 10;
            int t = Math.Max(baseSec, need);
            return ((t + 4) / 5) * 5;
        }

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
                b.hasKey = p.hasBrassKey && !b.immovable;
                b.lockId = p.lockId;
                b.innerKey = p.innerHasKey && b.hasInner;
                b.linkGroup = b.immovable ? 0 : p.linkGroup;
                b.crate = p.crate && !b.immovable;
                b.cycle = (p.cycle != null && p.cycle.Length > 1 && !b.immovable) ? p.cycle : null;
                r.Add(b);
            }
            // resolve welded pairs
            for (int i = 0; i < r.Count; i++)
            {
                if (r[i].linkGroup == 0 || r[i].partner >= 0) continue;
                for (int j = i + 1; j < r.Count; j++)
                    if (r[j].linkGroup == r[i].linkGroup && r[j].partner < 0) { r[i].partner = j; r[j].partner = i; break; }
            }
            for (int i = 0; i < r.Count; i++) if (r[i].linkGroup != 0 && r[i].partner < 0) r[i].linkGroup = 0;
            int authored = r.Count;
            for (int i = 0; i < authored; i++)
            {
                if (!r[i].hasInner) continue;
                BlockInfo c = new BlockInfo();
                c.color = r[i].innerColor;
                c.cells = new Int2[] { new Int2(0, 0) };
                c.isCompanion = true;
                c.companionOf = i;
                c.hasKey = r[i].innerKey;
                c.lockId = r[i].lockId;
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
            for (int pi = 0; pi < portals.Count; pi++)
            {
                PortalSpec ps = portals[pi];
                Int2[] ends = { ps.a, ps.b };
                for (int e = 0; e < 2; e++)
                {
                    if (ends[e].x < 0 || ends[e].y < 0 || ends[e].x >= w || ends[e].y >= h) return "portal " + pi + " out of grid";
                    for (int bi = 0; bi < blocks.Length; bi++)
                        if (blocks[bi].IsObstacle && !blocks[bi].isCompanion)
                            for (int c = 0; c < blocks[bi].cells.Length; c++)
                                if (blocks[bi].origin + blocks[bi].cells[c] == ends[e]) return "portal " + pi + " is under an obstacle";
                }
                if (ps.a == ps.b) return "portal " + pi + " has identical ends";
            }
            for (int i = 0; i < placements.Count; i++)
            {
                if (placements[i].linkGroup == 0) continue;
                int n = 0;
                for (int j = 0; j < placements.Count; j++) if (placements[j].linkGroup == placements[i].linkGroup) n++;
                if (n != 2) return "link group " + placements[i].linkGroup + " must have exactly two blocks";
            }
            for (int ri = 0; ri < repulsors.Count; ri++)
                if (repulsors[ri].cell.x < 0 || repulsors[ri].cell.y < 0 || repulsors[ri].cell.x >= w || repulsors[ri].cell.y >= h) return "repulsor " + ri + " out of grid";
            for (int ti = 0; ti < turnstiles.Count; ti++)
                if (turnstiles[ti].cell.x < 0 || turnstiles[ti].cell.y < 0 || turnstiles[ti].cell.x >= w || turnstiles[ti].cell.y >= h) return "turnstile " + ti + " out of grid";
            for (int ci = 0; ci < carriages.Count; ci++)
                if (carriages[ci].row < 0 || carriages[ci].row >= h || carriages[ci].dir == 0) return "carriage " + ci + " is invalid";
            for (int mi = 0; mi < magnets.Count; mi++)
                if (magnets[mi].cell.x < 0 || magnets[mi].cell.y < 0 || magnets[mi].cell.x >= w || magnets[mi].cell.y >= h) return "magnet " + mi + " out of grid";
            if (parts.Count == 0) return "no monument parts";
            for (int i = 0; i < parts.Count; i++)
                if (parts[i].color == BlockColor.Gray) return "monument part " + i + " cannot be gray";
            return null;
        }
    }
}
