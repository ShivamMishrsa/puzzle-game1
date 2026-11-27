using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ColorGateBlast.EditorTools
{
    /// <summary>
    /// Tools &gt; Color Gate Blast: builds Level_NNN LevelData assets from the hand-authored LevelNN classes,
    /// proving with the solver that each one can be won within its move limit before saving it.
    /// </summary>
    public static class LevelAssetTools
    {
        private const string LevelsPath = "Assets/_Project/Resources/Levels";

        [MenuItem("Tools/Color Gate Blast/Generate Levels 1-10", false, 10)]
        public static void Generate1To10() { GenerateRange(1, 10); }

        [MenuItem("Tools/Color Gate Blast/Generate Levels 11-20", false, 11)]
        public static void Generate11To20() { GenerateRange(11, 20); }

        [MenuItem("Tools/Color Gate Blast/Generate Levels 21-30", false, 12)]
        public static void Generate21To30() { GenerateRange(21, 30); }

        [MenuItem("Tools/Color Gate Blast/Generate Levels 31-40", false, 13)]
        public static void Generate31To40() { GenerateRange(31, 40); }

        [MenuItem("Tools/Color Gate Blast/Generate Levels 41-50", false, 14)]
        public static void Generate41To50() { GenerateRange(41, 50); }

        [MenuItem("Tools/Color Gate Blast/Generate Levels 51-60", false, 15)]
        public static void Generate51To60() { GenerateRange(51, 60); }

        [MenuItem("Tools/Color Gate Blast/Generate Levels 61-70", false, 16)]
        public static void Generate61To70() { GenerateRange(61, 70); }

        [MenuItem("Tools/Color Gate Blast/Generate Levels 71-80", false, 17)]
        public static void Generate71To80() { GenerateRange(71, 80); }

        [MenuItem("Tools/Color Gate Blast/Generate Levels 81-90", false, 18)]
        public static void Generate81To90() { GenerateRange(81, 90); }

        [MenuItem("Tools/Color Gate Blast/Generate Levels 91-100", false, 19)]
        public static void Generate91To100() { GenerateRange(91, 100); }

        [MenuItem("Tools/Color Gate Blast/Verify All Levels (solver)", false, 30)]
        public static void VerifyAll()
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { LevelsPath });
            int bad = 0;
            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    EditorUtility.DisplayProgressBar("Verifying levels", path, i / (float)guids.Length);
                    LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                    string err = data.definition.Validate();
                    LevelSolver.SolveResult r = err == null
                        ? SolveExact(data.definition)
                        : null;
                    if (err != null || !r.solved)
                    {
                        bad++;
                        Debug.LogError("[Color Gate Blast] " + path + " is NOT winnable within its move limit" + (err != null ? " (" + err + ")" : ""));
                    }
                }
            }
            finally { EditorUtility.ClearProgressBar(); }
            Debug.Log("[Color Gate Blast] Verified " + guids.Length + " level(s): " + (guids.Length - bad) + " OK, " + bad + " broken.");
        }

        /// <summary>Fast search first; the zero-margin levels whose exact optimum needs a deeper search get a second, bigger budget.</summary>
        private static LevelSolver.SolveResult SolveExact(LevelDefinition def)
        {
            LevelSolver.SolveResult r = LevelSolver.SolveWithin(def, 30000, 300000, def.moveLimit);
            if (!r.solved) r = LevelSolver.SolveWithin(def, 200000, 1500000, def.moveLimit);
            return r;
        }

        public static void GenerateRange(int from, int to)
        {
            EnsureFolder(LevelsPath);
            int ok = 0, failed = 0;
            try
            {
                for (int id = from; id <= to; id++)
                {
                    EditorUtility.DisplayProgressBar("Generating levels", "Level " + id + " (build + solve)", (id - from) / (float)(to - from + 1));
                    LevelDefinition def = LevelLibrary.Build(id);
                    if (def == null)
                    {
                        failed++;
                        Debug.LogError("[Color Gate Blast] No LevelNN class found for level " + id);
                        continue;
                    }
                    string err = def.Validate();
                    if (err != null)
                    {
                        failed++;
                        Debug.LogError("[Color Gate Blast] Level " + id + " is invalid: " + err);
                        continue;
                    }

                    LevelSolver.SolveResult within = SolveExact(def);
                    if (!within.solved)
                    {
                        failed++;
                        Debug.LogError("[Color Gate Blast] Level " + id + " could NOT be solved within its " + def.moveLimit + "-move limit. Not saved.");
                        continue;
                    }
                    def.parMoves = within.moves;

                    string path = LevelsPath + "/Level_" + id.ToString("000") + ".asset";
                    LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                    if (data == null)
                    {
                        data = ScriptableObject.CreateInstance<LevelData>();
                        AssetDatabase.CreateAsset(data, path);
                    }
                    def.timeLimit = def.ComputeTimeLimit(within.moves);
                    int distinctColours = 0;
                    {
                        System.Collections.Generic.HashSet<BlockColor> seen = new System.Collections.Generic.HashSet<BlockColor>();
                        for (int pi = 0; pi < def.placements.Count; pi++) if (def.placements[pi].color != BlockColor.Gray) seen.Add(def.placements[pi].color);
                        for (int pi = 0; pi < def.placements.Count; pi++) if (def.placements[pi].cycle != null) for (int ci = 0; ci < def.placements[pi].cycle.Length; ci++) seen.Add(def.placements[pi].cycle[ci]);
                        distinctColours = seen.Count;
                    }
                    Debug.Log("[Color Gate Blast] Level " + id + " grid=" + def.GridWidth + "x" + def.GridHeight + " colours=" + distinctColours + " slots=" + def.maxTrackSlots + ": optimalMoveCount=" + within.moves + (within.exact ? " (proven optimal)" : " (best found)") +
                              "  moveLimit=" + def.moveLimit + "  extra moves=" + (def.moveLimit - within.moves) + "  clock=" + def.timeLimit + "s");
                    data.definition = def;
                    EditorUtility.SetDirty(data);
                    ok++;
                    Debug.Log("[Color Gate Blast] Level " + id + " '" + def.title + "' " + def.GridWidth + "x" + def.GridHeight +
                              ", slots " + (def.UnlimitedTrack ? "unlimited" : def.maxTrackSlots.ToString()) +
                              ", limit " + def.moveLimit + ", solver " + within.moves + (within.exact ? " (optimal)" : " (upper bound)") + " - OK");
                }
            }
            finally { EditorUtility.ClearProgressBar(); }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Color Gate Blast] Generated " + ok + " level(s), " + failed + " failed. Assets in " + LevelsPath);
        }

        [MenuItem("Tools/Color Gate Blast/Configure Android (API 36, AAB)", false, 40)]
        public static void ConfigureAndroid()
        {
            PlayerSettings.productName = "Color Gate Blast";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.colorgateblast.game");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            // compact build: high managed stripping + size-optimised IL2CPP (the game has no imported art, so the .aab stays small)
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.High);
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Android, Il2CppCodeGeneration.OptimizeSize);
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.bundleVersion = "0.1.0";
            EditorUserBuildSettings.buildAppBundle = true;
            AssetDatabase.SaveAssets();
            Debug.Log("[Color Gate Blast] Android configured: target API 36, min API 25, IL2CPP, ARM64, App Bundle (.aab). " +
                      "Before building: switch platform to Android and create a keystore in Project Settings > Player > Publishing Settings.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}


