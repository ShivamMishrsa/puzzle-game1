using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ColorGateBlast.EditorTools
{
    /// <summary>
    /// Tools &gt; Color Gate Blast &gt; Setup Phase 1: builds the whole playable scene in one click
    /// (URP, camera, soft-shadow light, post-processing volume, managers, HUD, level assets) and saves it.
    /// </summary>
    public static class EditorSetupColorGateBlast
    {
        private const string Root = "Assets/_Project";
        private const string ScenePath = Root + "/Scenes/Main.unity";
        private const string AdMobSymbol = "GOOGLE_MOBILE_ADS";

        [MenuItem("Tools/Color Gate Blast/Setup Phase 1", false, 1)]
        public static void SetupPhase1()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureFolder(Root + "/Scenes");
            EnsureFolder(Root + "/Settings");
            EnsureFolder(Root + "/Resources/Levels");

            EnsureUrp();
            IncludeShaders();
            ApplyProjectSettings();

            // ---- levels (solver-verified before they are saved) ----
            int last = LevelLibrary.Range(1, 99).Count;
            if (last > 0) LevelAssetTools.GenerateRange(1, last);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---- camera (perspective, post-processing on; framing is set at runtime by LevelProgressor) ----
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.06f, 0.06f);
            cam.fieldOfView = 45f;
            camGo.transform.position = new Vector3(0f, 22f, -12f);
            camGo.transform.LookAt(new Vector3(0f, 0.8f, 0.9f));
            UniversalAdditionalCameraData camData = camGo.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;

            // ---- soft directional light + warm ambient ----
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.35f;
            light.color = new Color(1f, 0.95f, 0.86f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.8f;
            lightGo.transform.rotation = Quaternion.Euler(58f, -28f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.36f, 0.35f, 0.4f);
            RenderSettings.skybox = null;

            BuildVolume();

            // ---- managers ----
            GameObject rootGo = new GameObject("GameRoot");
            GridManager grid = Child<GridManager>(rootGo, "Tray (GridManager)");
            ConveyorTrackManager track = Child<ConveyorTrackManager>(rootGo, "Conveyor (ConveyorTrackManager)");
            MonumentManager monument = Child<MonumentManager>(rootGo, "Monument (MonumentManager)");
            FXManager fx = Child<FXManager>(rootGo, "FX (FXManager)");
            SoundSynth sound = Child<SoundSynth>(rootGo, "Sound (SoundSynth)");
            AdManager ads = Child<AdManager>(rootGo, "Ads (AdManager)");
            HintSystem hint = Child<HintSystem>(rootGo, "Hint (HintSystem)");
            UIManager ui = Child<UIManager>(rootGo, "HUD (UIManager)");
            LevelProgressor progressor = Child<LevelProgressor>(rootGo, "LevelProgressor");

            progressor.grid = grid;
            progressor.track = track;
            progressor.monument = monument;
            progressor.ui = ui;
            progressor.hint = hint;
            progressor.fx = fx;
            progressor.sound = sound;
            progressor.ads = ads;
            progressor.cam = cam;

            // bind every generated LevelData asset
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { Root + "/Resources/Levels" });
            List<LevelData> found = new List<LevelData>();
            for (int i = 0; i < guids.Length; i++) found.Add(AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(guids[i])));
            found.Sort(delegate (LevelData a, LevelData b) { return a.LevelNumber.CompareTo(b.LevelNumber); });
            progressor.levels = found.ToArray();

            // ---- HUD ----
            Sprite rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            ui.BuildUI(rounded);
            EditorUtility.SetDirty(ui);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            PlayerPrefs.SetInt("cgb_level", 1);

            Debug.Log("[Color Gate Blast] Phase 1 ready: " + found.Count + " level(s) bound, scene saved at " + ScenePath + ".\n" +
                      "Set the Game view to 1080x1920 (Portrait) and press Play. Use Tools > Color Gate Blast > Play From Level to jump to 11, 21, 31...");
        }

        // ---------------------------------------------------------------- helpers

        private static T Child<T>(GameObject parent, string name) where T : Component
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

        /// <summary>A fresh Unity 6 project without a template has no render pipeline asset: create one with soft shadows.</summary>
        private static void EnsureUrp()
        {
            UniversalRenderPipelineAsset urp = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null)
            {
                UniversalRendererData rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, Root + "/Settings/ColorGateBlast_Renderer.asset");
                urp = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(urp, Root + "/Settings/ColorGateBlast_URP.asset");

                GraphicsSettings.defaultRenderPipeline = urp;
                int current = QualitySettings.GetQualityLevel();
                for (int i = 0; i < QualitySettings.names.Length; i++)
                {
                    QualitySettings.SetQualityLevel(i, false);
                    QualitySettings.renderPipeline = urp;
                }
                QualitySettings.SetQualityLevel(current, false);
                Debug.Log("[Color Gate Blast] Created and assigned a URP pipeline asset.");
            }

            // soft contact shadows (property names differ between URP versions, so each is optional)
            SerializedObject so = new SerializedObject(urp);
            SetIfPresent(so, "m_MainLightShadowsSupported", true);
            SetIfPresent(so, "m_SoftShadowsSupported", true);
            SetIfPresent(so, "m_ShadowDistance", 40f);
            SetIfPresent(so, "m_MainLightShadowmapResolution", 2048);
            SetIfPresent(so, "m_SupportsHDR", true);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(urp);
            AssetDatabase.SaveAssets();
        }

        private static void SetIfPresent(SerializedObject so, string name, object value)
        {
            SerializedProperty p = so.FindProperty(name);
            if (p == null) return;
            if (value is bool) p.boolValue = (bool)value;
            else if (value is int) p.intValue = (int)value;
            else if (value is float) p.floatValue = (float)value;
        }

        /// <summary>
        /// All materials are created from code with Shader.Find, so the URP shaders must survive build stripping:
        /// add them to Graphics Settings > Always Included Shaders.
        /// </summary>
        private static void IncludeShaders()
        {
            string[] names =
            {
                "Universal Render Pipeline/Lit", "Universal Render Pipeline/Unlit",
                "Universal Render Pipeline/Particles/Lit", "Universal Render Pipeline/Particles/Unlit"
            };
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (assets == null || assets.Length == 0) return;
            SerializedObject so = new SerializedObject(assets[0]);
            SerializedProperty list = so.FindProperty("m_AlwaysIncludedShaders");
            if (list == null) return;
            for (int n = 0; n < names.Length; n++)
            {
                Shader s = Shader.Find(names[n]);
                if (s == null) continue;
                bool present = false;
                for (int i = 0; i < list.arraySize; i++)
                    if (list.GetArrayElementAtIndex(i).objectReferenceValue == s) { present = true; break; }
                if (present) continue;
                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = s;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ApplyProjectSettings()
        {
            PlayerSettings.productName = "Color Gate Blast";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
        }

        /// <summary>URP Volume: soft colour grading, bloom (for laser glows), vignette and light motion blur.</summary>
        private static void BuildVolume()
        {
            string path = Root + "/Settings/ColorGateBlast_Volume.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);

                Bloom bloom = profile.Add<Bloom>(true);
                bloom.intensity.Override(0.8f);
                bloom.threshold.Override(1.0f);
                bloom.scatter.Override(0.65f);
                AssetDatabase.AddObjectToAsset(bloom, profile);

                Vignette vig = profile.Add<Vignette>(true);
                vig.intensity.Override(0.28f);
                vig.smoothness.Override(0.5f);
                AssetDatabase.AddObjectToAsset(vig, profile);

                ColorAdjustments ca = profile.Add<ColorAdjustments>(true);
                ca.postExposure.Override(0.12f);
                ca.contrast.Override(10f);
                ca.saturation.Override(12f);
                AssetDatabase.AddObjectToAsset(ca, profile);

                Tonemapping tm = profile.Add<Tonemapping>(true);
                tm.mode.Override(TonemappingMode.ACES);
                AssetDatabase.AddObjectToAsset(tm, profile);

                MotionBlur mb = profile.Add<MotionBlur>(true);
                mb.intensity.Override(0.25f);
                AssetDatabase.AddObjectToAsset(mb, profile);

                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
            }

            GameObject volGo = new GameObject("Post-Processing Volume");
            Volume vol = volGo.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = profile;
        }

        // ---------------------------------------------------------------- start-level shortcuts

        [MenuItem("Tools/Color Gate Blast/Play From Level/Level 1", false, 20)]
        public static void From1() { SetStart(1); }
        [MenuItem("Tools/Color Gate Blast/Play From Level/Level 11", false, 21)]
        public static void From11() { SetStart(11); }
        [MenuItem("Tools/Color Gate Blast/Play From Level/Level 15 (Mini-Boss)", false, 22)]
        public static void From15() { SetStart(15); }
        [MenuItem("Tools/Color Gate Blast/Play From Level/Level 20 (Chapter 2 Boss)", false, 23)]
        public static void From20() { SetStart(20); }
        [MenuItem("Tools/Color Gate Blast/Play From Level/Level 21", false, 24)]
        public static void From21() { SetStart(21); }
        [MenuItem("Tools/Color Gate Blast/Play From Level/Level 31", false, 25)]
        public static void From31() { SetStart(31); }

        private static void SetStart(int level)
        {
            PlayerPrefs.SetInt("cgb_level", level);
            PlayerPrefs.Save();
            Debug.Log("[Color Gate Blast] The next Play session starts at level " + level + ".");
        }

        // ---------------------------------------------------------------- AdMob

        [MenuItem("Tools/Color Gate Blast/AdMob/Enable Google Mobile Ads (after importing the plugin)", false, 50)]
        public static void EnableAdMob()
        {
            bool sdk = false;
            foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                if (a.GetName().Name.Contains("GoogleMobileAds")) { sdk = true; break; }
            if (!sdk)
            {
                Debug.LogWarning("[Color Gate Blast] The Google Mobile Ads Unity plugin is not imported yet. Import it from " +
                                 "https://github.com/googleads/googleads-mobile-unity/releases, then run this menu again. " +
                                 "(Until then the built-in ad simulator is used.)");
                return;
            }
            SetDefine(true);
            Debug.Log("[Color Gate Blast] GOOGLE_MOBILE_ADS defined. In Assets > Google Mobile Ads > Settings set the Android app id to the TEST id " +
                      AdManager.TestAppId + " until your final release. Test ad unit ids are used automatically.");
        }

        [MenuItem("Tools/Color Gate Blast/AdMob/Disable (use ad simulator)", false, 51)]
        public static void DisableAdMob()
        {
            SetDefine(false);
            Debug.Log("[Color Gate Blast] GOOGLE_MOBILE_ADS removed: the ad simulator is active again.");
        }

        private static void SetDefine(bool on)
        {
            NamedBuildTarget[] targets = { NamedBuildTarget.Android, NamedBuildTarget.Standalone };
            for (int t = 0; t < targets.Length; t++)
            {
                string defs = PlayerSettings.GetScriptingDefineSymbols(targets[t]);
                List<string> list = new List<string>(defs.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                bool has = list.Contains(AdMobSymbol);
                if (on && !has) list.Add(AdMobSymbol);
                if (!on && has) list.Remove(AdMobSymbol);
                PlayerSettings.SetScriptingDefineSymbols(targets[t], string.Join(";", list.ToArray()));
            }
        }
    }
}
