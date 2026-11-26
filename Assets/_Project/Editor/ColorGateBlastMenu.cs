using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ColorGateBlast.EditorTools
{
    /// <summary>Tools &gt; Color Gate Blast menu: one-click scene setup, level generation and Android build settings.</summary>
    public static class ColorGateBlastMenu
    {
        private const string Root = "Assets/_Project";
        private const string ScenePath = Root + "/Scenes/Main.unity";
        private const string LevelsPath = Root + "/Resources/Levels";

        // ====================================================================== Setup Main Scene

        [MenuItem("Tools/Color Gate Blast/Setup Main Scene", false, 1)]
        public static void SetupMainScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureFolder(Root + "/Scenes");
            EnsureFolder(Root + "/Materials");
            EnsureFolder(Root + "/Settings");
            EnsureFolder(LevelsPath);

            EnsureUrp();
            ApplyBasicProjectSettings();

            Material lit = EnsureMaterial("ToyMatte", "Universal Render Pipeline/Lit");
            Material particle = EnsureMaterial("ParticleUnlit", "Universal Render Pipeline/Particles/Unlit");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---- camera (orthographic, looks down +Z; the board lives on the XY plane) ----
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = ColorPalette.Background;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 30f;
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            // ---- lighting: one soft directional light shining into the screen + flat ambient ----
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.97f, 0.92f);
            light.shadows = LightShadows.None;
            lightGo.transform.rotation = Quaternion.Euler(28f, -22f, 0f);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.46f, 0.48f, 0.58f);

            // ---- game objects ----
            GameObject rootGo = new GameObject("GameRoot");
            GridManager grid = AddChild<GridManager>(rootGo, "Board (GridManager)");
            ConveyorTrackManager track = AddChild<ConveyorTrackManager>(rootGo, "Conveyor (ConveyorTrackManager)");
            TargetCoreManager core = AddChild<TargetCoreManager>(rootGo, "TargetCore (TargetCoreManager)");
            JuiceManager juice = AddChild<JuiceManager>(rootGo, "Juice (JuiceManager)");
            MonetizationManager money = AddChild<MonetizationManager>(rootGo, "Monetization (MonetizationManager)");
            UIManager ui = AddChild<UIManager>(rootGo, "UI (UIManager)");
            GameManager gm = AddChild<GameManager>(rootGo, "GameManager");

            gm.grid = grid;
            gm.track = track;
            gm.core = core;
            gm.juice = juice;
            gm.ui = ui;
            gm.monetization = money;
            gm.cam = cam;
            gm.litTemplate = lit;
            gm.particleTemplate = particle;

            // ---- Canvas UI ----
            Sprite rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            ui.BuildUI(rounded);
            EditorUtility.SetDirty(ui);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Color Gate Blast] Main scene created at " + ScenePath + ".\n" +
                      "Next: Tools > Color Gate Blast > Generate Starter Levels 1-10, then set the Game view to 1080x1920 (portrait) and press Play.");
        }

        private static T AddChild<T>(GameObject parent, string name) where T : Component
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go.AddComponent<T>();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static Material EnsureMaterial(string name, string shaderName)
        {
            string path = Root + "/Materials/" + name + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            Shader s = Shader.Find(shaderName);
            if (s == null)
            {
                Debug.LogError("[Color Gate Blast] Shader not found: " + shaderName + " (is the Universal RP package installed?)");
                return null;
            }
            m = new Material(s);
            m.name = name;
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        /// <summary>Makes sure the project renders with URP (a fresh Unity 6 project without a template has no pipeline asset).</summary>
        private static void EnsureUrp()
        {
            if (GraphicsSettings.defaultRenderPipeline != null) return;

            UniversalRendererData rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, Root + "/Settings/ColorGateBlast_Renderer.asset");
            UniversalRenderPipelineAsset urp = UniversalRenderPipelineAsset.Create(rendererData);
            AssetDatabase.CreateAsset(urp, Root + "/Settings/ColorGateBlast_URP.asset");

            GraphicsSettings.defaultRenderPipeline = urp;
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = urp;
            }
            QualitySettings.SetQualityLevel(current, false);
            AssetDatabase.SaveAssets();
            Debug.Log("[Color Gate Blast] Created and assigned a URP pipeline asset.");
        }

        private static void ApplyBasicProjectSettings()
        {
            PlayerSettings.productName = "Color Gate Blast";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
        }

        // ====================================================================== Levels

        [MenuItem("Tools/Color Gate Blast/Generate Starter Levels 1-10", false, 2)]
        public static void GenerateStarterLevels() { GenerateRange(1, 10); }

        public static void GenerateRange(int from, int to)
        {
            EnsureFolder(LevelsPath);
            int ok = 0, failed = 0;
            try
            {
                for (int lv = from; lv <= to; lv++)
                {
                    EditorUtility.DisplayProgressBar("Generating levels", "Level " + lv + " (generate + solve)", (lv - from) / (float)(to - from + 1));
                    GenResult r = LevelGenerator.Generate(lv);
                    if (r.def == null)
                    {
                        failed++;
                        Debug.LogError("[Color Gate Blast] Could not generate a solvable level " + lv);
                        continue;
                    }

                    string path = LevelsPath + "/Level_" + lv.ToString("000") + ".asset";
                    LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                    if (data == null)
                    {
                        data = ScriptableObject.CreateInstance<LevelData>();
                        AssetDatabase.CreateAsset(data, path);
                    }
                    data.definition = r.def;
                    EditorUtility.SetDirty(data);
                    ok++;

                    LevelDefinition d = r.def;
                    Debug.Log("[Color Gate Blast] Level " + lv + " [" + d.tier + "] " + d.gridWidth + "x" + d.gridHeight +
                              ", " + d.colorCount + " colours, " + d.blocks.Count + " blocks, core " + d.coreWidth + "x" + d.coreHeight +
                              ", par " + d.parMoves + (r.solve.exact ? " (optimal)" : " (solver upper bound)") +
                              ", limit " + d.moveLimit);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Color Gate Blast] Generated " + ok + " level(s), " + failed + " failed. Assets in " + LevelsPath);
        }

        [MenuItem("Tools/Color Gate Blast/Verify All Levels (re-solve)", false, 3)]
        public static void VerifyAllLevels()
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { LevelsPath });
            int bad = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("Verifying levels", path, i / (float)guids.Length);
                LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                string err = data.definition.Validate();
                LevelSolver.SolveResult r = err == null ? LevelSolver.Solve(data.definition, 20000, 80000) : null;
                if (err != null || !r.solved)
                {
                    bad++;
                    Debug.LogError("[Color Gate Blast] " + path + " is NOT solvable" + (err != null ? " (" + err + ")" : ""));
                }
            }
            EditorUtility.ClearProgressBar();
            Debug.Log("[Color Gate Blast] Verified " + guids.Length + " level(s): " + (guids.Length - bad) + " solvable, " + bad + " broken.");
        }

        // ====================================================================== Android

        [MenuItem("Tools/Color Gate Blast/Configure Android (API 36, AAB)", false, 20)]
        public static void ConfigureAndroid()
        {
            ApplyBasicProjectSettings();
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.colorgateblast.game");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.bundleVersion = "0.1.0";
            EditorUserBuildSettings.buildAppBundle = true;
            AssetDatabase.SaveAssets();
            Debug.Log("[Color Gate Blast] Android configured: target API 36, min API 25, IL2CPP, ARM64, App Bundle (.aab).\n" +
                      "Before building: File > Build Profiles > Android > Switch Platform, then create a keystore in Project Settings > Player > Publishing Settings.");
        }
    }
}
