using System;
using System.Collections.Generic;

// Pure C# (no UnityEngine / UnityEditor) so it can also be compiled in a standalone test harness.

namespace ColorGateBlast.EditorTools
{
    /// <summary>
    /// Difficulty / feature table for the 150-level campaign.
    /// Difficulty follows a sawtooth: it ramps up over each 5-level wave (levels x1..x5) and drops back,
    /// while a slower overall ramp lifts every wave inside a tier. Levels ending in 5 are Mini-Bosses,
    /// levels ending in 0 are Chapter Bosses (bigger cores, more tiles, tighter move budget).
    /// </summary>
    public sealed class LevelParams
    {
        public int level;
        public Difficulty difficulty;
        public LevelTier tier;
        public int gridSize;
        public int colors;
        public int slots;
        public double d;          // 0..1 sawtooth difficulty
        public double fill;       // fraction of grid cells covered by blocks
        public double slackPct;   // extra moves on top of the solver's par
        public bool ice, key, twoLayer, directional, mystery, extendedShapes;
        public int maxBlockSize;
        public double[] sizeWeights; // index 1..4
        public int iceCount, lockCount, directionalCount, mysteryCount;

        public static LevelParams ForLevel(int level)
        {
            LevelParams p = new LevelParams();
            p.level = level;
            p.tier = (level % 10 == 0) ? LevelTier.ChapterBoss : ((level % 5 == 0) ? LevelTier.MiniBoss : LevelTier.Normal);

            int lo, hi;
            if (level <= 30) { p.difficulty = Difficulty.Easy; lo = 1; hi = 30; }
            else if (level <= 98) { p.difficulty = Difficulty.Medium; lo = 31; hi = 98; }
            else { p.difficulty = Difficulty.Hard; lo = 99; hi = 150; }

            double prog = Clamp01((level - lo) / (double)(hi - lo));
            double saw = ((level - 1) % 5) / 4.0;
            double d = 0.55 * prog + 0.45 * saw;
            if (p.tier == LevelTier.MiniBoss) d += 0.08;
            if (p.tier == LevelTier.ChapterBoss) d += 0.15;
            p.d = Clamp01(d);

            switch (p.difficulty)
            {
                case Difficulty.Easy:
                    p.gridSize = level <= 15 ? 5 : 6;
                    p.colors = level <= 10 ? 2 : 3;
                    p.slots = 5;
                    p.slackPct = 0.5;
                    break;
                case Difficulty.Medium:
                    p.gridSize = level <= 60 ? 6 : 7;
                    p.colors = level <= 64 ? 3 : 4;
                    p.slots = 5;
                    p.slackPct = 0.35;
                    break;
                default:
                    p.gridSize = level <= 124 ? 7 : 8;
                    p.colors = level <= 124 ? 4 : 5;
                    p.slots = 4;
                    p.slackPct = 0.25;
                    break;
            }
            if (p.tier != LevelTier.Normal) p.slackPct -= 0.05;

            p.fill = 0.27 + 0.25 * p.d;
            if (p.tier == LevelTier.MiniBoss) p.fill += 0.03;
            if (p.tier == LevelTier.ChapterBoss) p.fill += 0.05;
            if (p.fill > 0.58) p.fill = 0.58;

            p.ice = level >= 31;
            p.key = level >= 51;
            p.twoLayer = level >= 71;
            p.directional = level >= 101;
            p.mystery = level >= 121;
            p.extendedShapes = level >= 31;

            p.iceCount = p.ice ? 1 + (int)Math.Round(p.d * 2) : 0;
            p.lockCount = p.key ? ((level >= 71 && p.d > 0.5) ? 2 : 1) : 0;
            p.directionalCount = p.directional ? 2 + (int)Math.Round(p.d * 3) : 0;
            p.mysteryCount = p.mystery ? 1 + (int)Math.Round(p.d * 2) : 0;

            p.maxBlockSize = level <= 3 ? 2 : 4;
            if (level <= 3) p.sizeWeights = new double[] { 0, 2, 3, 0, 0 };
            else if (p.difficulty == Difficulty.Easy) p.sizeWeights = new double[] { 0, 3, 4, 2, 1 };
            else if (p.difficulty == Difficulty.Medium) p.sizeWeights = new double[] { 0, 2, 4, 3, 2 };
            else p.sizeWeights = new double[] { 0, 1, 3, 3, 3 };
            return p;
        }

        private static double Clamp01(double v) { return v < 0 ? 0 : (v > 1 ? 1 : v); }
    }

    public sealed class GenResult
    {
        public LevelDefinition def;
        public LevelSolver.SolveResult solve;
        public int attempts;
    }

    public static class LevelGenerator
    {
        public const int MaxAttempts = 80;

        /// <summary>Builds, solves and budgets one level. Returns null def when no solvable layout was found.</summary>
        public static GenResult Generate(int level)
        {
            return Generate(level, MaxAttempts, 15000, 60000);
        }

        public static GenResult Generate(int level, int maxAttempts, int bfsCap, int heuristicCap)
        {
            LevelParams p = LevelParams.ForLevel(level);
            GenResult best = new GenResult();
            int bestExtra = int.MinValue;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int seed = level * 7919 + attempt * 104729 + 13;
                Random rng = new Random(seed);
                LevelDefinition def = TryBuild(p, rng);
                if (def == null) continue;
                def.seed = seed;
                if (def.Validate() != null) continue;

                LevelSolver.SolveResult sr = LevelSolver.Solve(def, bfsCap, heuristicCap);
                if (!sr.solved) continue;

                int blocks = def.blocks.Count;
                int extra = sr.moves - blocks;
                int target = (int)Math.Round(p.d * blocks * 0.45);
                if (extra > bestExtra)
                {
                    bestExtra = extra;
                    best.def = def;
                    best.solve = sr;
                    best.attempts = attempt + 1;
                }
                if (extra >= target) break;
            }

            if (best.def != null)
            {
                int par = best.solve.moves;
                int slack = Math.Max(2, (int)Math.Ceiling(par * LevelParams.ForLevel(level).slackPct));
                best.def.parMoves = par;
                best.def.moveLimit = par + slack;
            }
            return best;
        }

        // ------------------------------------------------------------------------------------------

        private static LevelDefinition TryBuild(LevelParams p, Random rng)
        {
            int size = p.gridSize;
            int cells = size * size;
            int colors = p.colors;

            int tiles = (int)Math.Round(cells * p.fill);
            tiles = Math.Max(tiles, colors * 2);
            tiles = Math.Min(tiles, (int)(cells * 0.6));

            int doubles = p.twoLayer ? (int)Math.Round(tiles * (0.18 + 0.22 * p.d)) : 0;
            int brickCount = tiles - doubles;

            // ---- core footprint ----
            int ch = Math.Max(2, (int)Math.Round(Math.Sqrt(brickCount * 0.85)));
            int cw = (int)Math.Ceiling(brickCount / (double)ch);
            while (cw * ch < brickCount) cw++;
            if (cw > 8 || ch > 8) return null;

            List<Int2> coreCells = new List<Int2>();
            for (int y = 0; y < ch; y++)
                for (int x = 0; x < cw; x++)
                    coreCells.Add(new Int2(x, y));
            int remove = coreCells.Count - brickCount;
            if (remove > 0)
            {
                double mx = (cw - 1) / 2.0, my = (ch - 1) / 2.0;
                List<KeyValuePair<double, Int2>> scored = new List<KeyValuePair<double, Int2>>();
                for (int i = 0; i < coreCells.Count; i++)
                {
                    Int2 c = coreCells[i];
                    double dist = Math.Abs(c.x - mx) + Math.Abs(c.y - my) + rng.NextDouble() * 0.9;
                    scored.Add(new KeyValuePair<double, Int2>(dist, c));
                }
                scored.Sort(delegate (KeyValuePair<double, Int2> a, KeyValuePair<double, Int2> b) { return b.Key.CompareTo(a.Key); });
                coreCells.Clear();
                for (int i = remove; i < scored.Count; i++) coreCells.Add(scored[i].Value);
            }
            if (coreCells.Count < colors) return null;

            // ---- colour the core: nearest of C random seeds (gives chunky, readable colour regions) ----
            List<int> order = new List<int>();
            for (int i = 0; i < coreCells.Count; i++) order.Add(i);
            Shuffle(order, rng);
            Int2[] seeds = new Int2[colors];
            for (int c = 0; c < colors; c++) seeds[c] = coreCells[order[c]];

            List<BrickSpec> bricks = new List<BrickSpec>();
            for (int i = 0; i < coreCells.Count; i++)
            {
                Int2 cell = coreCells[i];
                int bestColor = 0;
                double bestDist = 1e9;
                for (int c = 0; c < colors; c++)
                {
                    double dist = Math.Abs(cell.x - seeds[c].x) + Math.Abs(cell.y - seeds[c].y) + rng.NextDouble() * 1.2;
                    if (dist < bestDist) { bestDist = dist; bestColor = c; }
                }
                BrickSpec b = new BrickSpec();
                b.x = cell.x;
                b.y = cell.y;
                b.layers = new BlockColor[] { (BlockColor)bestColor };
                bricks.Add(b);
            }
            // Seed bricks always keep their own colour so every colour is present.
            for (int c = 0; c < colors; c++)
                for (int i = 0; i < bricks.Count; i++)
                    if (bricks[i].x == seeds[c].x && bricks[i].y == seeds[c].y) bricks[i].layers[0] = (BlockColor)c;

            // ---- second layers ----
            List<int> bo = new List<int>();
            for (int i = 0; i < bricks.Count; i++) bo.Add(i);
            Shuffle(bo, rng);
            for (int k = 0; k < doubles && k < bo.Count; k++)
            {
                BrickSpec b = bricks[bo[k]];
                int front = (int)b.layers[0];
                int other = rng.Next(0, colors - 1);
                if (other >= front) other++;
                b.layers = new BlockColor[] { (BlockColor)front, (BlockColor)other };
            }

            // ---- ammo per colour == layers per colour; split into polyomino blocks ----
            int[] perColor = new int[colors];
            for (int i = 0; i < bricks.Count; i++)
                for (int l = 0; l < bricks[i].layers.Length; l++)
                    perColor[(int)bricks[i].layers[l]]++;

            List<BlockSpec> blocks = new List<BlockSpec>();
            for (int c = 0; c < colors; c++)
            {
                int n = perColor[c];
                while (n > 0)
                {
                    int sz = PickSize(n, p, rng);
                    BlockSpec bs = new BlockSpec();
                    bs.color = (BlockColor)c;
                    bs.cells = Shapes.Random(sz, p.extendedShapes, rng);
                    blocks.Add(bs);
                    n -= sz;
                }
            }

            // ---- place blocks, biggest first ----
            blocks.Sort(delegate (BlockSpec a, BlockSpec b) { return b.TileCount.CompareTo(a.TileCount); });
            bool[] occ = new bool[cells];
            for (int bi = 0; bi < blocks.Count; bi++)
            {
                BlockSpec b = blocks[bi];
                int ex = b.ExtentX(), ey = b.ExtentY();
                if (ex > size || ey > size) return null;
                bool placed = false;
                for (int t = 0; t < 150 && !placed; t++)
                {
                    int ox = rng.Next(0, size - ex + 1);
                    int oy = rng.Next(0, size - ey + 1);
                    bool free = true;
                    for (int c = 0; c < b.cells.Length && free; c++)
                        if (occ[(oy + b.cells[c].y) * size + ox + b.cells[c].x]) free = false;
                    if (!free) continue;
                    for (int c = 0; c < b.cells.Length; c++)
                        occ[(oy + b.cells[c].y) * size + ox + b.cells[c].x] = true;
                    b.origin = new Int2(ox, oy);
                    placed = true;
                }
                if (!placed) return null;
            }
            // Shuffle so special-block selection is not biased towards big blocks.
            Shuffle(blocks, rng);

            // ---- gates: sized to the blocks of their colour so every block can leave somewhere ----
            List<GateSpec> gates = new List<GateSpec>();
            for (int c = 0; c < colors; c++)
            {
                int count = 1 + ((p.level > 3 && rng.NextDouble() < 0.25 + 0.45 * p.d) ? 1 : 0);
                for (int k = 0; k < count; k++)
                {
                    for (int tries = 0; tries < 40; tries++)
                    {
                        Side side = Dirs.AllSides[rng.Next(0, 4)];
                        bool horizontalEdge = side == Side.Top || side == Side.Bottom;
                        int need = 1;
                        for (int i = 0; i < blocks.Count; i++)
                        {
                            if ((int)blocks[i].color != c) continue;
                            int e = horizontalEdge ? blocks[i].ExtentX() : blocks[i].ExtentY();
                            if (e > need) need = e;
                        }
                        int len = need;
                        if (len < size && rng.NextDouble() < 0.3) len++;
                        int start = rng.Next(0, size - len + 1);
                        bool clash = false;
                        for (int g = 0; g < gates.Count; g++)
                        {
                            GateSpec o = gates[g];
                            if (o.side == side && start < o.start + o.length && o.start < start + len) { clash = true; break; }
                        }
                        if (clash) continue;
                        GateSpec gate = new GateSpec();
                        gate.side = side;
                        gate.start = start;
                        gate.length = len;
                        gate.color = (BlockColor)c;
                        gates.Add(gate);
                        break;
                    }
                }
                bool hasGate = false;
                for (int g = 0; g < gates.Count; g++) if ((int)gates[g].color == c) hasGate = true;
                if (!hasGate) return null;
            }

            // ---- special mechanics (kinds are exclusive: Key, then Ice, then Mystery) ----
            if (p.lockCount > 0)
            {
                int keyIdx = rng.Next(0, blocks.Count);
                blocks[keyIdx].kind = BlockKind.Key;
                List<int> lockable = new List<int>();
                for (int g = 0; g < gates.Count; g++) if (gates[g].color != blocks[keyIdx].color) lockable.Add(g);
                if (lockable.Count == 0) return null;
                Shuffle(lockable, rng);
                for (int k = 0; k < p.lockCount && k < lockable.Count; k++) gates[lockable[k]].locked = true;
            }
            PickKind(blocks, BlockKind.Ice, p.iceCount, rng);
            for (int i = 0; i < blocks.Count; i++)
                if (blocks[i].kind == BlockKind.Ice)
                    blocks[i].iceThawExits = 1 + rng.Next(0, Math.Max(1, Math.Min(3, blocks.Count - 2)));
            PickKind(blocks, BlockKind.Mystery, p.mysteryCount, rng);
            if (p.directionalCount > 0)
            {
                List<int> idx = new List<int>();
                for (int i = 0; i < blocks.Count; i++) idx.Add(i);
                Shuffle(idx, rng);
                for (int k = 0; k < p.directionalCount && k < idx.Count; k++)
                {
                    BlockSpec b = blocks[idx[k]];
                    int ex = b.ExtentX(), ey = b.ExtentY();
                    if (ex > ey) b.axis = MoveAxis.Horizontal;
                    else if (ey > ex) b.axis = MoveAxis.Vertical;
                    else b.axis = rng.Next(0, 2) == 0 ? MoveAxis.Horizontal : MoveAxis.Vertical;
                }
            }

            LevelDefinition def = new LevelDefinition();
            def.levelNumber = p.level;
            def.difficulty = p.difficulty;
            def.tier = p.tier;
            def.gridWidth = size;
            def.gridHeight = size;
            def.coreWidth = cw;
            def.coreHeight = ch;
            def.maxTrackSlots = p.slots;
            def.colorCount = colors;
            def.blocks = blocks;
            def.gates = gates;
            def.bricks = bricks;
            return def;
        }

        private static void PickKind(List<BlockSpec> blocks, BlockKind kind, int count, Random rng)
        {
            if (count <= 0) return;
            List<int> normals = new List<int>();
            for (int i = 0; i < blocks.Count; i++) if (blocks[i].kind == BlockKind.Normal) normals.Add(i);
            Shuffle(normals, rng);
            // Never convert the last normal block: keep at least one plain block in every level.
            for (int k = 0; k < count && k < normals.Count - 1; k++) blocks[normals[k]].kind = kind;
        }

        private static int PickSize(int remaining, LevelParams p, Random rng)
        {
            int max = Math.Min(remaining, p.maxBlockSize);
            double total = 0;
            for (int s = 1; s <= max; s++) total += p.sizeWeights[s];
            if (total <= 0) return 1;
            double r = rng.NextDouble() * total;
            for (int s = 1; s <= max; s++)
            {
                r -= p.sizeWeights[s];
                if (r <= 0 && p.sizeWeights[s] > 0) return s;
            }
            return 1;
        }

        private static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                T t = list[i]; list[i] = list[j]; list[j] = t;
            }
        }
    }

    /// <summary>Polyomino catalogue: 1x1, 1x2, 2x2, L3 for Easy; plus 1x3, L4, T, S from Medium.</summary>
    internal static class Shapes
    {
        private static Int2[] Make(params int[] xy)
        {
            Int2[] r = new Int2[xy.Length / 2];
            for (int i = 0; i < r.Length; i++) r[i] = new Int2(xy[i * 2], xy[i * 2 + 1]);
            return r;
        }

        public static Int2[] Random(int size, bool extended, Random rng)
        {
            List<Int2[]> pool = new List<Int2[]>();
            switch (size)
            {
                case 1:
                    pool.Add(Make(0, 0));
                    break;
                case 2:
                    pool.Add(Make(0, 0, 1, 0));
                    break;
                case 3:
                    pool.Add(Make(0, 0, 1, 0, 0, 1));            // L
                    if (extended) pool.Add(Make(0, 0, 1, 0, 2, 0)); // 1x3
                    break;
                default:
                    pool.Add(Make(0, 0, 1, 0, 0, 1, 1, 1));      // 2x2
                    if (extended)
                    {
                        pool.Add(Make(0, 0, 0, 1, 0, 2, 1, 0));  // L4
                        pool.Add(Make(0, 0, 1, 0, 2, 0, 1, 1));  // T
                        pool.Add(Make(0, 0, 1, 0, 1, 1, 2, 1));  // S
                    }
                    break;
            }
            Int2[] baseShape = pool[rng.Next(0, pool.Count)];
            int rot = rng.Next(0, 4);
            bool mirror = rng.Next(0, 2) == 1;
            Int2[] cells = new Int2[baseShape.Length];
            int minX = int.MaxValue, minY = int.MaxValue;
            for (int i = 0; i < cells.Length; i++)
            {
                int x = baseShape[i].x, y = baseShape[i].y;
                for (int r = 0; r < rot; r++) { int nx = -y, ny = x; x = nx; y = ny; }
                if (mirror) x = -x;
                cells[i] = new Int2(x, y);
                if (x < minX) minX = x;
                if (y < minY) minY = y;
            }
            for (int i = 0; i < cells.Length; i++) cells[i] = new Int2(cells[i].x - minX, cells[i].y - minY);
            return cells;
        }
    }
}
