using System;
using System.Collections.Generic;
using System.Diagnostics;
using ColorGateBlast;
using ColorGateBlast.EditorTools;

class Program
{
    static string C(BlockColor c) { return c == BlockColor.Red ? "R" : c == BlockColor.Blue ? "B" : c == BlockColor.Yellow ? "Y" : "X"; }

    static string Fmt(Placement p)
    {
        string s;
        if (p.locked) s = "Scrap(" + p.width + ", " + p.height + ", " + p.x + ", " + p.y + ")";
        else if (p.shape == BlockShape.L) s = "Ell(" + C(p.color) + ", " + p.rotation + ", " + p.x + ", " + p.y + ")";
        else s = "Rect(" + C(p.color) + ", " + p.width + ", " + p.height + ", " + p.x + ", " + p.y + ")";
        if (p.iceLayer > 0) s = "Ice(" + s + ", " + p.iceLayer + ")";
        if (p.hasInner) s = "Peeled(" + s + ", " + C(p.innerColor) + ")";
        return s;
    }

    static LevelDefinition WithPlacements(LevelDefinition t, List<Placement> placed)
    {
        LevelDefinition d = new LevelDefinition();
        d.levelID = t.levelID; d.gridSize = t.gridSize; d.moveLimit = t.moveLimit; d.maxTrackSlots = t.maxTrackSlots;
        d.targetHpScale = t.targetHpScale; d.monument = t.monument; d.placements = placed; d.gates = t.gates; d.parts = t.parts;
        return d;
    }

    static void Verify(int from, int to)
    {
        LevelSolver.HeuristicWeight = double.Parse(Environment.GetEnvironmentVariable("W") ?? "0");
        int bad = 0;
        foreach (LevelDefinition d in LevelLibrary.Range(from, to))
        {
            string err = d.Validate();
            if (err != null) { Console.WriteLine("L" + d.levelID + " INVALID: " + err); bad++; continue; }
            Stopwatch sw = Stopwatch.StartNew();
            LevelSolver.SolveResult free = LevelSolver.Solve(d, 30000, 300000);
            LevelSolver.SolveResult within = LevelSolver.SolveWithin(d, 30000, 300000, d.moveLimit);
            int movable = 0;
            foreach (Placement p in d.placements) if (!p.locked && p.color != BlockColor.Gray) movable++;
            string status = within.solved ? "OK" : "FAIL";
            if (!within.solved) bad++;
            Console.WriteLine("L" + d.levelID + " " + status + " " + d.title + " | blocks=" + movable + " limit=" + d.moveLimit + " slots=" + d.maxTrackSlots
                + " | par(free)=" + (free.solved ? free.moves + (free.exact ? "e" : "h") : "none") + " par(within)=" + (within.solved ? within.moves + (within.exact ? "e" : "h") : "none")
                + " | ammo R/B/Y=" + d.TotalAmmo(BlockColor.Red) + "/" + d.TotalAmmo(BlockColor.Blue) + "/" + d.TotalAmmo(BlockColor.Yellow)
                + " hp=" + d.TotalHp(BlockColor.Red) + "/" + d.TotalHp(BlockColor.Blue) + "/" + d.TotalHp(BlockColor.Yellow)
                + " | " + sw.ElapsedMilliseconds + "ms");
        }
        Console.WriteLine(bad == 0 ? "ALL OK" : ("PROBLEMS: " + bad));
    }


    // Debug: switch off mechanics in a template to find which one makes it unwinnable (env STRIP=ice,peel,shutter,scrap,parts).
    static LevelDefinition Strip(LevelDefinition t)
    {
        string strip = Environment.GetEnvironmentVariable("STRIP") ?? "";
        if (strip.Length == 0) return t;
        List<Placement> ps = new List<Placement>();
        foreach (Placement p0 in t.placements)
        {
            Placement p = p0;
            if (strip.Contains("scrap") && p.locked) continue;
            if (strip.Contains("ice")) p.iceLayer = 0;
            if (strip.Contains("peel")) p.hasInner = false;
            ps.Add(p);
        }
        t.placements = ps;
        if (strip.Contains("shutter"))
            for (int i = 0; i < t.gates.Count; i++) { GateSpec g = t.gates[i]; g.isShutterGate = false; t.gates[i] = g; }
        return t;
    }

    static void Search(int id, double seconds, int minPar, int maxPar)
    {
        LevelDefinition t = Strip(LevelLibrary.Build(id));
        List<Placement> fixedP = new List<Placement>(), movable = new List<Placement>();
        foreach (Placement p in t.placements) { if (!p.Unplaced) fixedP.Add(p); else movable.Add(p); }
        movable.Sort(delegate (Placement a, Placement b) { return b.Cells().Length.CompareTo(a.Cells().Length); });
        LevelSolver.HeuristicWeight = double.Parse(Environment.GetEnvironmentVariable("W") ?? "0");
        Random rng = new Random(id * 31 + 7 + int.Parse(Environment.GetEnvironmentVariable("SEED") ?? "0")); double bias = double.Parse(Environment.GetEnvironmentVariable("BIAS") ?? "0");
        int w = t.GridWidth, h = t.GridHeight;
        Stopwatch sw = Stopwatch.StartNew();
        int bestPar = -1; List<Placement> best = null; int tried = 0, solvedN = 0;
        while (sw.Elapsed.TotalSeconds < seconds)
        {
            tried++;
            bool[] occ = new bool[w * h];
            foreach (Placement p in fixedP) foreach (Int2 c in p.Cells()) occ[(p.y + c.y) * w + p.x + c.x] = true;
            List<Placement> placed = new List<Placement>(fixedP);
            bool ok = true;
            foreach (Placement m0 in movable)
            {
                Placement m = m0; Int2[] cells = m.Cells();
                int ex = 0, ey = 0; foreach (Int2 c in cells) { if (c.x > ex) ex = c.x; if (c.y > ey) ey = c.y; }
                bool done = false;
                for (int k = 0; k < 80 && !done; k++)
                {
                    int ox = rng.Next(0, w - ex), oy = rng.Next(0, h - ey);
                    if (rng.NextDouble() < bias)
                    {
                        List<GateSpec> mine = t.gates.FindAll(delegate (GateSpec g) { return g.color == m.color; });
                        if (mine.Count > 0)
                        {
                            GateSpec g = mine[rng.Next(0, mine.Count)];
                            int span = (g.direction == GateDirection.Up || g.direction == GateDirection.Down) ? (w - ex) : (h - ey);
                            int lo = Math.Max(0, g.start - (((g.direction == GateDirection.Up || g.direction == GateDirection.Down) ? ex : ey) + 1 - g.length));
                            int hi = Math.Min(span - 1, g.start + g.length - 1);
                            int lane = hi >= lo ? rng.Next(lo, hi + 1) : rng.Next(0, span);
                            int near = rng.Next(0, 3); // 0..2 cells away from the edge
                            if (g.direction == GateDirection.Up) { ox = lane; oy = h - 1 - ey - near; }
                            else if (g.direction == GateDirection.Down) { ox = lane; oy = near; }
                            else if (g.direction == GateDirection.Left) { oy = lane; ox = near; }
                            else { oy = lane; ox = w - 1 - ex - near; }
                            if (ox < 0 || oy < 0 || ox > w - 1 - ex || oy > h - 1 - ey) continue;
                        }
                    }
                    bool free = true;
                    foreach (Int2 c in cells) if (occ[(oy + c.y) * w + ox + c.x]) { free = false; break; }
                    if (!free) continue;
                    foreach (Int2 c in cells) occ[(oy + c.y) * w + ox + c.x] = true;
                    m.x = ox; m.y = oy; placed.Add(m); done = true;
                }
                if (!done) { ok = false; break; }
            }
            if (!ok) continue;
            LevelDefinition d = WithPlacements(t, placed);
            if (d.Validate() != null) continue;
            LevelSolver.SolveResult r = LevelSolver.SolveWithin(d, int.Parse(Environment.GetEnvironmentVariable("CAPB") ?? "20000"), int.Parse(Environment.GetEnvironmentVariable("CAPH") ?? "120000"), int.Parse(Environment.GetEnvironmentVariable("LIMIT") ?? t.moveLimit.ToString()));
            if (!r.solved) continue;
            solvedN++;
            if (maxPar > 0 && r.moves > maxPar) continue;
            if (r.moves > bestPar) { bestPar = r.moves; best = placed; Console.WriteLine("  trial " + tried + " par=" + r.moves + (r.exact ? " exact" : " heur")); }
            if (minPar > 0 && bestPar >= minPar) break;
        }
        Console.WriteLine("tried=" + tried + " solved=" + solvedN + " bestPar=" + bestPar + " limit=" + t.moveLimit);
        if (best != null) foreach (Placement p in best) Console.WriteLine("d.placements.Add(" + Fmt(p) + ");");
    }

    // Debug: can ANY exit order (ignoring board geometry) dismantle the monument with the available ammo?
    static bool Dfs(PuzzleState st, int depth, ref int nodes, List<int> order)
    {
        if (st.IsWon) return true;
        if (nodes++ > 3000000) return false;
        for (int i = 0; i < st.Board.BlockCount; i++)
        {
            if (st.Board.Exited[i] || st.Board.Dormant[i] || st.Board.Blocks[i].IsObstacle) continue;
            int col = (int)st.Board.Blocks[i].color;
            if (!st.CanExit(col)) continue;
            PuzzleState n = st.Clone();
            Move m = new Move(); m.block = i; m.exit = true;
            n.Apply(m);
            order.Add(i);
            if (Dfs(n, depth + 1, ref nodes, order)) return true;
            order.RemoveAt(order.Count - 1);
        }
        return false;
    }

    static void Mon(int id)
    {
        LevelDefinition d = LevelLibrary.Build(id);
        // give unplaced blocks dummy positions (geometry is ignored here)
        for (int i = 0; i < d.placements.Count; i++) { Placement p = d.placements[i]; if (p.Unplaced) { p.x = 0; p.y = 0; d.placements[i] = p; } }
        PuzzleState st = new PuzzleState(d);
        int nodes = 0; List<int> order = new List<int>();
        bool ok = Dfs(st, 0, ref nodes, order);
        Console.WriteLine("L" + id + " monument-only: " + (ok ? "WINNABLE order=" + string.Join(",", order.ConvertAll(delegate (int x) { return x.ToString(); }).ToArray()) : "NOT WINNABLE") + " nodes=" + nodes);
        Console.WriteLine("ammo R/B/Y=" + d.TotalAmmo(BlockColor.Red) + "/" + d.TotalAmmo(BlockColor.Blue) + "/" + d.TotalAmmo(BlockColor.Yellow) + " hp=" + d.TotalHp(BlockColor.Red) + "/" + d.TotalHp(BlockColor.Blue) + "/" + d.TotalHp(BlockColor.Yellow));
    }

    // Debug: print a layout and, for each block, whether it can ever reach an exit in isolation (other movable blocks removed).
    static void Dbg(int id, int seed)
    {
        LevelDefinition t = LevelLibrary.Build(id);
        Random rng = new Random(seed);
        int w = t.GridWidth, h = t.GridHeight;
        bool[] occ = new bool[w * h];
        List<Placement> placed = new List<Placement>();
        foreach (Placement p in t.placements) if (!p.Unplaced) { placed.Add(p); foreach (Int2 c in p.Cells()) occ[(p.y + c.y) * w + p.x + c.x] = true; }
        foreach (Placement m0 in t.placements)
        {
            if (!m0.Unplaced) continue;
            Placement m = m0; Int2[] cells = m.Cells(); int ex = 0, ey = 0; foreach (Int2 c in cells) { if (c.x > ex) ex = c.x; if (c.y > ey) ey = c.y; }
            for (int k = 0; k < 200; k++)
            {
                int ox = rng.Next(0, w - ex), oy = rng.Next(0, h - ey); bool free = true;
                foreach (Int2 c in cells) if (occ[(oy + c.y) * w + ox + c.x]) { free = false; break; }
                if (!free) continue;
                foreach (Int2 c in cells) occ[(oy + c.y) * w + ox + c.x] = true; m.x = ox; m.y = oy; placed.Add(m); break;
            }
        }
        LevelDefinition d = WithPlacements(t, placed);
        Console.WriteLine("validate: " + d.Validate());
        char[,] g = new char[w, h];
        for (int x = 0; x < w; x++) for (int y = 0; y < h; y++) g[x, y] = '.';
        for (int i = 0; i < placed.Count; i++) foreach (Int2 c in placed[i].Cells()) g[placed[i].x + c.x, placed[i].y + c.y] = placed[i].locked ? '#' : (char)('a' + i);
        for (int y = h - 1; y >= 0; y--) { string row = ""; for (int x = 0; x < w; x++) row += g[x, y]; Console.WriteLine(row); }
        BoardModel b = new BoardModel(d);
        for (int i = 0; i < b.BlockCount; i++)
        {
            if (b.Blocks[i].IsObstacle) continue;
            List<Move> mv = new List<Move>(); b.GenerateMoves(mv, null);
            int slides = 0, exits = 0; foreach (Move m in mv) if (m.block == i) { if (m.exit) exits++; else slides++; }
            Console.WriteLine("block " + (char)('a' + i) + " " + b.Blocks[i].color + " tiles=" + b.Blocks[i].Tiles + " ice=" + b.Ice[i] + " dormant=" + b.Dormant[i] + " slides=" + slides + " exits=" + exits);
        }
        LevelSolver.SolveResult r = LevelSolver.SolveWithin(d, 30000, int.Parse(Environment.GetEnvironmentVariable("CAPH") ?? "300000"), 80);
        Console.WriteLine("solve(<=80): " + r.solved + " moves=" + r.moves + " visited=" + r.visited);
    }

    static void Main(string[] args)
    {
        if (args.Length >= 3 && args[0] == "dbg") { Dbg(int.Parse(args[1]), int.Parse(args[2])); return; }
        if (args.Length >= 2 && args[0] == "mon") { Mon(int.Parse(args[1])); return; }
        if (args.Length >= 3 && args[0] == "search") { Search(int.Parse(args[1]), double.Parse(args[2]), args.Length > 3 ? int.Parse(args[3]) : 0, args.Length > 4 ? int.Parse(args[4]) : 0); return; }
        int from = args.Length > 1 ? int.Parse(args[0]) : 1;
        int to = args.Length > 1 ? int.Parse(args[1]) : 99;
        Verify(from, to);
    }
}


