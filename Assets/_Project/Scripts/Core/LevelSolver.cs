using System;
using System.Collections.Generic;

namespace ColorGateBlast
{
    /// <summary>
    /// Solvability verifier + hint engine (pure C#, no UnityEngine, safe to run on a worker thread).
    /// Phase 1 is an exact breadth-first search over (block positions, monument HP, parked turrets) which also
    /// yields the true minimum move count (a move = one straight slide or exit). If the state budget runs out,
    /// phase 2 continues with a weighted best-first search that proves solvability and gives an upper bound.
    /// Every node remembers the FIRST move of its path, so the game can ask "what should I do next?".
    /// </summary>
    public static class LevelSolver
    {
        public sealed class SolveResult
        {
            public bool solved;
            /// <summary>Solution length in moves (minimal when <see cref="exact"/>, otherwise an upper bound).</summary>
            public int moves;
            public bool exact;
            public int visited;
            /// <summary>First move of the found solution (valid when solved and moves &gt; 0).</summary>
            public Move firstMove;
        }

        private sealed class Node
        {
            public PuzzleState state;
            public int depth;
            public double priority;
            public long order;
            public Move first;
        }

        public static SolveResult Solve(LevelDefinition def, int bfsStateCap, int heuristicStateCap)
        {
            return Solve(new PuzzleState(def), bfsStateCap, heuristicStateCap, int.MaxValue);
        }

        /// <summary>Like Solve but only accepts solutions of at most <paramref name="maxMoves"/> moves (e.g. the level's move limit).</summary>
        public static SolveResult SolveWithin(LevelDefinition def, int bfsStateCap, int heuristicStateCap, int maxMoves)
        {
            return Solve(new PuzzleState(def), bfsStateCap, heuristicStateCap, maxMoves);
        }

        public static SolveResult Solve(PuzzleState startState, int bfsStateCap, int heuristicStateCap)
        {
            return Solve(startState, bfsStateCap, heuristicStateCap, int.MaxValue);
        }

        public static SolveResult Solve(PuzzleState startState, int bfsStateCap, int heuristicStateCap, int maxMoves)
        {
            SolveResult res = new SolveResult();
            if (startState.IsWon) { res.solved = true; return res; }

            // ---- Phase 1: exact BFS ----
            HashSet<string> seen = new HashSet<string>();
            Queue<Node> queue = new Queue<Node>();
            seen.Add(startState.Key());
            Node root = new Node();
            root.state = startState;
            queue.Enqueue(root);
            List<Move> moves = new List<Move>();
            while (queue.Count > 0 && seen.Count < bfsStateCap)
            {
                Node n = queue.Dequeue();
                if (n.depth >= maxMoves) continue;
                moves.Clear();
                n.state.GenerateMoves(moves);
                for (int i = 0; i < moves.Count; i++)
                {
                    PuzzleState ns = n.state.Clone();
                    ns.Apply(moves[i]);
                    if (!seen.Add(ns.Key())) continue;
                    Move first = n.depth == 0 ? moves[i] : n.first;
                    if (ns.IsWon)
                    {
                        res.solved = true;
                        res.exact = true;
                        res.moves = n.depth + 1;
                        res.visited = seen.Count;
                        res.firstMove = first;
                        return res;
                    }
                    Node c = new Node();
                    c.state = ns;
                    c.depth = n.depth + 1;
                    c.first = first;
                    queue.Enqueue(c);
                }
            }
            if (queue.Count == 0)
            {
                res.visited = seen.Count; // whole reachable space exhausted: provably unsolvable
                return res;
            }

            // ---- Phase 2: weighted best-first search (greedy first, then closer to optimal) ----
            double[] weights = HeuristicWeight > 0 ? new double[] { HeuristicWeight } : new double[] { 3.0, 1.6, 0.9 };
            int[][] table = BuildDistanceTable(startState.Def);
            int visitedTotal = seen.Count;
            for (int wi = 0; wi < weights.Length; wi++)
            {
                SolveResult r = BestFirst(startState, heuristicStateCap / weights.Length, weights[wi], maxMoves, table);
                visitedTotal += r.visited;
                if (r.solved)
                {
                    r.visited = visitedTotal;
                    return r;
                }
            }
            res.visited = visitedTotal;
            return res;
        }

        private static SolveResult BestFirst(PuzzleState startState, int cap, double weight, int maxMoves, int[][] table)
        {
            SolveResult res = new SolveResult();
            HashSet<string> seen = new HashSet<string>();
            seen.Add(startState.Key());
            List<Node> heap = new List<Node>();
            List<Move> moves = new List<Move>();
            long counter = 0;
            Node r2 = new Node();
            r2.state = startState;
            r2.priority = Priority(startState, 0, weight, table);
            HeapPush(heap, r2);
            while (heap.Count > 0 && seen.Count < cap)
            {
                Node n = HeapPop(heap);
                if (n.depth >= maxMoves) continue;
                moves.Clear();
                n.state.GenerateMoves(moves);
                for (int i = 0; i < moves.Count; i++)
                {
                    PuzzleState ns = n.state.Clone();
                    ns.Apply(moves[i]);
                    if (!seen.Add(ns.Key())) continue;
                    Move first = n.depth == 0 ? moves[i] : n.first;
                    if (ns.IsWon)
                    {
                        res.solved = true;
                        res.exact = false;
                        res.moves = n.depth + 1;
                        res.visited = seen.Count;
                        res.firstMove = first;
                        return res;
                    }
                    Node c = new Node();
                    c.state = ns;
                    c.depth = n.depth + 1;
                    c.first = first;
                    c.priority = Priority(ns, c.depth, weight, table);
                    c.order = ++counter;
                    HeapPush(heap, c);
                }
            }
            res.visited = seen.Count;
            return res;
        }

        /// <summary>Weight of the heuristic in phase 2. 0 = automatic sequence (greedy, then tighter); 1.0 is A*-like.</summary>
        public static double HeuristicWeight = 0;

        private static double Priority(PuzzleState s, int depth, double weight, int[][] table)
        {
            return depth * 10.0 + weight * Heuristic(s, table);
        }

        /// <summary>
        /// For every block still to remove: 10 x (moves it needs to reach an exit if it were alone on the tray).
        /// Dormant companions count as "parent exit + one more move".
        /// </summary>
        private static double Heuristic(PuzzleState s, int[][] table)
        {
            BoardModel b = s.Board;
            double h = 0;
            int w = b.Width;
            for (int i = 0; i < b.BlockCount; i++)
            {
                if (b.Exited[i] || b.Blocks[i].IsObstacle) continue;
                if (b.Dormant[i]) { h += 20; continue; }
                int d = table[i][b.Pos[i].x + b.Pos[i].y * w];
                h += 10.0 * (d >= 99 ? 12 : d);
            }
            h += s.Parked.Count * 3;
            return h;
        }

        /// <summary>table[block][x + y*W] = minimal moves for the block to leave if it were alone (scrap iron only).</summary>
        private static int[][] BuildDistanceTable(LevelDefinition def)
        {
            int w = def.GridWidth, hgt = def.GridHeight;
            BlockInfo[] all = def.BuildBlocks();
            int[][] table = new int[all.Length][];
            for (int i = 0; i < all.Length; i++)
            {
                table[i] = new int[w * hgt];
                for (int k = 0; k < table[i].Length; k++) table[i][k] = 99;
                if (all[i].IsObstacle) continue;

                // a tiny level containing only the scrap iron and this one block (shutters treated as open)
                LevelDefinition solo = new LevelDefinition();
                solo.gridSize = def.gridSize;
                for (int g = 0; g < def.gates.Count; g++)
                {
                    GateSpec gs = def.gates[g];
                    gs.isShutterGate = false;
                    gs.isPadlocked = false;
                    solo.gates.Add(gs);
                }
                solo.portals = def.portals;
                for (int j = 0; j < def.placements.Count; j++)
                    if (def.placements[j].locked || def.placements[j].color == BlockColor.Gray) solo.placements.Add(def.placements[j]);

                Placement me;
                if (i < def.placements.Count)
                {
                    me = def.placements[i];
                    me.iceLayer = 0; me.hasInner = false; me.locked = false;
                }
                else
                {
                    me = new Placement();
                    me.color = all[i].color; me.shape = BlockShape.Rect; me.width = 1; me.height = 1;
                }
                me.x = 0; me.y = 0;
                me.linkGroup = 0;
                BlockColor[] cyc = me.cycle;
                me.cycle = null;
                // a prism block may leave in any colour of its cycle: the table keeps the best of them
                int variants = (cyc != null && cyc.Length > 1) ? cyc.Length : 1;
                for (int vv = 0; vv < variants; vv++)
                {
                    LevelDefinition one = new LevelDefinition();
                    one.gridSize = solo.gridSize;
                    one.gates = solo.gates;
                    one.portals = solo.portals;
                    one.placements = new List<Placement>(solo.placements);
                    Placement mv = me;
                    if (variants > 1) mv.color = cyc[vv];
                    one.placements.Add(mv);

                    BoardModel board = new BoardModel(one);
                    int blk = one.placements.Count - 1;
                    Int2[] cells = board.Blocks[blk].cells;
                    for (int sy = 0; sy < hgt; sy++)
                        for (int sx = 0; sx < w; sx++)
                            if (FitsAlone(board, blk, cells, new Int2(sx, sy)))
                            {
                                int dd = AloneDistance(board, blk, new Int2(sx, sy), w, hgt);
                                if (dd < table[i][sx + sy * w]) table[i][sx + sy * w] = dd;
                            }
                }
            }
            return table;
        }

        private static bool FitsAlone(BoardModel board, int blk, Int2[] cells, Int2 origin)
        {
            for (int c = 0; c < cells.Length; c++)
            {
                Int2 p = origin + cells[c];
                if (!board.InGrid(p)) return false;
                int o = board.At(p.x, p.y);
                if (o != -1 && o != blk) return false;
            }
            return true;
        }

        private static int AloneDistance(BoardModel board, int blk, Int2 start, int w, int h)
        {
            int[] dist = new int[w * h];
            for (int k = 0; k < dist.Length; k++) dist[k] = -1;
            Queue<Int2> q = new Queue<Int2>();
            dist[start.x + start.y * w] = 0;
            q.Enqueue(start);
            while (q.Count > 0)
            {
                Int2 cur = q.Dequeue();
                int cd = dist[cur.x + cur.y * w];
                for (int d = 0; d < 4; d++)
                {
                    Int2 dir = Dirs.Of(Dirs.All[d]);
                    Int2 o = cur;
                    int guard = 0;
                    while (guard++ < 64)
                    {
                        StepResult r = board.Probe(blk, o, dir);
                        if (r == StepResult.Blocked) break;
                        if (r == StepResult.Exit) return cd + 1;
                        o = board.Land(blk, o, dir);
                        int key = o.x + o.y * w;
                        if (dist[key] < 0) { dist[key] = cd + 1; q.Enqueue(o); }
                    }
                }
            }
            return 99;
        }

        // ---- tiny binary min-heap on (priority, order) ----
        private static bool Less(Node a, Node b)
        {
            if (a.priority != b.priority) return a.priority < b.priority;
            return a.order < b.order;
        }

        private static void HeapPush(List<Node> h, Node n)
        {
            h.Add(n);
            int i = h.Count - 1;
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (!Less(h[i], h[p])) break;
                Node t = h[i]; h[i] = h[p]; h[p] = t;
                i = p;
            }
        }

        private static Node HeapPop(List<Node> h)
        {
            Node top = h[0];
            int last = h.Count - 1;
            h[0] = h[last];
            h.RemoveAt(last);
            int i = 0;
            int n = h.Count;
            while (true)
            {
                int l = i * 2 + 1, r = l + 1, m = i;
                if (l < n && Less(h[l], h[m])) m = l;
                if (r < n && Less(h[r], h[m])) m = r;
                if (m == i) break;
                Node t = h[i]; h[i] = h[m]; h[m] = t;
                i = m;
            }
            return top;
        }
    }
}
