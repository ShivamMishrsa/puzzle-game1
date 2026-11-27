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
                        ? LevelSolver.SolveWithin(data.definition, 30000, 300000, data.definition.moveLimit)
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

                    LevelSolver.SolveResult within = LevelSolver.SolveWithin(def, 30000, 300000, def.moveLimit);
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

