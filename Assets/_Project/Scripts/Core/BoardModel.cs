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

        public void Clear()
        {
            thawed.Clear();
            spawned = -1;
            shutterToggled = false;
        }
    }

    /// <summary>
    /// Pure rules model of the recess tray. x = right, y = up (away from the player). Gates live just outside
    /// the tray edge. A block may leave only through an OPEN gate of its own colour whose span covers every column
    /// (Up/Down) or row (Left/Right) it occupies. Leaving is atomic: the whole path out must be free.
    ///
    /// Batch 2 mechanics:
    ///  * Ice: a block with iceLayer &gt; 0 is frozen. Whenever a block exits, every orthogonally adjacent iced block
    ///    loses one layer.
    ///  * Dual layer: a block with an inner colour leaves a dormant 1x1 "companion" that drops into the vacated slot.
    ///  * Shutter gates: every exit toggles all shutter gates (open &lt;-&gt; closed).
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
        public int ExitedCount;
        /// <summary>Number of exits so far; its parity flips every shutter gate.</summary>
        public int Toggles;

        private readonly int[] occ;
        private readonly bool hasShutter;

        public BoardModel(LevelDefinition def)
        {
            Def = def;
            for (int g = 0; g < def.gates.Count; g++) if (def.gates[g].isShutterGate) hasShutter = true;
            Width = def.GridWidth;
            Height = def.GridHeight;
            Blocks = def.BuildBlocks();
            int n = Blocks.Length;
            Pos = new Int2[n];
            Exited = new bool[n];
            Dormant = new bool[n];
            Ice = new int[n];
            occ = new int[Width * Height];
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
            occ = (int[])o.occ.Clone();
            ExitedCount = o.ExitedCount;
            Toggles = o.Toggles;
            hasShutter = o.hasShutter;
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

        /// <summary>Block index occupying the cell, or -1.</summary>
        public int At(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return -1;
            return occ[y * Width + x];
        }

        public bool IsGateOpen(GateSpec g)
        {
            if (!g.isShutterGate) return true;
            return g.startsOpen != ((Toggles & 1) == 1);
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

        /// <summary>Is the (out-of-grid) cell inside the lane of an OPEN gate with this colour?</summary>
        private bool OutsideAllowed(Int2 p, BlockColor color)
        {
            GateDirection dir;
            int lane;
            if (p.x >= 0 && p.x < Width)
            {
                if (p.y >= Height) dir = GateDirection.Up;
                else if (p.y < 0) dir = GateDirection.Down;
                else return false;
                lane = p.x;
            }
            else if (p.y >= 0 && p.y < Height)
            {
                dir = p.x >= Width ? GateDirection.Right : GateDirection.Left;
                lane = p.y;
            }
            else
            {
                return false;
            }

            List<GateSpec> gates = Def.gates;
            for (int g = 0; g < gates.Count; g++)
            {
                GateSpec gt = gates[g];
                if (gt.direction == dir && gt.color == color && lane >= gt.start && lane < gt.start + gt.length && IsGateOpen(gt))
                    return true;
            }
            return false;
        }

        private bool CheckPlacement(int i, Int2 origin, out int outsideCount)
        {
            outsideCount = 0;
            BlockInfo b = Blocks[i];
            Int2[] cells = b.cells;
            for (int c = 0; c < cells.Length; c++)
            {
                Int2 p = origin + cells[c];
                if (InGrid(p))
                {
                    int v = occ[p.y * Width + p.x];
                    if (v != -1 && v != i) return false;
                }
                else
                {
                    if (!OutsideAllowed(p, b.color)) return false;
                    outsideCount++;
                }
            }
            return true;
        }

        /// <summary>Result of pushing block <paramref name="i"/> one cell from the hypothetical origin.</summary>
        public StepResult Probe(int i, Int2 origin, Int2 dir)
        {
            BlockInfo b = Blocks[i];
            if (Exited[i] || Dormant[i] || b.IsObstacle || Ice[i] > 0) return StepResult.Blocked;
            if (dir.x != 0 && b.axis == MoveAxis.Vertical) return StepResult.Blocked;
            if (dir.y != 0 && b.axis == MoveAxis.Horizontal) return StepResult.Blocked;

            Int2 o = origin + dir;
            int outCount;
            if (!CheckPlacement(i, o, out outCount)) return StepResult.Blocked;
            if (outCount == 0) return StepResult.Moved;

            // The block is entering a gate: it leaves only if the entire remaining path is clear.
            int tiles = b.cells.Length;
            while (outCount < tiles)
            {
                o = o + dir;
                if (!CheckPlacement(i, o, out outCount)) return StepResult.Blocked;
            }
            return StepResult.Exit;
        }

        public StepResult Probe(int i, Int2 dir) { return Probe(i, Pos[i], dir); }

        public void Commit(int i, Int2 newOrigin)
        {
            Stamp(i, Pos[i], -1);
            Pos[i] = newOrigin;
            Stamp(i, newOrigin, i);
        }

        /// <summary>Removes block <paramref name="i"/> from the tray and applies ice thaw, companion respawn and shutter toggle.</summary>
        public void ExitBlock(int i, ExitEffects fx)
        {
            if (fx != null) fx.Clear();
            BlockInfo b = Blocks[i];

            // orthogonal neighbours of the block at the moment it leaves
            List<int> neighbours = new List<int>();
            Int2[] cells = b.cells;
            for (int c = 0; c < cells.Length; c++)
            {
                Int2 p = Pos[i] + cells[c];
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

            for (int k = 0; k < neighbours.Count; k++)
            {
                int j = neighbours[k];
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
                Pos[c] = Pos[i] + b.cells[0];
                Stamp(c, Pos[c], c);
                if (fx != null) { fx.spawned = c; fx.spawnCell = Pos[c]; }
            }
        }

        public bool HasShutter() { return hasShutter; }

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
                    while (true)
                    {
                        StepResult r = Probe(i, o, dir);
                        if (r == StepResult.Blocked) break;
                        if (r == StepResult.Exit)
                        {
                            if (exitAllowed == null || exitAllowed[(int)Blocks[i].color])
                            {
                                Move m = new Move();
                                m.block = i;
                                m.to = o;
                                m.exit = true;
                                m.dir = gd;
                                moves.Add(m);
                            }
                            break;
                        }
                        o = o + dir;
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
        }
    }
}
