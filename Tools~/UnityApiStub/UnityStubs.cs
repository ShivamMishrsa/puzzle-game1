// Minimal UnityEngine API surface used by Color Gate Blast runtime scripts - for compile-checking only.
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.All)] public class SerializeField : Attribute { }
    [AttributeUsage(AttributeTargets.All)] public class HeaderAttribute : Attribute { public HeaderAttribute(string s) { } }
    [AttributeUsage(AttributeTargets.All)] public class TooltipAttribute : Attribute { public TooltipAttribute(string s) { } }
    [AttributeUsage(AttributeTargets.All)] public class DefaultExecutionOrder : Attribute { public DefaultExecutionOrder(int o) { } }
    [AttributeUsage(AttributeTargets.All)] public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
    [AttributeUsage(AttributeTargets.All)] public class RequireComponent : Attribute { public RequireComponent(Type t) { } }

    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero { get { return new Vector2(0, 0); } }
        public static Vector2 one { get { return new Vector2(1, 1); } }
        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }
        public static Vector2 operator *(Vector2 a, float f) { return new Vector2(a.x * f, a.y * f); }
        public static implicit operator Vector3(Vector2 v) { return new Vector3(v.x, v.y, 0); }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public Vector3(float x, float y) { this.x = x; this.y = y; this.z = 0; }
        public static Vector3 zero { get { return new Vector3(0, 0, 0); } }
        public static Vector3 one { get { return new Vector3(1, 1, 1); } }
        public static Vector3 up { get { return new Vector3(0, 1, 0); } }
        public static Vector3 forward { get { return new Vector3(0, 0, 1); } }
        public float magnitude { get { return 0; } }
        public float sqrMagnitude { get { return 0; } }
        public Vector3 normalized { get { return this; } }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return a; }
        public static Vector3 operator -(Vector3 a, Vector3 b) { return a; }
        public static Vector3 operator -(Vector3 a) { return a; }
        public static Vector3 operator *(Vector3 a, float f) { return a; }
        public static Vector3 operator *(float f, Vector3 a) { return a; }
        public static Vector3 operator /(Vector3 a, float f) { return a; }
        public static Vector3 Cross(Vector3 a, Vector3 b) { return a; }
        public static float Dot(Vector3 a, Vector3 b) { return 0; }
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) { return a; }
        public static float Distance(Vector3 a, Vector3 b) { return 0; }
        public static Vector3 MoveTowards(Vector3 a, Vector3 b, float d) { return a; }
        public static implicit operator Vector2(Vector3 v) { return new Vector2(v.x, v.y); }
    }

    public struct Quaternion
    {
        public static Quaternion identity { get { return new Quaternion(); } }
        public Vector3 eulerAngles { get { return Vector3.zero; } }
        public static Quaternion Euler(float x, float y, float z) { return new Quaternion(); }
        public static Quaternion LookRotation(Vector3 f, Vector3 u) { return new Quaternion(); }
        public static Vector3 operator *(Quaternion q, Vector3 v) { return v; }
        public static Quaternion operator *(Quaternion a, Quaternion b) { return a; }
    }

    public struct Color32 { public byte r, g, b, a; public Color32(byte r, byte g, byte b, byte a) { this.r = r; this.g = g; this.b = b; this.a = a; } public static implicit operator Color32(Color c) { return new Color32(); } public static implicit operator Color(Color32 c) { return new Color(); } }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; this.a = 1; }
        public static Color white { get { return new Color(1, 1, 1, 1); } }
        public static Color black { get { return new Color(0, 0, 0, 1); } }
        public static Color gray { get { return new Color(.5f, .5f, .5f, 1); } }
        public static Color Lerp(Color a, Color b, float t) { return a; }
        public static Color operator *(Color a, float f) { return a; }
        public static Color operator +(Color a, Color b) { return a; }
    }

    public struct Rect { public Rect(float x, float y, float w, float h) { } }
    public struct Ray { public Vector3 GetPoint(float d) { return Vector3.zero; } }
    public struct Plane { public Plane(Vector3 n, Vector3 p) { } public bool Raycast(Ray r, out float d) { d = 0; return false; } }
    public struct Keyframe { public Keyframe(float t, float v) { } }
    public class AnimationCurve { public AnimationCurve(params Keyframe[] k) { } }
    public struct GradientColorKey { public GradientColorKey(Color c, float t) { } }
    public struct GradientAlphaKey { public GradientAlphaKey(float a, float t) { } }
    public class Gradient { public void SetKeys(GradientColorKey[] c, GradientAlphaKey[] a) { } }
    public struct Bounds { }

    public enum Space { World, Self }
    public enum ForceMode { Force, Impulse, VelocityChange, Acceleration }
    public enum TextAnchor { UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }
    public enum TextAlignment { Left, Center, Right }
    public enum FontStyle { Normal, Bold, Italic, BoldAndItalic }
    public enum CameraClearFlags { Skybox, SolidColor }
    public enum LightType { Spot, Directional, Point }
    public enum LightShadows { None, Hard, Soft }
    public enum TextureFormat { RGBA32 }
    public enum TextureWrapMode { Repeat, Clamp }
    public enum FilterMode { Point, Bilinear }
    public enum RenderMode { ScreenSpaceOverlay }
    public enum ParticleSystemSimulationSpace { Local, World }
    public enum ParticleSystemRenderMode { Billboard, Stretch }
    public enum ParticleSystemStopBehavior { StopEmitting, StopEmittingAndClear }

    public class Object
    {
        public string name;
        public static void Destroy(Object o) { }
        public static void Destroy(Object o, float t) { }
        public static void DestroyImmediate(Object o) { }
        public static T FindFirstObjectByType<T>() where T : Object { return null; }
        public static implicit operator bool(Object o) { return o != null; }
        public static bool operator ==(Object a, Object b) { return ReferenceEquals(a, b); }
        public static bool operator !=(Object a, Object b) { return !ReferenceEquals(a, b); }
        public override bool Equals(object o) { return base.Equals(o); }
        public override int GetHashCode() { return 0; }
    }

    public class Component : Object
    {
        public GameObject gameObject { get { return null; } }
        public Transform transform { get { return null; } }
        public string tag;
        public T GetComponent<T>() { return default(T); }
    }

    public class GameObject : Object
    {
        public GameObject() { }
        public GameObject(string n) { }
        public GameObject(string n, params Type[] t) { }
        public Transform transform { get { return null; } }
        public bool activeSelf { get { return true; } }
        public string tag;
        public T AddComponent<T>() where T : Component { return null; }
        public T GetComponent<T>() { return default(T); }
        public void SetActive(bool b) { }
    }

    public class Transform : Component, IEnumerable
    {
        public Vector3 position, localPosition, localScale, eulerAngles, localEulerAngles;
        public Quaternion rotation, localRotation;
        public Transform parent;
        public int childCount { get { return 0; } }
        public Vector3 forward { get { return Vector3.zero; } }
        public Vector3 up { get { return Vector3.zero; } }
        public void DetachChildren() { }
        public void SetParent(Transform p) { }
        public void SetParent(Transform p, bool worldPositionStays) { }
        public Transform GetChild(int i) { return null; }
        public Transform Find(string n) { return null; }
        public void Rotate(float x, float y, float z, Space s) { }
        public void Rotate(float x, float y, float z) { }
        public void LookAt(Vector3 t, Vector3 up) { }
        public void LookAt(Vector3 t) { }
        public Vector3 TransformPoint(Vector3 p) { return p; }
        public IEnumerator GetEnumerator() { return null; }
    }

    public class RectTransform : Transform
    {
        public Vector2 anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta, offsetMin, offsetMax;
    }

    public class Behaviour : Component { public bool enabled; }
    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator r) { return null; }
        public void StopAllCoroutines() { }
        public void StopCoroutine(Coroutine c) { }
    }
    public class ScriptableObject : Object { public static T CreateInstance<T>() where T : ScriptableObject { return null; } }
    public class YieldInstruction { }
    public class Coroutine : YieldInstruction { }
    public class CustomYieldInstruction : IEnumerator { public object Current { get { return null; } } public bool MoveNext() { return false; } public void Reset() { } }
    public class WaitForSeconds : YieldInstruction { public WaitForSeconds(float s) { } }
    public class WaitForSecondsRealtime : CustomYieldInstruction { public WaitForSecondsRealtime(float s) { } }

    public static class Time
    {
        public static float deltaTime, unscaledDeltaTime, time, unscaledTime, realtimeSinceStartup, timeScale, fixedDeltaTime;
    }

    public static class Mathf
    {
        public const float PI = 3.14159f, Deg2Rad = 0.0174f, Rad2Deg = 57.29f;
        public static float Sin(float f) { return 0; }
        public static float Cos(float f) { return 0; }
        public static float Atan(float f) { return 0; }
        public static float Exp(float f) { return 0; }
        public static float Pow(float a, float b) { return 0; }
        public static float Sqrt(float f) { return 0; }
        public static float Abs(float f) { return 0; }
        public static float Sign(float f) { return 0; }
        public static float Clamp(float v, float a, float b) { return v; }
        public static int Clamp(int v, int a, int b) { return v; }
        public static float Clamp01(float v) { return v; }
        public static float Lerp(float a, float b, float t) { return a; }
        public static float Max(float a, float b) { return a; }
        public static float Max(float a, float b, float c) { return a; }
        public static int Max(int a, int b) { return a; }
        public static float Min(float a, float b) { return a; }
        public static float Min(float a, float b, float c) { return a; }
        public static int Min(int a, int b) { return a; }
        public static int RoundToInt(float f) { return 0; }
        public static int FloorToInt(float f) { return 0; }
        public static int CeilToInt(float f) { return 0; }
        public static float Repeat(float t, float l) { return t; }
        public static float PerlinNoise(float x, float y) { return 0; }
    }

    public static class Random
    {
        public static float value { get { return 0; } }
        public static float Range(float a, float b) { return a; }
        public static int Range(int a, int b) { return a; }
        public static Vector3 onUnitSphere { get { return Vector3.zero; } }
        public static Vector3 insideUnitSphere { get { return Vector3.zero; } }
        public static Quaternion rotation { get { return new Quaternion(); } }
    }

    public static class Debug
    {
        public static bool isDebugBuild;
        public static void Log(object o) { }
        public static void LogWarning(object o) { }
        public static void LogError(object o) { }
    }
    public static class Application { public static bool isEditor, isPlaying; public static int targetFrameRate; }
    public static class Screen { public static int width, height; }
    public static class Resources
    {
        public static T GetBuiltinResource<T>(string n) where T : Object { return null; }
        public static T[] LoadAll<T>(string p) where T : Object { return null; }
    }
    public static class PlayerPrefs
    {
        public static int GetInt(string k, int d) { return d; }
        public static void SetInt(string k, int v) { }
        public static void Save() { }
    }
    public static class Input
    {
        public static bool GetMouseButtonDown(int b) { return false; }
        public static bool GetMouseButton(int b) { return false; }
        public static bool GetMouseButtonUp(int b) { return false; }
        public static Vector3 mousePosition;
    }
    public static class Physics
    {
        public static Vector3 gravity;
        public static Collider[] OverlapSphere(Vector3 p, float r) { return null; }
    }

    public class Mesh : Object
    {
        public Vector2[] uv;
        public void SetVertices(List<Vector3> v) { }
        public void SetTriangles(List<int> t, int sub) { }
        public void RecalculateNormals() { }
        public void RecalculateBounds() { }
    }
    public class MeshFilter : Component { public Mesh sharedMesh; }
    public class Renderer : Component { public Material sharedMaterial; public bool enabled; public void SetPropertyBlock(MaterialPropertyBlock b) { } }
    public class MeshRenderer : Renderer { }
    public class MaterialPropertyBlock { public void SetColor(string n, Color c) { } }
    public class Shader : Object { public static Shader Find(string n) { return null; } }
    public class Texture : Object { public TextureWrapMode wrapMode; public FilterMode filterMode; }
    public class Texture2D : Texture
    {
        public Texture2D(int w, int h, TextureFormat f, bool mip, bool linear) { }
        public static Texture2D whiteTexture;
        public void SetPixels32(Color32[] c) { }
        public void Apply(bool mip, bool noLonger) { }
    }
    public class Material : Object
    {
        public Material(Shader s) { }
        public Material(Material m) { }
        public int renderQueue;
        public void SetColor(string n, Color c) { }
        public void SetFloat(string n, float f) { }
        public void SetInt(string n, int i) { }
        public void SetTexture(string n, Texture t) { }
        public Texture GetTexture(string n) { return null; }
        public void SetTextureScale(string n, Vector2 s) { }
        public void EnableKeyword(string k) { }
        public void DisableKeyword(string k) { }
        public void SetOverrideTag(string a, string b) { }
    }
    public class Font : Object { public Material material; }
    public class TextMesh : Component { public Font font; public int fontSize; public float characterSize; public TextAnchor anchor; public TextAlignment alignment; public FontStyle fontStyle; public Color color; public string text; }

    public class Camera : Behaviour
    {
        public static Camera main;
        public bool orthographic; public float fieldOfView, nearClipPlane, farClipPlane; public CameraClearFlags clearFlags; public Color backgroundColor;
        public Ray ScreenPointToRay(Vector3 p) { return new Ray(); }
    }
    public class Light : Behaviour { public float shadowStrength; public LightType type; public float range, intensity; public Color color; public LightShadows shadows; }
    public class Rigidbody : Component
    {
        public bool isKinematic; public float mass;
        public void AddForce(Vector3 f, ForceMode m) { }
        public void AddTorque(Vector3 f, ForceMode m) { }
        public void AddExplosionForce(float f, Vector3 p, float r, float u, ForceMode m) { }
    }
    public class Collider : Component { public Rigidbody attachedRigidbody { get { return null; } } }
    public class BoxCollider : Collider { public Vector3 size, center; }
    public class SphereCollider : Collider { public float radius; public Vector3 center; }
    public class LineRenderer : Renderer { public int positionCount; public float startWidth, endWidth; public void SetPosition(int i, Vector3 p) { } }

    public class AudioClip : Object
    {
        public static AudioClip Create(string n, int len, int ch, int freq, bool stream) { return null; }
        public void SetData(float[] d, int off) { }
    }
    public class AudioSource : Behaviour { public AudioClip clip; public float pitch, volume, spatialBlend; public bool playOnAwake; public void Play() { } }

    public class ParticleSystem : Component
    {
        public struct MinMaxCurve { public MinMaxCurve(float m, AnimationCurve c) { } public static implicit operator MinMaxCurve(float f) { return new MinMaxCurve(); } }
        public struct MinMaxGradient { public MinMaxGradient(Gradient g) { } }
        public struct EmitParams { public Vector3 position, velocity; public Color32 startColor; public float startSize, startLifetime; }
        public struct MainModule { public bool loop, playOnAwake; public ParticleSystemSimulationSpace simulationSpace; public MinMaxCurve startLifetime, startSpeed, startSize, gravityModifier; public int maxParticles; }
        public struct EmissionModule { public bool enabled; }
        public struct ShapeModule { public bool enabled; }
        public struct SizeOverLifetimeModule { public bool enabled; public MinMaxCurve size; }
        public struct ColorOverLifetimeModule { public bool enabled; public MinMaxGradient color; }
        public MainModule main { get { return new MainModule(); } }
        public EmissionModule emission { get { return new EmissionModule(); } }
        public ShapeModule shape { get { return new ShapeModule(); } }
        public SizeOverLifetimeModule sizeOverLifetime { get { return new SizeOverLifetimeModule(); } }
        public ColorOverLifetimeModule colorOverLifetime { get { return new ColorOverLifetimeModule(); } }
        public void Play() { }
        public void Stop(bool children, ParticleSystemStopBehavior b) { }
        public void Emit(EmitParams p, int n) { }
    }
    public class ParticleSystemRenderer : Renderer { public ParticleSystemRenderMode renderMode; public float velocityScale, lengthScale; }

    public static class GUI { public static Color color; public static GUISkin skin; public static void DrawTexture(Rect r, Texture t) { } public static void Label(Rect r, string s, GUIStyle st) { } }
    public class GUISkin { public GUIStyle label; }
    public class GUIStyleState { public Color textColor; }
    public class GUIStyle { public GUIStyle(GUIStyle o) { } public TextAnchor alignment; public int fontSize; public GUIStyleState normal; }

    public class Canvas : Behaviour { public RenderMode renderMode; public int sortingOrder; }
    public class CanvasScaler : Behaviour
    {
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize }
        public enum ScreenMatchMode { MatchWidthOrHeight }
        public ScaleMode uiScaleMode; public Vector2 referenceResolution; public ScreenMatchMode screenMatchMode; public float matchWidthOrHeight;
    }
    public class Sprite : Object { }

    namespace Events { public delegate void UnityAction(); public class UnityEvent { public void AddListener(UnityAction a) { } public void RemoveAllListeners() { } } }
    namespace Rendering
    {
        public enum BlendMode { SrcAlpha, OneMinusSrcAlpha }
        public enum RenderQueue { Transparent = 3000 }
    }
    namespace EventSystems
    {
        public class EventSystem : Behaviour { public static EventSystem current; public bool IsPointerOverGameObject() { return false; } }
        public class StandaloneInputModule : Behaviour { }
    }
    namespace UI
    {
        public class Graphic : Behaviour { public Color color; public bool raycastTarget; }
        public class Text : Graphic { public Font font; public int fontSize; public FontStyle fontStyle; public TextAnchor alignment; public HorizontalWrapMode horizontalOverflow; public VerticalWrapMode verticalOverflow; public string text; public RectTransform rectTransform; }
        public enum HorizontalWrapMode { Wrap, Overflow }
        public enum VerticalWrapMode { Truncate, Overflow }
        public class Image : Graphic { public enum Type { Simple, Sliced } public Sprite sprite; public Type type; }
        public struct ColorBlock { public Color highlightedColor, pressedColor, disabledColor; }
        public class Selectable : Behaviour { public bool interactable; public Graphic targetGraphic; public ColorBlock colors; }
        public class Button : Selectable { public Events.UnityEvent onClick; }
        public class GraphicRaycaster : Behaviour { }
        public class Outline : Behaviour { public Color effectColor; public Vector2 effectDistance; }
    }
}



