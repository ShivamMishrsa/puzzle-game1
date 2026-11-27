// Minimal UnityEditor / URP surface for compile-checking the Editor scripts (signatures only).
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine
{
    public class AudioListener : Behaviour { }
    public static class RenderSettings { public static Rendering.AmbientMode ambientMode; public static Color ambientLight; public static Material skybox; }
    public static class QualitySettings { public static string[] names; public static int GetQualityLevel() { return 0; } public static void SetQualityLevel(int i, bool b) { } public static Rendering.RenderPipelineAsset renderPipeline; }
    public enum UIOrientation { Portrait }
    namespace Rendering
    {
        public enum AmbientMode { Flat }
        public class RenderPipelineAsset : ScriptableObject { }
        public static class GraphicsSettings { public static RenderPipelineAsset defaultRenderPipeline; }
        public class VolumeComponent : ScriptableObject { }
        public class VolumeParameter<T> { public void Override(T v) { } }
        public class ClampedFloatParameter : VolumeParameter<float> { }
        public class FloatParameter : VolumeParameter<float> { }
        public class VolumeProfile : ScriptableObject { public T Add<T>(bool overrides) where T : VolumeComponent { return null; } }
        public class Volume : Behaviour { public bool isGlobal; public VolumeProfile sharedProfile; }
        namespace Universal
        {
            public class UniversalRenderPipelineAsset : RenderPipelineAsset { public static UniversalRenderPipelineAsset Create(UniversalRendererData d) { return null; } }
            public class UniversalRendererData : ScriptableObject { }
            public enum AntialiasingMode { SubpixelMorphologicalAntiAliasing }
            public class UniversalAdditionalCameraData : Behaviour { public bool renderPostProcessing; public AntialiasingMode antialiasing; }
            public class Bloom : VolumeComponent { public ClampedFloatParameter intensity, scatter; public FloatParameter threshold; }
            public class Vignette : VolumeComponent { public ClampedFloatParameter intensity, smoothness; }
            public class ColorAdjustments : VolumeComponent { public FloatParameter postExposure, contrast, saturation; }
            public enum TonemappingMode { ACES }
            public class Tonemapping : VolumeComponent { public VolumeParameter<TonemappingMode> mode; }
            public class MotionBlur : VolumeComponent { public ClampedFloatParameter intensity; }
        }
    }
}

namespace UnityEditor
{
    namespace Build { public struct NamedBuildTarget { public static NamedBuildTarget Android, Standalone; } }
    namespace SceneManagement
    {
        public enum NewSceneSetup { EmptyScene }
        public enum NewSceneMode { Single }
        public struct Scene { }
        public static class EditorSceneManager
        {
            public static bool SaveCurrentModifiedScenesIfUserWantsTo() { return true; }
            public static Scene NewScene(NewSceneSetup s, NewSceneMode m) { return new Scene(); }
            public static bool MarkSceneDirty(Scene s) { return true; }
            public static bool SaveScene(Scene s, string p) { return true; }
        }
    }
    [AttributeUsage(AttributeTargets.Method)] public class MenuItem : Attribute { public MenuItem(string p, bool v, int pr) { } public MenuItem(string p) { } }
    public static class EditorUtility
    {
        public static void SetDirty(UnityEngine.Object o) { }
        public static void DisplayProgressBar(string a, string b, float p) { }
        public static void ClearProgressBar() { }
    }
    public static class AssetDatabase
    {
        public static bool IsValidFolder(string p) { return true; }
        public static string CreateFolder(string parent, string name) { return ""; }
        public static void CreateAsset(UnityEngine.Object o, string p) { }
        public static void AddObjectToAsset(UnityEngine.Object o, UnityEngine.Object parent) { }
        public static T LoadAssetAtPath<T>(string p) where T : UnityEngine.Object { return null; }
        public static UnityEngine.Object[] LoadAllAssetsAtPath(string p) { return null; }
        public static string[] FindAssets(string f, string[] folders) { return null; }
        public static string GUIDToAssetPath(string g) { return ""; }
        public static T GetBuiltinExtraResource<T>(string p) where T : UnityEngine.Object { return null; }
        public static void SaveAssets() { }
        public static void Refresh() { }
    }
    public class SerializedProperty
    {
        public bool boolValue; public int intValue; public float floatValue; public int arraySize; public UnityEngine.Object objectReferenceValue;
        public SerializedProperty GetArrayElementAtIndex(int i) { return null; }
    }
    public class SerializedObject
    {
        public SerializedObject(UnityEngine.Object o) { }
        public SerializedProperty FindProperty(string n) { return null; }
        public bool ApplyModifiedPropertiesWithoutUndo() { return true; }
    }
    public static class PlayerSettings
    {
        public static string productName, bundleVersion;
        public static UIOrientation defaultInterfaceOrientation;
        public static bool allowedAutorotateToPortrait, allowedAutorotateToPortraitUpsideDown, allowedAutorotateToLandscapeLeft, allowedAutorotateToLandscapeRight;
        public static void SetApplicationIdentifier(Build.NamedBuildTarget t, string id) { }
        public static void SetScriptingBackend(Build.NamedBuildTarget t, ScriptingImplementation i) { }
        public static string GetScriptingDefineSymbols(Build.NamedBuildTarget t) { return ""; }
        public static void SetScriptingDefineSymbols(Build.NamedBuildTarget t, string s) { }
        public static class Android
        {
            public static AndroidSdkVersions minSdkVersion, targetSdkVersion;
            public static AndroidArchitecture targetArchitectures;
            public static int bundleVersionCode;
        }
    }
    public enum AndroidSdkVersions { AndroidApiLevel25 = 25 }
    public enum AndroidArchitecture { ARM64 }
    public enum ScriptingImplementation { IL2CPP }
    public static class EditorUserBuildSettings { public static bool buildAppBundle; }
    public class EditorBuildSettingsScene { public EditorBuildSettingsScene(string p, bool e) { } }
    public static class EditorBuildSettings { public static EditorBuildSettingsScene[] scenes; }
}
