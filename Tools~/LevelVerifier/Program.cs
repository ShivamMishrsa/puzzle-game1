using System;
using System.Collections.Generic;
using System.Diagnostics;
using ColorGateBlast;
using ColorGateBlast.EditorTools;

class Program
{
    static string C(BlockColor c) { return c == BlockColor.Red ? "R" : c == BlockColor.Blue ? "B" : c == BlockColor.Yellow ? "Y" : c == BlockColor.Green ? "G" : c == BlockColor.Purple ? "P" : c == BlockColor.Orange ? "O" : c == BlockColor.Cyan ? "C" : "X"; }

    static string Fmt(Placement p)
    {
        string s;
        if (p.locked) s = "Scrap(" + p.width + ", " + p.height + ", " + p.x + ", " + p.y + ")";
        else if (p.shape == BlockShape.L) s = "Ell(" + C(p.color) + ", " + p.rotation + ", " + p.x + ", " + p.y + ")";
        else s = "Rect(" + C(p.color) + ", " + p.width + ", " + p.height + ", " + p.x + ", " + p.y + ")";
        if (p.arrow == MoveAxis.Horizontal) s = "Arrow(" + s + ", Horizontal)";
        if (p.arrow == MoveAxis.Vertical) s = "Arrow(" + s + ", Vertical)";
        if (p.iceLayer > 0) s = "Ice(" + s + ", " + p.iceLayer + ")";
        if (p.hasInner && p.innerHasKey) s = "PeeledKey(" + s + ", " + C(p.innerColor) + ", " + p.lockId + ")";
        else if (p.hasInner) s = "Peeled(" + s + ", " + C(p.innerColor) + ")";
        if (p.hasBrassKey) s = "Key(" + s + ", " + p.lockId + ")";
        if (p.cycle != null && p.cycle.Length > 1)
        {
            string cs = "";
            foreach (BlockColor c in p.cycle) cs += ", " + C(c);
            s = "Prism(" + s + cs + ")";
        }
        if (p.linkGroup != 0) s = "Link(" + s + ", " + p.linkGroup + ")";
        if (p.crate) s = "Crate(" + s + ")";
        return s;
    }

    static LevelDefinition WithPlacements(LevelDefinition t, List<Placement> placed)
    {
        LevelDefinition d = new LevelDefinition();
        d.levelID = t.levelID; d.gridSize = t.gridSize; d.moveLimit = t.moveLimit; d.maxTrackSlots = t.maxTrackSlots;
        d.targetHpScale = t.targetHpScale; d.monument = t.monument; d.placements = placed; d.gates = t.gates; d.parts = t.parts; d.portals = t.portals; d.magnets = t.magnets; d.carriages = t.carriages; d.turnstiles = t.turnstiles; d.repulsors = t.repulsors; d.megaPierces = t.megaPierces; d.timeLimit = t.timeLimit; d.targetHpScale = t.targetHpScale;
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
            if (!within.solved) within = LevelSolver.SolveWithin(d, 200000, 1500000, d.moveLimit);
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
                    if (m.linkGroup != 0)
                    {
                        Placement mate = placed.Find(delegate (Placement q) { return q.linkGroup == m.linkGroup; });
                        if (mate.linkGroup != 0 && Math.Abs(mate.x - ox) + Math.Abs(mate.y - oy) > 2) continue;
                    }
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
            int col = (int)st.Board.ColorOf(i);
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

    // ---- mechanics self test (welded pairs, steam gates, prism blocks, magnet pads) ----
    static int selfFail = 0;
    static void Check(bool ok, string what) { Console.WriteLine((ok ? "PASS " : "FAIL ") + what); if (!ok) selfFail++; }

    static LevelDefinition Tiny(int w, int h)
    {
        LevelDefinition d = new LevelDefinition();
        d.gridSize = new Int2(w, h); d.moveLimit = 20; d.maxTrackSlots = 5;
        PartSpec p = new PartSpec(); p.type = PartType.Casing; p.color = BlockColor.Red; p.hp = 3; d.parts.Add(p);
        return d;
    }
    static Placement Blk(BlockColor c, int x, int y) { Placement p = new Placement(); p.color = c; p.shape = BlockShape.Rect; p.width = 1; p.height = 1; p.x = x; p.y = y; return p; }
    static GateSpec Gt(GateDirection dir, int start, int len, BlockColor c) { GateSpec g = new GateSpec(); g.direction = dir; g.start = start; g.length = len; g.color = c; return g; }

    static void SelfTest()
    {
        // welded pair: red (1,1) + blue (2,1); red gate Left at row 1
        LevelDefinition e = Tiny(4, 4);
        Placement ra = Blk(BlockColor.Red, 1, 1); ra.linkGroup = 1; Placement rb = Blk(BlockColor.Blue, 2, 1); rb.linkGroup = 1;
        e.placements.Add(ra); e.placements.Add(rb);
        e.gates.Add(Gt(GateDirection.Left, 1, 1, BlockColor.Red));
        BoardModel me = new BoardModel(e);
        Check(me.Linked[0] && me.Linked[1], "pair starts linked");
        Check(me.Probe(0, new Int2(-1, 0)) == StepResult.Moved, "pair: both halves can step left");
        Check(me.Probe(1, new Int2(0, 1)) == StepResult.Moved, "pair: both halves can step up");
        me.Commit(0, new Int2(0, 1));
        Check(me.Pos[0].x == 0 && me.Pos[1].x == 1, "pair: partner slid along (red x=0, blue x=1)");
        Check(me.Probe(0, new Int2(-1, 0)) == StepResult.Exit, "pair: red exits left, blue steps into the vacated cell");
        Check(me.Probe(1, new Int2(-1, 0)) == StepResult.Blocked, "pair: dragging the blue half toward the red gate does nothing (blue has no gate)");
        ExitEffects fx = new ExitEffects();
        me.ExitBlock(0, fx, GateDirection.Left, me.Pos[0], me.Pos[0]);
        Check(!me.Linked[1] && me.Pos[1].x == 0 && fx.linkBroken == 1, "pair: link broken, blue stepped to (0,1)");
        Check(me.Probe(1, new Int2(0, 1)) == StepResult.Moved, "pair: freed blue moves on its own");
        // a blocked partner blocks the pair
        LevelDefinition f = Tiny(4, 4);
        Placement fa = Blk(BlockColor.Red, 1, 1); fa.linkGroup = 1; Placement fb = Blk(BlockColor.Blue, 2, 1); fb.linkGroup = 1;
        f.placements.Add(fa); f.placements.Add(fb); f.placements.Add(Blk(BlockColor.Yellow, 2, 2));
        BoardModel mf = new BoardModel(f);
        Check(mf.Probe(0, new Int2(0, 1)) == StepResult.Blocked, "pair: a blocker above the partner blocks the whole pair");

        // steam gate: interval 2 -> moves 0,1 normal; 2,3 pressurized
        LevelDefinition sd = Tiny(4, 3);
        sd.placements.Add(Blk(BlockColor.Red, 1, 0));
        GateSpec sg = Gt(GateDirection.Down, 1, 1, BlockColor.Red); sg.steamInterval = 2; sd.gates.Add(sg);
        BoardModel sb = new BoardModel(sd);
        Check(!sb.SteamPressurized(sg), "steam: normal at move 0");
        sb.CountMove(); sb.CountMove();
        Check(sb.SteamPressurized(sg), "steam: pressurized after 2 moves");
        ExitEffects sfx = new ExitEffects();
        sb.ExitBlock(0, sfx, GateDirection.Down, sb.Pos[0], sb.Pos[0]);
        Check(sfx.bonusAmmo == 2, "steam: exit while pressurized gives +2 ammo");
        PuzzleState ps = new PuzzleState(sd);
        ps.Board.CountMove(); ps.Board.CountMove();
        Move mv = new Move(); mv.block = 0; mv.exit = true; mv.dir = GateDirection.Down; mv.to = ps.Board.Pos[0];
        ps.Apply(mv);
        Check(ps.Monument.Hp[0] == 3 - 3 || ps.Monument.AliveCount == 0, "steam: 1 tile + 2 bonus ammo destroys a 3-HP part (alive=" + ps.Monument.AliveCount + ")");

        // prism: cycles Blue -> Red; after one exit it matches the red gate
        LevelDefinition pd = Tiny(4, 4);
        Placement prism = Blk(BlockColor.Blue, 1, 1); prism.cycle = new BlockColor[] { BlockColor.Blue, BlockColor.Red };
        pd.placements.Add(prism);
        pd.placements.Add(Blk(BlockColor.Yellow, 3, 3));
        pd.gates.Add(Gt(GateDirection.Down, 1, 1, BlockColor.Red));
        pd.gates.Add(Gt(GateDirection.Up, 3, 1, BlockColor.Yellow));
        BoardModel pb = new BoardModel(pd);
        Check(pb.ColorOf(0) == BlockColor.Blue, "prism: starts blue");
        Check(pb.Probe(0, new Int2(0, -1)) == StepResult.Moved, "prism: blue cannot use the red gate yet (moves to (1,0))");
        pb.Commit(0, new Int2(1, 0));
        Check(pb.Probe(0, new Int2(0, -1)) == StepResult.Blocked, "prism: blue at the red gate is blocked");
        pb.ExitBlock(1, null, GateDirection.Up, pb.Pos[1], pb.Pos[1]);
        Check(pb.ColorOf(0) == BlockColor.Red, "prism: turns red after another block exits");
        Check(pb.Probe(0, new Int2(0, -1)) == StepResult.Exit, "prism: now exits through the red gate");

        // magnets: pad at (2,1) pulling Right: a block resting at (1,1) is pulled onto (2,1)
        LevelDefinition md = Tiny(4, 3);
        md.placements.Add(Blk(BlockColor.Red, 0, 1));
        md.magnets.Add(new MagnetSpec { cell = new Int2(2, 1), pull = GateDirection.Right });
        BoardModel mb = new BoardModel(md);
        mb.Commit(0, new Int2(1, 1));
        int pulls = mb.PullAll(0);
        Check(pulls == 1 && mb.Pos[0].x == 2, "magnet: block resting at (1,1) pulled to (2,1) (pulls=" + pulls + ", x=" + mb.Pos[0].x + ")");
        Check(mb.PullAll(0) == 0, "magnet: block on the pad is not pulled further");

        // crates: closed crate cannot exit; opens when a neighbour leaves
        LevelDefinition cd = Tiny(5, 4);
        Placement crate = Blk(BlockColor.Red, 1, 1); crate.crate = true;
        cd.placements.Add(crate);
        cd.placements.Add(Blk(BlockColor.Blue, 2, 1));
        cd.gates.Add(Gt(GateDirection.Down, 1, 1, BlockColor.Red));
        cd.gates.Add(Gt(GateDirection.Down, 2, 1, BlockColor.Blue));
        BoardModel cb = new BoardModel(cd);
        Check(cb.Crated[0], "crate: starts closed");
        cb.Commit(0, new Int2(1, 0));
        Check(cb.Probe(0, new Int2(0, -1)) == StepResult.Blocked, "crate: closed crate cannot leave through its gate");
        cb.Commit(0, new Int2(1, 1));
        ExitEffects cfx = new ExitEffects();
        cb.ExitBlock(1, cfx, GateDirection.Down, cb.Pos[1], cb.Pos[1]);
        Check(!cb.Crated[0] && cfx.revealed.Count == 1, "crate: opens when the adjacent block leaves");
        cb.Commit(0, new Int2(1, 0));
        Check(cb.Probe(0, new Int2(0, -1)) == StepResult.Exit, "crate: opened block can use its gate");

        // carriages: row 0 slides right after every exit, bounces at the wall
        LevelDefinition rd = Tiny(4, 3);
        rd.placements.Add(Blk(BlockColor.Red, 0, 0));
        rd.placements.Add(Blk(BlockColor.Blue, 3, 2));
        rd.gates.Add(Gt(GateDirection.Up, 3, 1, BlockColor.Blue));
        rd.carriages.Add(new CarriageSpec { row = 0, dir = 1 });
        BoardModel cab = new BoardModel(rd);
        ExitEffects rfx = new ExitEffects();
        cab.ExitBlock(1, rfx, GateDirection.Up, cab.Pos[1], cab.Pos[1]);
        Check(cab.Pos[0].x == 1 && rfx.shifted.Count == 1, "carriage: red slid from x=0 to x=1");
        cab.ApplyCarriages(null); cab.ApplyCarriages(null);
        Check(cab.Pos[0].x == 3, "carriage: red reached the right wall");
        cab.ApplyCarriages(null);
        Check(cab.Pos[0].x == 2 && cab.CarriageDir[0] == -1, "carriage: bounced back (x=" + cab.Pos[0].x + ")");

        // turnstiles: a tile at (2,1) that only accepts blocks sliding Right
        LevelDefinition td = Tiny(5, 3);
        td.placements.Add(Blk(BlockColor.Red, 0, 1));
        td.placements.Add(Blk(BlockColor.Blue, 4, 0));
        td.turnstiles.Add(new TurnstileSpec { cell = new Int2(2, 1), dir = GateDirection.Right });
        BoardModel tb = new BoardModel(td);
        Check(tb.Probe(0, new Int2(1, 0)) == StepResult.Moved, "turnstile: red may step toward the tile from the left");
        tb.Commit(0, new Int2(1, 1));
        Check(tb.Probe(0, new Int2(1, 0)) == StepResult.Moved, "turnstile: red may slide ONTO the tile moving Right");
        tb.Commit(0, new Int2(2, 1));
        Check(tb.Probe(0, new Int2(1, 0)) == StepResult.Moved, "turnstile: red may keep sliding Right across it");
        tb.Commit(0, new Int2(3, 1));
        Check(tb.Probe(0, new Int2(-1, 0)) == StepResult.Blocked, "turnstile: red cannot slide back Left onto the tile");
        Check(tb.TurnstileBlocks(0, tb.Pos[0], new Int2(-1, 0)), "turnstile: ratchet click reported for the wrong direction");
        tb.Commit(0, new Int2(3, 2));
        tb.Commit(0, new Int2(2, 2));
        Check(tb.Probe(0, new Int2(0, -1)) == StepResult.Blocked, "turnstile: red cannot drop onto the tile from above");

        // repulsors: coil at (2,1): a block coming to rest next to it is shoved one tile away
        LevelDefinition rpd = Tiny(6, 4);
        rpd.placements.Add(Blk(BlockColor.Red, 0, 1));
        rpd.repulsors.Add(new RepulsorSpec { cell = new Int2(3, 1) });
        BoardModel rpb = new BoardModel(rpd);
        rpb.Commit(0, new Int2(2, 1));
        bool pushed = rpb.RepelAfterSlide(0, new Int2(1, 0));
        Check(pushed && rpb.Pos[0].x == 1, "repulsor: block resting beside the coil is pushed one tile away (x=" + rpb.Pos[0].x + ")");
        rpb.Commit(0, new Int2(3, 1));
        pushed = rpb.RepelAfterSlide(0, new Int2(1, 0));
        Check(pushed && rpb.Pos[0].x == 4, "repulsor: block resting ON the coil continues one tile in its move direction");
        rpb.Commit(0, new Int2(5, 1));
        Check(!rpb.RepelAfterSlide(0, new Int2(1, 0)), "repulsor: far away block is not touched");

        Console.WriteLine(selfFail == 0 ? "SELFTEST ALL PASS" : ("SELFTEST FAILURES: " + selfFail));
    }

    // Tighten a level's move count: keep asking the solver for a shorter solution until it cannot find one.
    static void Opt(int from, int to)
    {
        int bfs = int.Parse(Environment.GetEnvironmentVariable("OPTB") ?? "200000"), heu = int.Parse(Environment.GetEnvironmentVariable("OPTH") ?? "1500000");
        foreach (LevelDefinition d in LevelLibrary.Range(from, to))
        {
            Stopwatch sw = Stopwatch.StartNew();
            LevelSolver.SolveResult best = LevelSolver.SolveWithin(d, bfs, heu, d.moveLimit);
            if (!best.solved) { Console.WriteLine("L" + d.levelID + " OPT FAIL"); continue; }
            while (best.moves > 1)
            {
                LevelSolver.SolveResult r = LevelSolver.SolveWithin(d, bfs, heu, best.moves - 1);
                if (!r.solved) break;
                best = r;
            }
            Console.WriteLine("L" + d.levelID + " OPT " + best.moves + (best.exact ? "e" : "h") + " (limit was " + d.moveLimit + ") " + sw.ElapsedMilliseconds + "ms");
        }
    }

    static void Main(string[] args)
    {
        if (args.Length >= 3 && args[0] == "opt") { Opt(int.Parse(args[1]), int.Parse(args[2])); return; }
        if (args.Length >= 1 && args[0] == "selftest") { SelfTest(); return; }
        if (args.Length >= 3 && args[0] == "dbg") { Dbg(int.Parse(args[1]), int.Parse(args[2])); return; }
        if (args.Length >= 2 && args[0] == "mon") { Mon(int.Parse(args[1])); return; }
        if (args.Length >= 3 && args[0] == "search") { Search(int.Parse(args[1]), double.Parse(args[2]), args.Length > 3 ? int.Parse(args[3]) : 0, args.Length > 4 ? int.Parse(args[4]) : 0); return; }
        int from = args.Length > 1 ? int.Parse(args[0]) : 1;
        int to = args.Length > 1 ? int.Parse(args[1]) : 99;
        Verify(from, to);
    }
}


