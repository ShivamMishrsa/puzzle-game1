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
                float t0 = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - t0 < 90f && steps < 80)
                {
                    if (lp.State == GameState.Won) { result = "WON"; break; }
                    if (lp.State == GameState.Lost) { result = "LOST"; break; }

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
                    LevelSolver.SolveResult r = LevelSolver.Solve(snap, 30000, 300000, lp.MovesLeft);
                    if (!r.solved) r = LevelSolver.Solve(snap, 30000, 300000);
                    if (!r.solved || r.moves == 0) { result = "NO_SOLUTION_FROM_LIVE_STATE"; break; }
                    bool ok = lp.grid.AutoMove(r.firstMove);
                    steps++;
                    if (!ok && ++rejected > 3) { result = "MOVE_REJECTED"; break; }
                    yield return null;
                }
                if (result == "TIMEOUT" && lp.State == GameState.Won) result = "WON";
                for (int f = 0; f < 90 && lp.State == GameState.Playing && result == "TIMEOUT"; f++) yield return null;
                if (lp.State == GameState.Won) result = "WON";
                else if (lp.State == GameState.Lost && result == "TIMEOUT") result = "LOST";

                int used = lp.Level.moveLimit - lp.MovesLeft;
                string line = "[BOT] Level " + num + " '" + lp.Level.title + "': " + result + " | moves used " + used + "/" + lp.Level.moveLimit +
                              " | solver steps " + steps + " | errors " + errors.Count;
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
