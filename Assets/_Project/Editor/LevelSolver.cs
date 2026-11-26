using System;
using System.Collections.Generic;

// Pure C# (no UnityEngine / UnityEditor) so it can also be compiled in a standalone test harness.

namespace ColorGateBlast.EditorTools
{
    /// <summary>
    /// Solvability verifier. Phase 1 is an exact breadth-first search over (block positions, core layers,
    /// parked turrets) which also yields the true minimum move count (a move = one straight slide or exit).
    /// If the state budget is exhausted, phase 2 continues with a weighted best-first search (BFS ordered by
    /// moves + weighted heuristic) that proves solvability and gives an upper bound for the par.
    /// </summary>
    public static class LevelSolver
    {
        public sealed class SolveResult
        {
            public bool solved;
            /// <summary>Solution length in moves (minimal when <see cref="exact"/>, otherwise an upper bound).</summary>
            public int moves;
            /// <summary>True when found by the exhaustive BFS phase.</summary>
            public bool exact;
            public int visited;
        }

        private sealed class Node
        {
            public PuzzleState state;
            public int depth;
            public double priority;
            public long order;
        }

        public static SolveResult Solve(LevelDefinition def, int bfsStateCap, int heuristicStateCap)
        {
            SolveResult res = new SolveResult();
            PuzzleState start = new PuzzleState(def);
            if (start.IsWon) { res.solved = true; return res; }

            // ---- Phase 1: exact BFS ----
            HashSet<string> seen = new HashSet<string>();
            Queue<Node> queue = new Queue<Node>();
            seen.Add(start.Key());
            Node root = new Node();
            root.state = start;
            queue.Enqueue(root);
            List<Move> moves = new List<Move>();
            while (queue.Count > 0 && seen.Count < bfsStateCap)
            {
                Node n = queue.Dequeue();
                moves.Clear();
                n.state.GenerateMoves(moves);
                for (int i = 0; i < moves.Count; i++)
                {
                    PuzzleState ns = n.state.Clone();
                    ns.Apply(moves[i]);
                    if (!seen.Add(ns.Key())) continue;
                    if (ns.IsWon)
                    {
                        res.solved = true;
                        res.exact = true;
                        res.moves = n.depth + 1;
                        res.visited = seen.Count;
                        return res;
                    }
                    Node c = new Node();
                    c.state = ns;
                    c.depth = n.depth + 1;
                    queue.Enqueue(c);
                }
            }
            if (queue.Count == 0)
            {
                // The whole reachable space was exhausted without a win: provably unsolvable.
                res.visited = seen.Count;
                return res;
            }

            // ---- Phase 2: weighted best-first search ----
            seen.Clear();
            seen.Add(start.Key());
            List<Node> heap = new List<Node>();
            long counter = 0;
            Node r2 = new Node();
            r2.state = start;
            r2.priority = Priority(start, 0);
            HeapPush(heap, r2);
            while (heap.Count > 0 && seen.Count < heuristicStateCap)
            {
                Node n = HeapPop(heap);
                moves.Clear();
                n.state.GenerateMoves(moves);
                for (int i = 0; i < moves.Count; i++)
                {
                    PuzzleState ns = n.state.Clone();
                    ns.Apply(moves[i]);
                    if (!seen.Add(ns.Key())) continue;
                    if (ns.IsWon)
                    {
                        res.solved = true;
                        res.exact = false;
                        res.moves = n.depth + 1;
                        res.visited = seen.Count;
                        return res;
                    }
                    Node c = new Node();
                    c.state = ns;
                    c.depth = n.depth + 1;
                    c.priority = Priority(ns, c.depth);
                    c.order = ++counter;
                    HeapPush(heap, c);
                }
            }
            res.visited = seen.Count;
            return res;
        }

        private static double Priority(PuzzleState s, int depth)
        {
            return depth * 10.0 + 1.0 * Heuristic(s);
        }

        /// <summary>Roughly "10 per block still on the board" plus how far each is from its nearest gate.</summary>
        private static double Heuristic(PuzzleState s)
        {
            BoardModel b = s.Board;
            LevelDefinition def = s.Def;
            double h = 0;
            for (int i = 0; i < b.BlockCount; i++)
            {
                if (b.Exited[i]) continue;
                BlockSpec spec = def.blocks[i];
                double cx = b.Pos[i].x + spec.ExtentX() / 2.0;
                double cy = b.Pos[i].y + spec.ExtentY() / 2.0;
                double best = 99;
                for (int g = 0; g < def.gates.Count; g++)
                {
                    GateSpec gt = def.gates[g];
                    if (gt.color != spec.color) continue;
                    double gx, gy;
                    double mid = gt.start + gt.length / 2.0;
                    switch (gt.side)
                    {
                        case Side.Top: gx = mid; gy = b.Height; break;
                        case Side.Bottom: gx = mid; gy = 0; break;
                        case Side.Left: gx = 0; gy = mid; break;
                        default: gx = b.Width; gy = mid; break;
                    }
                    double d = Math.Abs(cx - gx) + Math.Abs(cy - gy);
                    if (d < best) best = d;
                }
                h += 10 + best;
            }
            // Parked turrets that cannot fire are a liability.
            h += s.Parked.Count * 4;
            return h;
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
