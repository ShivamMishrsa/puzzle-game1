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
        public Side dir;
    }

    /// <summary>
    /// Pure rules model of the inner grid. x = right, y = up. Gates live just outside the grid edge.
    /// A block may leave the grid only through an open gate of its own colour whose span covers every
    /// column (Top/Bottom) or row (Left/Right) the block occupies. Leaving is atomic: the whole path out
    /// must be free, then the block is removed.
    /// </summary>
    public sealed class BoardModel
    {
        public readonly int Width;
        public readonly int Height;
        public readonly LevelDefinition Def;
        public readonly Int2[] Pos;
        public readonly bool[] Exited;
        public int ExitedCount;
        public bool KeyExited;

        private readonly int[] occ;

        public BoardModel(LevelDefinition def)
        {
            Def = def;
            Width = def.gridWidth;
            Height = def.gridHeight;
            int n = def.blocks.Count;
            Pos = new Int2[n];
            Exited = new bool[n];
            occ = new int[Width * Height];
            for (int i = 0; i < occ.Length; i++) occ[i] = -1;
            for (int i = 0; i < n; i++)
            {
                Pos[i] = def.blocks[i].origin;
                Stamp(i, Pos[i], i);
            }
        }

        private BoardModel(BoardModel o)
        {
            Def = o.Def;
            Width = o.Width;
            Height = o.Height;
            Pos = (Int2[])o.Pos.Clone();
            Exited = (bool[])o.Exited.Clone();
            occ = (int[])o.occ.Clone();
            ExitedCount = o.ExitedCount;
            KeyExited = o.KeyExited;
        }

        public BoardModel Clone() { return new BoardModel(this); }

        public int BlockCount { get { return Def.blocks.Count; } }

        public bool InGrid(Int2 p) { return p.x >= 0 && p.y >= 0 && p.x < Width && p.y < Height; }

        /// <summary>Block index occupying the cell, or -1.</summary>
        public int At(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return -1;
            return occ[y * Width + x];
        }

        public bool IsFrozen(int i)
        {
            BlockSpec b = Def.blocks[i];
            return b.kind == BlockKind.Ice && ExitedCount < b.iceThawExits;
        }

        public bool IsGateOpen(GateSpec g) { return !g.locked || KeyExited; }

        private void Stamp(int i, Int2 origin, int value)
        {
            Int2[] cells = Def.blocks[i].cells;
            for (int c = 0; c < cells.Length; c++)
            {
                Int2 p = origin + cells[c];
                occ[p.y * Width + p.x] = value;
            }
        }

        /// <summary>Is the (out-of-grid) cell inside the lane of an open gate with this colour?</summary>
        private bool OutsideAllowed(Int2 p, BlockColor color)
        {
            Side side;
            int lane;
            if (p.x >= 0 && p.x < Width)
            {
                if (p.y >= Height) side = Side.Top;
                else if (p.y < 0) side = Side.Bottom;
                else return false;
                lane = p.x;
            }
            else if (p.y >= 0 && p.y < Height)
            {
                side = p.x >= Width ? Side.Right : Side.Left;
                lane = p.y;
            }
            else
            {
                return false; // diagonal corner region is never reachable
            }

            List<GateSpec> gates = Def.gates;
            for (int g = 0; g < gates.Count; g++)
            {
                GateSpec gt = gates[g];
                if (gt.side == side && gt.color == color && lane >= gt.start && lane < gt.start + gt.length && IsGateOpen(gt))
                    return true;
            }
            return false;
        }

        private bool CheckPlacement(int i, Int2 origin, out int outsideCount)
        {
            outsideCount = 0;
            BlockSpec b = Def.blocks[i];
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

        /// <summary>
        /// Result of pushing block <paramref name="i"/> one cell in <paramref name="dir"/> from the hypothetical
        /// origin <paramref name="origin"/> (all other blocks stay where they are).
        /// </summary>
        public StepResult Probe(int i, Int2 origin, Int2 dir)
        {
            if (Exited[i] || IsFrozen(i)) return StepResult.Blocked;
            MoveAxis axis = Def.blocks[i].axis;
            if (dir.x != 0 && axis == MoveAxis.Vertical) return StepResult.Blocked;
            if (dir.y != 0 && axis == MoveAxis.Horizontal) return StepResult.Blocked;

            Int2 o = origin + dir;
            int outCount;
            if (!CheckPlacement(i, o, out outCount)) return StepResult.Blocked;
            if (outCount == 0) return StepResult.Moved;

            // The block is entering a gate. It leaves only if the entire remaining path is clear.
            int tiles = Def.blocks[i].cells.Length;
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

        public void ExitBlock(int i)
        {
            Stamp(i, Pos[i], -1);
            Exited[i] = true;
            ExitedCount++;
            if (Def.blocks[i].kind == BlockKind.Key) KeyExited = true;
        }

        /// <summary>Appends every single-direction slide / exit available from the current position.</summary>
        public void GenerateMoves(List<Move> moves, bool allowExit)
        {
            for (int i = 0; i < Pos.Length; i++)
            {
                if (Exited[i] || IsFrozen(i)) continue;
                for (int d = 0; d < 4; d++)
                {
                    Side side = Dirs.AllSides[d];
                    Int2 dir = Dirs.Of(side);
                    Int2 o = Pos[i];
                    while (true)
                    {
                        StepResult r = Probe(i, o, dir);
                        if (r == StepResult.Blocked) break;
                        if (r == StepResult.Exit)
                        {
                            if (allowExit)
                            {
                                Move m = new Move();
                                m.block = i;
                                m.to = o;
                                m.exit = true;
                                m.dir = side;
                                moves.Add(m);
                            }
                            break;
                        }
                        o = o + dir;
                        Move s = new Move();
                        s.block = i;
                        s.to = o;
                        s.exit = false;
                        s.dir = side;
                        moves.Add(s);
                    }
                }
            }
        }

        public void AppendKey(List<char> key)
        {
            for (int i = 0; i < Pos.Length; i++)
                key.Add(Exited[i] ? (char)255 : (char)(Pos[i].x * 16 + Pos[i].y + 1));
        }
    }
}
