#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// Editor-only autoplay test bot. Start Unity with <c>-cgbBot [-cgbFrom N] [-cgbTo M]</c> and enter Play Mode
    /// (see Editor/AutoPlayLauncher). For every level it asks the solver for the next move from the LIVE game state,
    /// performs it through the real drag code (GridManager.AutoMove), waits for the conveyor / monument to settle
    /// and repeats until the level is Won or Lost. Any exception or error log is counted per level.
    /// Results are written to autoplay_results.txt next to the Assets folder.
    /// </summary>
    public sealed class AutoPlayer : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-cgbBot") < 0) return;
            GameObject go = new GameObject("AutoPlayer");
            DontDestroyOnLoad(go);
            go.AddComponent<AutoPlayer>();
        }

        private readonly List<string> errors = new List<string>();
        private readonly StringBuilder report = new StringBuilder();

        private static int Arg(string name, int def)
        {
            string[] a = Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length - 1; i++) if (a[i] == name) { int v; if (int.TryParse(a[i + 1], out v)) return v; }
            return def;
        }

        private void OnEnable() { Application.logMessageReceived += OnLog; }
        private void OnDisable() { Application.logMessageReceived -= OnLog; }

        private void OnLog(string cond, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
                errors.Add(type + ": " + cond + (stack != null && stack.Length > 0 ? " @ " + stack.Split('\n')[0] : ""));
        }

        private void Start() { StartCoroutine(Run()); }

        private IEnumerator Run()
        {
            LevelProgressor lp = null;
            float wait = 0f;
            while (lp == null || lp.levels == null || lp.levels.Length == 0)
            {
                lp = FindFirstObjectByType<LevelProgressor>();
                wait += Time.unscaledDeltaTime;
                if (wait > 30f) { Finish(3, "LevelProgressor / levels not found"); yield break; }
                yield return null;
            }
            yield return new WaitForSeconds(1f);

            lp.ClockDisabled = true;
            int from = Arg("-cgbFrom", 1), to = Arg("-cgbTo", 999);
            int won = 0, lost = 0, other = 0, errLevels = 0;
            for (int idx = 0; idx < lp.levels.Length; idx++)
            {
                int num = lp.levels[idx].LevelNumber;
                if (num < from || num > to) continue;

                errors.Clear();
                lp.StartLevel(idx);
                yield return new WaitForSeconds(0.4f);

                string result = "TIMEOUT";
                int steps = 0, rejected = 0;
                string predKey = "", predNoTrack = "";
                PuzzleState predState = null;
                System.Collections.Generic.List<Move> plan = new System.Collections.Generic.List<Move>();
                bool storedPlan = false, storedBroken = false;
                if (lp.Level.solution != null && lp.Level.solution.Length > 0)
                {
                    for (int si = 0; si < lp.Level.solution.Length; si++) plan.Add(LevelSolver.Unpack(lp.Level.solution[si]));
                    storedPlan = true;
                }
                float t0 = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - t0 < 900f && steps < 90)
                {
                    if (lp.State == GameState.Won) { result = "WON"; break; }
                    if (lp.State == GameState.Lost) { result = "LOST " + lp.LastLoseReason; break; }

                    // wait until the conveyor / monument stopped changing by themselves
                    float w0 = Time.realtimeSinceStartup;
                    while (lp.State == GameState.Playing && Time.realtimeSinceStartup - w0 < 25f
                           && (!lp.track.IsSettled() || lp.monument.InFlight > 0))
                    {
                        if (Time.timeScale > 0.01f) Time.timeScale = 6f;
                        yield return null;
                    }
                    Time.timeScale = 6f;
                    yield return new WaitForSeconds(0.25f);
                    if (lp.State != GameState.Playing) continue;
                    if (lp.grid.RemainingBlocks == 0 && lp.monument.AliveCount == 0) { yield return new WaitForSeconds(0.5f); continue; }

                    PuzzleState snap = new PuzzleState(lp.grid.Level, lp.grid.Board, lp.monument.Model, lp.track.Snapshot());
                    if (plan.Count > 0 && predKey.Length > 0 && snap.Key() != predKey)
                    {
                        if (storedPlan && snap.KeyNoTrack() == predNoTrack)
                        {
                            /* only the belt differs: keep following the stored solution */
                            System.Text.StringBuilder bd = new System.Text.StringBuilder();
                            bd.Append(" liveParked=");
                            foreach (TurretInfo ti in snap.Parked) bd.Append(ti.color + ":" + ti.ammo + (ti.piercing ? "p" : "") + ",");
                            bd.Append(" predParked=");
                            foreach (TurretInfo ti in predState.Parked) bd.Append(ti.color + ":" + ti.ammo + (ti.piercing ? "p" : "") + ",");
                            bd.Append(" liveFindRed=" + lp.monument.FindTarget(0) + " snapFindRed=" + snap.Monument.FindTarget(0) + " predFindRed=" + predState.Monument.FindTarget(0) + " settled=" + lp.track.IsSettled() + " inflight=" + lp.monument.InFlight + " screwMask0=" + lp.monument.Model.ScrewMask[0] + "/" + snap.Monument.ScrewMask[0]); Debug.Log("[BOT] BELT-ONLY DIFF at step " + steps + bd);
                        }
                        else
                        {
                            if (storedPlan)
                            {
                                storedBroken = true;
                                System.Text.StringBuilder dg = new System.Text.StringBuilder();
                                dg.Append(" liveHp=");
                                for (int hi = 0; hi < snap.Monument.Hp.Length; hi++) dg.Append(snap.Monument.Hp[hi]).Append(',');
                                dg.Append(" predHp=");
                                for (int hi = 0; hi < predState.Monument.Hp.Length; hi++) dg.Append(predState.Monument.Hp[hi]).Append(',');
                                dg.Append(" liveParked=");
                                foreach (TurretInfo ti in snap.Parked) dg.Append(ti.color + ":" + ti.ammo + ",");
                                dg.Append(" predParked=");
                                foreach (TurretInfo ti in predState.Parked) dg.Append(ti.color + ":" + ti.ammo + ",");
                                Debug.Log("[BOT] STORED SOLUTION DIVERGED at step " + steps + dg);
                            }
                            plan.Clear();
                        }
                    }
                    if (plan.Count == 0)
                    {
                        LevelSolver.SolveResult r = LevelSolver.Solve(snap, 30000, 300000, lp.MovesLeft);
                        if (!r.solved) r = LevelSolver.Solve(snap, 200000, 1500000, lp.MovesLeft);
                        if (!r.solved) r = LevelSolver.Solve(snap, 30000, 300000);
                        if (!r.solved || r.moves == 0) { result = "NO_SOLUTION_FROM_LIVE_STATE"; break; }
                        plan = new System.Collections.Generic.List<Move>(r.path);
                    }
                    Move mv = plan[0];
                    plan.RemoveAt(0);
                    if (storedPlan && lp.Level.levelID == 135)
                    {
                        System.Text.StringBuilder st = new System.Text.StringBuilder();
                        st.Append("[BOT] STEP " + steps + " blk" + mv.block + "(" + snap.Board.ColorOf(mv.block) + " tiles " + snap.Board.Blocks[mv.block].Tiles + ") exit=" + mv.exit + " liveParked=");
                        foreach (TurretInfo ti in snap.Parked) st.Append(ti.color + ":" + ti.ammo + ",");
                        st.Append(" hpLive=");
                        for (int hi = 0; hi < snap.Monument.Hp.Length; hi++) st.Append(snap.Monument.Hp[hi]).Append(",");
                        Debug.Log(st.ToString());
                    }
                    PuzzleState pred = snap.Clone();
                    pred.Apply(mv);
                    if (storedPlan && lp.Level.levelID == 135)
                    {
                        System.Text.StringBuilder sp2 = new System.Text.StringBuilder("[BOT]   predAfter parked=");
                        foreach (TurretInfo ti in pred.Parked) sp2.Append(ti.color + ":" + ti.ammo + ",");
                        sp2.Append(" hp=");
                        for (int hi = 0; hi < pred.Monument.Hp.Length; hi++) sp2.Append(pred.Monument.Hp[hi]).Append(",");
                        Debug.Log(sp2.ToString());
                    }
                    predKey = pred.Key();
                    predNoTrack = pred.KeyNoTrack();
                    predState = pred;
                    bool ok = lp.grid.AutoMove(mv);
                    if (true)
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        BoardModel lb = lp.grid.Board;
                        for (int bi = 0; bi < lb.BlockCount; bi++)
                        {
                            bool pe = pred.Board.Exited[bi], le = lb.Exited[bi];
                            if (pe != le || (!pe && !le && !lb.Dormant[bi] && (pred.Board.Pos[bi] != lb.Pos[bi] || pred.Board.Crated[bi] != lb.Crated[bi] || pred.Board.Ice[bi] != lb.Ice[bi])))
                                sb.Append(" block" + bi + "(" + lb.Blocks[bi].color + ") pred=" + (pe ? "exited" : pred.Board.Pos[bi].ToString()) + " live=" + (le ? "exited" : lb.Pos[bi].ToString()));
                        }
                        if (sb.Length > 0) { plan.Clear(); Debug.Log("[BOT] DIVERGE-CTX canExitBefore=" + snap.CanExit((int)snap.Board.ColorOf(mv.block)) + " parked=" + snap.Parked.Count + " lockedGates? movesLeft=" + lp.MovesLeft + " blockCells=" + snap.Board.Pos[mv.block] + " dormant=" + snap.Board.Dormant[mv.block] + " crated=" + snap.Board.Crated[mv.block] + " ice=" + snap.Board.Ice[mv.block]); Debug.Log("[BOT] DIVERGE at step " + steps + " move block" + mv.block + " dir " + mv.dir + " to " + mv.to + " exit=" + mv.exit + ":" + sb); }
                    }
                    steps++;
                    if (!ok && ++rejected > 3) { result = "MOVE_REJECTED"; break; }
                    yield return null;
                }
                if (result == "TIMEOUT") Debug.Log("[BOT] TIMEOUT-DIAG state=" + lp.State + " remaining=" + lp.grid.RemainingBlocks + " alive=" + lp.monument.AliveCount + " movesLeft=" + lp.MovesLeft + " settled=" + lp.track.IsSettled() + " inflight=" + lp.monument.InFlight);
                if (result == "TIMEOUT" && lp.State == GameState.Won) result = "WON";
                for (int f = 0; f < 90 && lp.State == GameState.Playing && result == "TIMEOUT"; f++) yield return null;
                if (lp.State == GameState.Won) result = "WON";
                else if (lp.State == GameState.Lost && result == "TIMEOUT") result = "LOST " + lp.LastLoseReason;

                int used = lp.Level.moveLimit - lp.MovesLeft;
                string line = "[BOT] Level " + num + " '" + lp.Level.title + "': " + result + " | moves used " + used + "/" + lp.Level.moveLimit +
                              " | solver steps " + steps + " | errors " + errors.Count + (storedPlan ? (storedBroken ? " | stored=DIVERGED" : " | stored=ok") : "");
                report.AppendLine(line);
                Debug.Log(line);
                for (int e = 0; e < errors.Count && e < 4; e++) report.AppendLine("      " + errors[e]);
                if (result == "WON") won++; else if (result == "LOST") lost++; else other++;
                if (errors.Count > 0) errLevels++;
            }
            string sum = "[BOT] SUMMARY won=" + won + " lost=" + lost + " other=" + other + " levelsWithErrors=" + errLevels;
            report.AppendLine(sum);
            Finish(lost + other + errLevels == 0 ? 0 : 2, sum);
        }

        private void Finish(int code, string message)
        {
            Time.timeScale = 1f;
            Debug.Log("[BOT] " + message);
            try { File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath, "../autoplay_results.txt")), report.ToString()); } catch (Exception) { }
            UnityEditor.EditorApplication.Exit(code);
        }
    }
}
#endif
