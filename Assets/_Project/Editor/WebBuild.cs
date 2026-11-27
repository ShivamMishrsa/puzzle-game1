using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ColorGateBlast.EditorTools
{
    /// <summary>
    /// WebGL build for GitHub Pages.
    ///  * Tools > Color Gate Blast > Build WebGL (docs/play)  - local one-click build into docs/play (commit it and serve /docs with Pages).
    ///  * WebBuild.BuildForCI - used by .github/workflows/webgl.yml (game-ci) in batch mode.
    /// Both run the Setup Phase 1 logic first, so the scene and all level assets are (re)generated and solver-verified.
    /// </summary>
    public static class WebBuild
    {
        [MenuItem("Tools/Color Gate Blast/Build WebGL (docs/play)", false, 60)]
        public static void BuildLocal()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../docs/play"));
            Build(folder);
        }

        /// <summary>Entry point for CI: -executeMethod ColorGateBlast.EditorTools.WebBuild.BuildForCI</summary>
        public static void BuildForCI()
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../build/WebGL"));
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-customBuildPath") folder = args[i + 1];
            Build(folder);
        }

        private static void Build(string folder)
        {
            EditorSetupColorGateBlast.SetupPhase1Silent();

            // Compact build: Brotli + JS decompression fallback (GitHub Pages cannot send Content-Encoding headers, so the
            // loader decompresses in the page), high managed stripping and IL2CPP size optimisation.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.High);
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, Il2CppCodeGeneration.OptimizeSize);
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.template = "PROJECT:ColorGateBlast";
            PlayerSettings.defaultWebScreenWidth = 540;
            PlayerSettings.defaultWebScreenHeight = 960;
            PlayerSettings.productName = "Color Gate Blast";

            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);

            BuildPlayerOptions opt = new BuildPlayerOptions();
            opt.scenes = new[] { EditorSetupColorGateBlast.ScenePath };
            opt.locationPathName = folder;
            opt.target = BuildTarget.WebGL;
            opt.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(opt);
            BuildSummary sum = report.summary;
            if (sum.result != BuildResult.Succeeded)
            {
                Debug.LogError("[Color Gate Blast] WebGL build FAILED: " + sum.result + " (" + sum.totalErrors + " errors)");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }
            Debug.Log("[Color Gate Blast] WebGL build OK -> " + folder + " (" + (sum.totalSize / (1024 * 1024)) + " MB). " +
                      "Commit docs/play and set GitHub Pages to deploy from the main branch /docs folder.");
        }
    }
}
