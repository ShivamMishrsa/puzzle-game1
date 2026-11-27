using System;
using System.Collections.Generic;

namespace ColorGateBlast
{
    public enum StepResult
    {
        Blocked,
        Moved,
        /// <summary>The step pushes the block into a matching open gate and the whole block can leave.</summary>
        Exit
    }

    /// <summary>One player action: slide a block to a new rest position, or push it out through a gate.</summary>
    public struct Move
    {
        public int block;
        public Int2 to;
        public bool exit;
        public GateDirection dir;
        /// <summary>Exit moves only: the gate is a Clockwork Piercing Gate.</summary>
        public bool piercing;
    }

    /// <summary>Side effects of a block leaving the tray (used to drive FX / audio / UI).</summary>
    public sealed class ExitEffects
    {
        /// <summary>Iced neighbours that lost one ice layer.</summary>
        public readonly List<int> thawed = new List<int>();
        /// <summary>Companion block (1x1 inner colour) that dropped into the vacated slot, or -1.</summary>
        public int spawned = -1;
        public Int2 spawnCell;
        public bool shutterToggled;
        /// <summary>Lock id opened by a leaving brass key block, or -1.</summary>
        public int unlockedLock = -1;
        /// <summary>Index of the gate the block left through, or -1.</summary>
        public int gateIndex = -1;
        public bool piercing;
        /// <summary>Bonus ammo from a Pressurized steam gate (+2), else 0.</summary>
        public int bonusAmmo;
        /// <summary>Crated blocks that burst open because a neighbour left.</summary>
        public readonly List<int> revealed = new List<int>();
        /// <summary>Blocks that a carriage row slid by one tile (and where they came from).</summary>
        public readonly List<int> shifted = new List<int>();
        public readonly List<Int2> shiftedFrom = new List<Int2>();
        /// <summary>Index of the welded partner that was freed by this exit, or -1.</summary>
        public int linkBroken = -1;

        public void Clear()
        {
            bonusAmmo = 0;
            linkBroken = -1;
            revealed.Clear();
            shifted.Clear();
            shiftedFrom.Clear();
            thawed.Clear();
            spawned = -1;
            shutterToggled = false;
            unlockedLock = -1;
            gateIndex = -1;
            piercing = false;
        }
    }

    /// <summary>
    /// Pure rules model of the recess tray. x = right, y = up (away from the player). Gates live just outside
    /// the tray edge. A block may leave only through an OPEN gate of its own colour whose span covers every column
    /// (Up/Down) or row (Left/Right) it occupies. Leaving is atomic: the whole path out must be free.
    ///
    /// Mechanics:
    ///  * Ice: a block with iceLayer &gt; 0 is frozen. Whenever a block exits, every orthogonally adjacent iced block
    ///    loses one layer.
    ///  * Dual layer: a block with an inner colour leaves a dormant 1x1 "companion" that drops into the vacated slot.
    ///  * Shutter gates: every exit toggles all shutter gates (open &lt;-&gt; closed).
    ///  * Brass key / padlock: a key block leaving opens every padlocked gate with its lockId.
    ///  * Portals: a 1x1 block sliding onto a pad warps to the partner pad (if that tile is free) and keeps sliding.
    ///  * Directional rails: blocks with an axis may only slide along it.
    /// Gray (and locked) blocks never move, never leave and do not need to be cleared.
    /// </summary>
    public sealed class BoardModel
    {
        public readonly int Width;
        public readonly int Height;
        public readonly LevelDefinition Def;
        public readonly BlockInfo[] Blocks;
        public readonly Int2[] Pos;
        public readonly bool[] Exited;
        public readonly bool[] Dormant;
        public readonly int[] Ice;
        /// <summary>True while a welded pair is still joined (both halves share the flag).</summary>
        public readonly bool[] Linked;
        /// <summary>True while a mystery crate is still closed.</summary>
        public readonly bool[] Crated;
        /// <summary>Current direction of every carriage row (+1 right / -1 left).</summary>
        public readonly int[] CarriageDir;
        /// <summary>Player moves made so far (drives the Steam Pressure Gate cycle).</summary>
        public int Moves;
        public int ExitedCount;
        /// <summary>Number of exits so far; its parity flips every shutter gate.</summary>
        public int Toggles;
        /// <summary>Bit n set = the brass key for lock n has left the tray.</summary>
        public int UnlockedMask;

        private readonly int[] occ;
        private readonly bool hasShutter;
        private readonly int steamPeriod;
        private readonly Int2[] partner; // per cell: the partner pad cell, or (-1,-1)

        public BoardModel(LevelDefinition def)
        {
            Def = def;
            Width = def.GridWidth;
            Height = def.GridHeight;
            for (int g = 0; g < def.gates.Count; g++)
            {
                if (def.gates[g].isShutterGate) hasShutter = true;
                if (def.gates[g].steamInterval > 0 && def.gates[g].steamInterval * 2 > steamPeriod) steamPeriod = def.gates[g].steamInterval * 2;
            }
            Blocks = def.BuildBlocks();
            int n = Blocks.Length;
            Pos = new Int2[n];
            Exited = new bool[n];
            Dormant = new bool[n];
            Ice = new int[n];
            Linked = new bool[n];
            for (int i = 0; i < n; i++) Linked[i] = Blocks[i].partner >= 0;
            Crated = new bool[n];
            for (int i = 0; i < n; i++) Crated[i] = Blocks[i].crate;
            CarriageDir = new int[def.carriages.Count];
            for (int k = 0; k < CarriageDir.Length; k++) CarriageDir[k] = def.carriages[k].dir >= 0 ? 1 : -1;
            occ = new int[Width * Height];
            partner = BuildPartners(def);
            for (int i = 0; i < occ.Length; i++) occ[i] = -1;
            for (int i = 0; i < n; i++)
            {
                Ice[i] = Blocks[i].iceLayer;
                if (Blocks[i].isCompanion) { Dormant[i] = true; continue; }
                Pos[i] = Blocks[i].origin;
                Stamp(i, Pos[i], i);
            }
        }

        private BoardModel(BoardModel o)
        {
            Def = o.Def;
            Width = o.Width;
            Height = o.Height;
            Blocks = o.Blocks;
            Pos = (Int2[])o.Pos.Clone();
            Exited = (bool[])o.Exited.Clone();
            Dormant = (bool[])o.Dormant.Clone();
            Ice = (int[])o.Ice.Clone();
            Linked = (bool[])o.Linked.Clone();
            Crated = (bool[])o.Crated.Clone();
            CarriageDir = (int[])o.CarriageDir.Clone();
            Moves = o.Moves;
            steamPeriod = o.steamPeriod;
            occ = (int[])o.occ.Clone();
            ExitedCount = o.ExitedCount;
            Toggles = o.Toggles;
            UnlockedMask = o.UnlockedMask;
            hasShutter = o.hasShutter;
            partner = o.partner;
        }

        private Int2[] BuildPartners(LevelDefinition def)
        {
            Int2[] p = new Int2[Width * Height];
            for (int i = 0; i < p.Length; i++) p[i] = new Int2(-1, -1);
            for (int k = 0; k < def.portals.Count; k++)
            {
                PortalSpec ps = def.portals[k];
                if (ps.a.x >= 0 && ps.a.x < Width && ps.a.y >= 0 && ps.a.y < Height && ps.b.x >= 0 && ps.b.x < Width && ps.b.y >= 0 && ps.b.y < Height)
                {
                    p[ps.a.y * Width + ps.a.x] = ps.b;
                    p[ps.b.y * Width + ps.b.x] = ps.a;
                }
            }
            return p;
        }

        public BoardModel Clone() { return new BoardModel(this); }

        public int BlockCount { get { return Blocks.Length; } }

        /// <summary>Blocks the player still has to remove (obstacles excluded; dormant companions included).</summary>
        public int RemainingMovable
        {
            get
            {
                int n = 0;
                for (int i = 0; i < Blocks.Length; i++) if (!Blocks[i].IsObstacle && !Exited[i]) n++;
                return n;
            }
        }

        public bool InGrid(Int2 p) { return p.x >= 0 && p.y >= 0 && p.x < Width && p.y < Height; }

        public bool IsFrozen(int i) { return Ice[i] > 0; }

        /// <summary>True when stepping block i from <paramref name="origin"/> in <paramref name="dir"/> would push it onto a one-way turnstile from the wrong side.</summary>
        public bool TurnstileBlocks(int i, Int2 origin, Int2 dir)
        {
            Int2[] cells = Blocks[i].cells;
            for (int c = 0; c < cells.Length; c++)
            {
                Int2 np = origin + dir + cells[c];
                for (int t = 0; t < Def.turnstiles.Count; t++)
                {
                    TurnstileSpec ts = Def.turnstiles[t];
                    if (ts.cell != np) continue;
                    bool already = false;
                    for (int c2 = 0; c2 < cells.Length; c2++) if (origin + cells[c2] == np) { already = true; break; }
                    if (!already && Dirs.Of(ts.dir) != dir) return true;
                }
            }
            return false;
        }

        /// <summary>Current colour of block i (prism blocks cycle once per exit).</summary>
        public BlockColor ColorOf(int i)
        {
            BlockInfo b = Blocks[i];
            if (b.cycle == null || b.cycle.Length < 2) return b.color;
            return b.cycle[Toggles % b.cycle.Length];
        }

        /// <summary>Colour block i will have after the next <paramref name="steps"/> exits.</summary>
        public BlockColor ColorAfter(int i, int steps)
        {
            BlockInfo b = Blocks[i];
            if (b.cycle == null || b.cycle.Length < 2) return b.color;
            return b.cycle[(Toggles + steps) % b.cycle.Length];
        }

        public bool SteamPressurized(GateSpec g) { return g.steamInterval > 0 && ((Moves / g.steamInterval) & 1) == 1; }

        /// <summary>Call once per completed player move (slide or exit).</summary>
        public void CountMove() { Moves++; }

        /// <summary>Block index occupying the cell, or -1.</summary>
        public int At(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return -1;
            return occ[y * Width + x];
        }

        public bool IsPortal(Int2 p) { return InGrid(p) && partner[p.y * Width + p.x].x >= 0; }

        public Int2 PortalPartner(Int2 p) { return partner[p.y * Width + p.x]; }

        public bool IsGateOpen(GateSpec g)
        {
            if (g.isShutterGate && g.startsOpen == ((Toggles & 1) == 1)) return false;
            if (g.isPadlocked && ((UnlockedMask >> g.lockId) & 1) == 0) return false;
            return true;
        }

        public bool IsGateOpen(int gateIndex) { return IsGateOpen(Def.gates[gateIndex]); }

        private void Stamp(int i, Int2 origin, int value)
        {
            Int2[] cells = Blocks[i].cells;
            for (int c = 0; c < cells.Length; c++)
            {
                Int2 p = origin + cells[c];
                occ[p.y * Width + p.x] = value;
            }
        }

        /// <summary>Gate index whose lane contains this out-of-grid cell for a block of this colour (open or not), or -1.</summary>
        private int GateOfOutsideCell(Int2 p, BlockColor color, bool onlyOpen)
        {
            GateDirection dir;
            int lane;
            if (p.x >= 0 && p.x < Width)
            {
                if (p.y >= Height) dir = GateDirection.Up;
                else if (p.y < 0) dir = GateDirection.Down;
                else return -1;
                lane = p.x;
            }
            else if (p.y >= 0 && p.y < Height)
            {
                dir = p.x >= Width ? GateDirection.Right : GateDirection.Left;
                lane = p.y;
            }
            else
            {
                return -1; // diagonal corner region is never reachable
            }

            List<GateSpec> gates = Def.gates;
            for (int g = 0; g < gates.Count; g++)
            {
                GateSpec gt = gates[g];
                if (gt.direction == dir && gt.color == color && lane >= gt.start && lane < gt.start + gt.length && (!onlyOpen || IsGateOpen(gt)))
                    return g;
            }
            return -1;
        }

        private bool CheckPlacement(int i, Int2 origin, out int outsideCount) { return CheckPlacement(i, origin, -1, out outsideCount); }

        private bool CheckPlacement(int i, Int2 origin, int ignore, out int outsideCount)
        {
            outsideCount = 0;
            BlockInfo b = Blocks[i];
            Int2[] cells = b.cells;
            BlockColor col = ColorOf(i);
            for (int c = 0; c < cells.Length; c++)
            {
                Int2 p = origin + cells[c];
                if (InGrid(p))
                {
                    int v = occ[p.y * Width + p.x];
                    if (v != -1 && v != i && v != ignore) return false;
                }
                else
                {
                    if (GateOfOutsideCell(p, col, true) < 0) return false;
                    outsideCount++;
                }
            }
            return true;
        }

        /// <summary>Where a block ends up after stepping one cell from <paramref name="origin"/>: the next cell, or the partner pad for a 1x1 stepping onto a portal.</summary>
        public Int2 Land(int i, Int2 origin, Int2 dir)
        {
            Int2 o = origin + dir;
            if (Blocks[i].cells.Length == 1 && IsPortal(o) && !Linked[i]) return PortalPartner(o);
            return o;
        }

        /// <summary>
        /// Result of pushing block <paramref name="i"/> one cell from the hypothetical origin. A welded block drags its partner
        /// along: both halves need a free cell; the pushed half may leave through its own gate while the partner just steps forward.
        /// </summary>
        public StepResult Probe(int i, Int2 origin, Int2 dir)
        {
            if (!Linked[i]) return ProbeOne(i, origin, dir, -1);
            int j = Blocks[i].partner;
            if (Exited[j] || Ice[j] > 0) return StepResult.Blocked;
            StepResult ri = ProbeOne(i, origin, dir, j);
            if (ri == StepResult.Blocked) return ri;
            Int2 oj = Pos[j] + (origin - Pos[i]);
            StepResult rj = ProbeOne(j, oj, dir, i);
            return rj == StepResult.Moved ? ri : StepResult.Blocked;
        }

        private StepResult ProbeOne(int i, Int2 origin, Int2 dir, int ignore)
        {
            BlockInfo b = Blocks[i];
            if (Exited[i] || Dormant[i] || b.IsObstacle || Ice[i] > 0) return StepResult.Blocked;
            if (dir.x != 0 && b.axis == MoveAxis.Vertical) return StepResult.Blocked;
            if (dir.y != 0 && b.axis == MoveAxis.Horizontal) return StepResult.Blocked;

            Int2 o = origin + dir;
            int outCount;
            if (!CheckPlacement(i, o, ignore, out outCount)) return StepResult.Blocked;
            if (outCount > 0 && Crated[i]) return StepResult.Blocked; // a closed crate cannot leave through a gate
            if (Def.turnstiles.Count > 0 && TurnstileBlocks(i, origin, dir)) return StepResult.Blocked;
            if (outCount == 0)
            {
                // a 1x1 block stepping onto a pad warps to the partner - only if the exit pad is free
                if (b.cells.Length == 1 && IsPortal(o) && !Linked[i])
                {
                    Int2 q = PortalPartner(o);
                    int v = occ[q.y * Width + q.x];
                    if (v != -1 && v != i) return StepResult.Blocked;
                }
                return StepResult.Moved;
            }

            // The block is entering a gate: it leaves only if the entire remaining path is clear.
            int tiles = b.cells.Length;
            while (outCount < tiles)
            {
                o = o + dir;
                if (!CheckPlacement(i, o, ignore, out outCount)) return StepResult.Blocked;
            }
            return StepResult.Exit;
        }

        public StepResult Probe(int i, Int2 dir) { return Probe(i, Pos[i], dir); }

        /// <summary>Index of the gate block <paramref name="i"/> would leave through when pushed from <paramref name="origin"/> in <paramref name="dir"/> (call only when Probe says Exit).</summary>
        public int ExitGateIndex(int i, Int2 origin, Int2 dir)
        {
            BlockInfo b = Blocks[i];
            Int2 o = origin + dir;
            for (int step = 0; step < 16; step++)
            {
                for (int c = 0; c < b.cells.Length; c++)
                {
                    Int2 p = o + b.cells[c];
                    if (!InGrid(p))
                    {
                        int g = GateOfOutsideCell(p, ColorOf(i), true);
                        if (g >= 0) return g;
                    }
                }
                o = o + dir;
            }
            return -1;
        }

        public void Commit(int i, Int2 newOrigin)
        {
            if (Linked[i])
            {
                int j = Blocks[i].partner;
                Int2 delta = newOrigin - Pos[i];
                Stamp(i, Pos[i], -1);
                Stamp(j, Pos[j], -1);
                Pos[i] = newOrigin;
                Pos[j] = Pos[j] + delta;
                Stamp(i, Pos[i], i);
                Stamp(j, Pos[j], j);
                return;
            }
            Stamp(i, Pos[i], -1);
            Pos[i] = newOrigin;
            Stamp(i, newOrigin, i);
        }

        /// <summary>Removes block <paramref name="i"/> from the tray (exit direction unknown: it is searched from its current cell).</summary>
        public void ExitBlock(int i, ExitEffects fx)
        {
            GateDirection found = GateDirection.Up;
            for (int d = 0; d < 4; d++)
            {
                if (Probe(i, Pos[i], Dirs.Of(Dirs.All[d])) == StepResult.Exit) { found = Dirs.All[d]; break; }
            }
            ExitBlock(i, fx, found, Pos[i], Pos[i]);
        }

        /// <summary>
        /// Removes block <paramref name="i"/> from the tray and applies the side effects.
        /// <paramref name="startOrigin"/> is where the block was RESTING when its exit move began: iced neighbours are
        /// measured from there (so "a block beside the ice leaves" thaws it) and a dual-layer block's companion drops
        /// into that slot. <paramref name="exitOrigin"/> is the last in-grid position before leaving (used to find the gate).
        /// </summary>
        public void ExitBlock(int i, ExitEffects fx, GateDirection exitDir, Int2 startOrigin, Int2 exitOrigin)
        {
            ExitCore(i, fx, exitDir, startOrigin, exitOrigin, true);
        }

        /// <summary>Removes a block with a tool (pliers): no gate, no steam bonus, a welded partner is simply freed in place.</summary>
        public void RemoveByTool(int i, ExitEffects fx)
        {
            ExitCore(i, fx, GateDirection.Up, Pos[i], Pos[i], false);
        }

        private void ExitCore(int i, ExitEffects fx, GateDirection exitDir, Int2 startOrigin, Int2 exitOrigin, bool viaGate)
        {
            if (fx != null) fx.Clear();
            BlockInfo b = Blocks[i];

            int exitGate = viaGate ? ExitGateIndex(i, exitOrigin, Dirs.Of(exitDir)) : -1;
            if (fx != null)
            {
                fx.gateIndex = exitGate;
                fx.piercing = exitGate >= 0 && Def.gates[exitGate].isPiercingGate;
                if (exitGate >= 0 && SteamPressurized(Def.gates[exitGate])) fx.bonusAmmo = 2;
            }

            // a welded partner steps forward with the leaving half and is freed
            if (Linked[i])
            {
                int j = b.partner;
                Linked[i] = false;
                Linked[j] = false;
                if (viaGate)
                {
                    Int2 disp = (exitOrigin - Pos[i]) + Dirs.Of(exitDir);
                    Stamp(j, Pos[j], -1);
                    Pos[j] = Pos[j] + disp;
                    Stamp(j, Pos[j], j);
                }
                if (fx != null) fx.linkBroken = j;
            }

            // orthogonal neighbours of the block at the slot it started from
            List<int> neighbours = new List<int>();
            Int2[] cells = b.cells;
            for (int c = 0; c < cells.Length; c++)
            {
                Int2 p = startOrigin + cells[c];
                for (int d = 0; d < 4; d++)
                {
                    Int2 q = p + Dirs.Of(Dirs.All[d]);
                    int j = At(q.x, q.y);
                    if (j >= 0 && j != i && !neighbours.Contains(j)) neighbours.Add(j);
                }
            }

            Stamp(i, Pos[i], -1);
            Exited[i] = true;
            ExitedCount++;
            Toggles++;
            if (fx != null) fx.shutterToggled = hasShutter;

            if (b.hasKey)
            {
                UnlockedMask |= 1 << b.lockId;
                if (fx != null) fx.unlockedLock = b.lockId;
            }

            for (int k = 0; k < neighbours.Count; k++)
            {
                int j = neighbours[k];
                if (Crated[j]) { Crated[j] = false; if (fx != null) fx.revealed.Add(j); }
                if (Ice[j] > 0)
                {
                    Ice[j]--;
                    if (fx != null) fx.thawed.Add(j);
                }
            }

            if (b.companion >= 0)
            {
                int c = b.companion;
                Dormant[c] = false;
                Pos[c] = startOrigin + b.cells[0];
                Stamp(c, Pos[c], c);
                if (fx != null) { fx.spawned = c; fx.spawnCell = Pos[c]; }
            }

            if (viaGate) ApplyCarriages(fx);
        }

        /// <summary>Typewriter carriages: every gate exit slides the blocks lying inside a carriage row by one tile (bouncing at the walls).</summary>
        public void ApplyCarriages(ExitEffects fx)
        {
            for (int k = 0; k < Def.carriages.Count; k++)
            {
                int row = Def.carriages[k].row;
                List<int> eligible = new List<int>();
                for (int i = 0; i < Blocks.Length; i++)
                {
                    if (Exited[i] || Dormant[i] || Blocks[i].IsObstacle || Ice[i] > 0 || Linked[i]) continue;
                    bool inside = true;
                    for (int c = 0; c < Blocks[i].cells.Length; c++) if (Pos[i].y + Blocks[i].cells[c].y != row) { inside = false; break; }
                    if (inside) eligible.Add(i);
                }
                if (eligible.Count == 0) continue;
                int dir = CarriageDir[k];
                // bounce: if any block would be pushed through the side wall the row reverses first
                for (int e = 0; e < eligible.Count; e++)
                {
                    int i = eligible[e];
                    for (int c = 0; c < Blocks[i].cells.Length; c++)
                    {
                        int nx = Pos[i].x + Blocks[i].cells[c].x + dir;
                        if (nx < 0 || nx >= Width) { dir = -dir; e = eligible.Count; break; }
                    }
                }
                CarriageDir[k] = dir;
                // move the leading blocks first so a train of blocks can move together
                eligible.Sort(delegate (int a, int b) { return dir > 0 ? Pos[b].x.CompareTo(Pos[a].x) : Pos[a].x.CompareTo(Pos[b].x); });
                for (int e = 0; e < eligible.Count; e++)
                {
                    int i = eligible[e];
                    Int2 target = Pos[i] + new Int2(dir, 0);
                    int oc;
                    if (!CheckPlacement(i, target, out oc) || oc != 0) continue;
                    Int2 dest = target;
                    if (Blocks[i].cells.Length == 1 && IsPortal(target))
                    {
                        Int2 q = PortalPartner(target);
                        if (occ[q.y * Width + q.x] != -1) continue;
                        dest = q;
                    }
                    Int2 from = Pos[i];
                    Commit(i, dest);
                    if (fx != null) { fx.shifted.Add(i); fx.shiftedFrom.Add(from); }
                }
            }
        }

        public bool HasShutter() { return hasShutter; }

        /// <summary>
        /// Repulsor coils: after a slide, a block resting on or orthogonally beside a repulsor is shoved one tile away (a welded pair moves
        /// together). Returns true when the block moved.
        /// </summary>
        public bool RepelAfterSlide(int i, Int2 moveDir)
        {
            if (Def.repulsors.Count == 0) return false;
            BlockInfo b = Blocks[i];
            if (Exited[i] || Dormant[i] || b.IsObstacle || Ice[i] > 0) return false;
            Int2 push = new Int2(0, 0);
            bool found = false;
            for (int r = 0; r < Def.repulsors.Count && !found; r++)
            {
                Int2 rc = Def.repulsors[r].cell;
                for (int c = 0; c < b.cells.Length && !found; c++)
                {
                    Int2 p = Pos[i] + b.cells[c];
                    if (p == rc) { push = moveDir; found = true; }
                    else
                    {
                        int dx = p.x - rc.x, dy = p.y - rc.y;
                        if (Math.Abs(dx) + Math.Abs(dy) == 1) { push = new Int2(dx, dy); found = true; }
                    }
                }
            }
            if (!found || (push.x == 0 && push.y == 0)) return false;
            if (push.x != 0 && b.axis == MoveAxis.Vertical) return false;
            if (push.y != 0 && b.axis == MoveAxis.Horizontal) return false;
            int j = Linked[i] ? b.partner : -1;
            Int2 target = Pos[i] + push;
            int oc;
            if (!CheckPlacement(i, target, j, out oc) || oc != 0) return false;
            if (j >= 0 && (!CheckPlacement(j, Pos[j] + push, i, out oc) || oc != 0)) return false;
            if (b.cells.Length == 1 && IsPortal(target)) return false;
            Commit(i, target);
            return true;
        }

        /// <summary>
        /// Electromagnet pull: if a cell of block i sits directly behind a pad (relative to the pad's pull direction) the block
        /// is pulled one tile onto it - free of charge. Returns true when the block moved (and updates <paramref name="newOrigin"/>).
        /// </summary>
        public bool PullOnce(int i, out Int2 newOrigin)
        {
            newOrigin = Pos[i];
            BlockInfo b = Blocks[i];
            if (Exited[i] || Dormant[i] || b.IsObstacle || Ice[i] > 0 || Linked[i]) return false;
            for (int m = 0; m < Def.magnets.Count; m++)
            {
                MagnetSpec ms = Def.magnets[m];
                Int2 pd = Dirs.Of(ms.pull);
                if (pd.x != 0 && b.axis == MoveAxis.Vertical) continue;
                if (pd.y != 0 && b.axis == MoveAxis.Horizontal) continue;
                bool behind = false;
                for (int c = 0; c < b.cells.Length; c++)
                {
                    Int2 p = Pos[i] + b.cells[c];
                    if (p + pd == ms.cell) { behind = true; break; }
                }
                if (!behind) continue;
                Int2 target = Pos[i] + pd;
                int outCount;
                if (!CheckPlacement(i, target, out outCount) || outCount != 0) continue;
                if (b.cells.Length == 1 && IsPortal(target)) continue;
                Commit(i, target);
                newOrigin = target;
                return true;
            }
            return false;
        }

        /// <summary>Applies pulls until the block settles (max 4). Returns how many pulls happened.</summary>
        public int PullAll(int i)
        {
            int n = 0;
            Int2 o;
            while (n < 4 && PullOnce(i, out o)) n++;
            return n;
        }

        /// <summary>Appends every single-direction slide / exit available now. exitAllowed[color] gates exits (null = all).</summary>
        public void GenerateMoves(List<Move> moves, bool[] exitAllowed)
        {
            for (int i = 0; i < Pos.Length; i++)
            {
                if (Exited[i] || Dormant[i] || Ice[i] > 0 || Blocks[i].IsObstacle) continue;
                for (int d = 0; d < 4; d++)
                {
                    GateDirection gd = Dirs.All[d];
                    Int2 dir = Dirs.Of(gd);
                    Int2 o = Pos[i];
                    int guard = 0;
                    while (guard++ < 64)
                    {
                        StepResult r = Probe(i, o, dir);
                        if (r == StepResult.Blocked) break;
                        if (r == StepResult.Exit)
                        {
                            if (exitAllowed == null || exitAllowed[(int)ColorOf(i)])
                            {
                                Move m = new Move();
                                m.block = i;
                                m.to = o;
                                m.exit = true;
                                m.dir = gd;
                                int g = ExitGateIndex(i, o, dir);
                                m.piercing = g >= 0 && Def.gates[g].isPiercingGate;
                                moves.Add(m);
                            }
                            break;
                        }
                        Int2 next = Land(i, o, dir);
                        if (next == Pos[i]) break; // a warp that lands on the block's own start cell: no new state
                        o = next;
                        Move s = new Move();
                        s.block = i;
                        s.to = o;
                        s.exit = false;
                        s.dir = gd;
                        moves.Add(s);
                    }
                }
            }
        }

        public void AppendKey(List<char> key)
        {
            for (int i = 0; i < Pos.Length; i++)
            {
                if (Blocks[i].IsObstacle) continue;
                key.Add(Dormant[i] ? (char)254 : (Exited[i] ? (char)255 : (char)(Pos[i].x * 16 + Pos[i].y + 1)));
                if (Blocks[i].iceLayer > 0) key.Add((char)(Ice[i] + 1));
            }
            if (hasShutter) key.Add((char)((Toggles & 1) + 1));
            if (steamPeriod > 0) key.Add((char)((Moves % steamPeriod) + 1));
            for (int i = 0; i < Pos.Length; i++) if (Blocks[i].crate) key.Add(Crated[i] ? (char)2 : (char)1);
            for (int k = 0; k < CarriageDir.Length; k++) key.Add(CarriageDir[k] > 0 ? (char)2 : (char)1);
            for (int i = 0; i < Pos.Length; i++) if (Blocks[i].partner > i && !Linked[i]) key.Add((char)(i + 1)); // pair already broken
            if (UnlockedMask != 0) key.Add((char)(UnlockedMask + 1));
        }
    }
}
