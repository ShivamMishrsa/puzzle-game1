using System.Collections.Generic;

namespace ColorGateBlast
{
    /// <summary>
    /// Geometry of the conveyor loop that surrounds the Target Core. The loop is the ring of cells around a
    /// (cw x ch) core, indexed clockwise starting at the top-left corner:
    /// top row (left to right), right column (down), bottom row (right to left), left column (up).
    /// Ring cell (rx, ry) sits next to core cell (rx-1, ry-1); x = right, y = up.
    /// </summary>
    public static class RingPath
    {
        public static int Length(int cw, int ch) { return 2 * cw + 2 * ch + 4; }

        /// <summary>Where new turrets enter the loop: the bottom-left corner (closest to the player's board).</summary>
        public static int EntryIndex(int cw, int ch) { return 2 * cw + ch + 3; }

        public static void Cell(int idx, int cw, int ch, out int rx, out int ry)
        {
            idx = ((idx % Length(cw, ch)) + Length(cw, ch)) % Length(cw, ch);
            if (idx <= cw + 1) { rx = idx; ry = ch + 1; return; }
            idx -= cw + 2;
            if (idx <= ch) { rx = cw + 1; ry = ch - idx; return; }
            idx -= ch + 1;
            if (idx <= cw) { rx = cw - idx; ry = 0; return; }
            idx -= cw + 1;
            rx = 0;
            ry = 1 + idx;
        }

        /// <summary>
        /// Turrets on non-corner ring cells fire straight at the core. Returns false for corners.
        /// <paramref name="side"/> is the core side the turret stands on; <paramref name="lane"/> is the
        /// core column (Top/Bottom) or row (Left/Right) it fires along.
        /// </summary>
        public static bool FireInfo(int idx, int cw, int ch, out Side side, out int lane)
        {
            int rx, ry;
            Cell(idx, cw, ch, out rx, out ry);
            side = Side.Top;
            lane = 0;
            if (ry == ch + 1 && rx >= 1 && rx <= cw) { side = Side.Top; lane = rx - 1; return true; }
            if (rx == cw + 1 && ry >= 1 && ry <= ch) { side = Side.Right; lane = ry - 1; return true; }
            if (ry == 0 && rx >= 1 && rx <= cw) { side = Side.Bottom; lane = rx - 1; return true; }
            if (rx == 0 && ry >= 1 && ry <= ch) { side = Side.Left; lane = ry - 1; return true; }
            return false;
        }
    }

    /// <summary>
    /// Pure model of the Target Core. Bricks may have several layers; only layers[Layer[b]] is "exposed"
    /// and only the first brick seen from a side along a lane can be hit from that side.
    /// </summary>
    public sealed class CoreModel
    {
        public readonly int Width;
        public readonly int Height;
        public readonly BrickSpec[] Bricks;
        public readonly int[] Layer;
        public int RemainingLayers;

        private readonly int[] cell;

        public CoreModel(LevelDefinition def)
        {
            Width = def.coreWidth;
            Height = def.coreHeight;
            Bricks = def.bricks.ToArray();
            Layer = new int[Bricks.Length];
            cell = new int[Width * Height];
            for (int i = 0; i < cell.Length; i++) cell[i] = -1;
            for (int i = 0; i < Bricks.Length; i++)
            {
                cell[Bricks[i].y * Width + Bricks[i].x] = i;
                RemainingLayers += Bricks[i].layers.Length;
            }
        }

        private CoreModel(CoreModel o)
        {
            Width = o.Width;
            Height = o.Height;
            Bricks = o.Bricks;
            cell = o.cell;
            Layer = (int[])o.Layer.Clone();
            RemainingLayers = o.RemainingLayers;
        }

        public CoreModel Clone() { return new CoreModel(this); }

        public bool IsAlive(int b) { return Layer[b] < Bricks[b].layers.Length; }

        /// <summary>Colour of the exposed layer, or -1 when the brick is gone.</summary>
        public int FrontColor(int b) { return IsAlive(b) ? (int)Bricks[b].layers[Layer[b]] : -1; }

        /// <summary>First living brick met when looking into the core from <paramref name="side"/> along <paramref name="lane"/>.</summary>
        public int FirstInLane(Side side, int lane)
        {
            switch (side)
            {
                case Side.Top:
                    if (lane < 0 || lane >= Width) return -1;
                    for (int y = Height - 1; y >= 0; y--) { int b = cell[y * Width + lane]; if (b >= 0 && IsAlive(b)) return b; }
                    return -1;
                case Side.Bottom:
                    if (lane < 0 || lane >= Width) return -1;
                    for (int y = 0; y < Height; y++) { int b = cell[y * Width + lane]; if (b >= 0 && IsAlive(b)) return b; }
                    return -1;
                case Side.Left:
                    if (lane < 0 || lane >= Height) return -1;
                    for (int x = 0; x < Width; x++) { int b = cell[lane * Width + x]; if (b >= 0 && IsAlive(b)) return b; }
                    return -1;
                default:
                    if (lane < 0 || lane >= Height) return -1;
                    for (int x = Width - 1; x >= 0; x--) { int b = cell[lane * Width + x]; if (b >= 0 && IsAlive(b)) return b; }
                    return -1;
            }
        }

        /// <summary>Brick that a turret of <paramref name="color"/> could hit from this exact firing position, or -1.</summary>
        public int FindHitAt(int color, Side side, int lane)
        {
            int b = FirstInLane(side, lane);
            return (b >= 0 && FrontColor(b) == color) ? b : -1;
        }

        /// <summary>Any brick a turret of this colour could hit from anywhere on the loop, or -1.</summary>
        public int FindHit(int color)
        {
            for (int s = 0; s < 4; s++)
            {
                Side side = Dirs.AllSides[s];
                int lanes = (side == Side.Top || side == Side.Bottom) ? Width : Height;
                for (int lane = 0; lane < lanes; lane++)
                {
                    int b = FindHitAt(color, side, lane);
                    if (b >= 0) return b;
                }
            }
            return -1;
        }

        public void Hit(int b)
        {
            Layer[b]++;
            RemainingLayers--;
        }

        public int RemainingOfColor(int color)
        {
            int n = 0;
            for (int b = 0; b < Bricks.Length; b++)
            {
                BlockColor[] l = Bricks[b].layers;
                for (int k = Layer[b]; k < l.Length; k++) if ((int)l[k] == color) n++;
            }
            return n;
        }

        public void AppendKey(List<char> key)
        {
            for (int i = 0; i < Layer.Length; i++) key.Add((char)(Layer[i] + 1));
        }
    }
}
