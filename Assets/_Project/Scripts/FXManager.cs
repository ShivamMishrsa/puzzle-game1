using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorGateBlast
{
    public enum FxKind { Spark, Splinter, Smoke, Glass, IceShard, GoldSpark, Fire, Dust }

    /// <summary>
    /// Screen-space camera shake, hit-stop / slow-motion for explosions, procedural particle systems that react to
    /// URP lighting (lit particle material for solid fragments, unlit-additive look for sparks) and light Android haptics.
    /// </summary>
    public sealed class FXManager : MonoBehaviour
    {
        public static FXManager Instance { get; private set; }

        public bool HapticsEnabled = true;

        private sealed class Emitter
        {
            public ParticleSystem ps;
            public float baseSize;
            public float baseLife;
            public float gravity;
        }

        private readonly Dictionary<FxKind, Emitter> emitters = new Dictionary<FxKind, Emitter>();
        private Camera cam;
        private Vector3 camBase;
        private bool shaking;
        private float shakeTime, shakeDuration, shakeMag;
        private bool timeBusy;
        private Light flash;
        private float flashLeft;
        private float lastHaptic = -1f;

        private void Awake()
        {
            Instance = this;
            BuildEmitters();
            GameObject l = new GameObject("ExplosionFlash");
            l.transform.SetParent(transform, false);
            flash = l.AddComponent<Light>();
            flash.type = LightType.Point;
            flash.range = 9f;
            flash.color = new Color(1f, 0.65f, 0.25f);
            flash.intensity = 0f;
            flash.shadows = LightShadows.None;
        }

        public void SetCamera(Camera c)
        {
            cam = c;
            camBase = c.transform.position;
        }

        // ------------------------------------------------------------------ emitters

        private void Make(FxKind kind, Material mat, float life, float gravity, float size, bool fade, Color tint, bool stretch)
        {
            GameObject go = new GameObject("FX_" + kind);
            go.transform.SetParent(transform, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = life;
            main.startSpeed = 0f;
            main.startSize = size;
            main.maxParticles = 800;
            main.gravityModifier = gravity;

            ParticleSystem.EmissionModule em = ps.emission;
            em.enabled = false;
            ParticleSystem.ShapeModule sh = ps.shape;
            sh.enabled = false;

            ParticleSystem.SizeOverLifetimeModule sol = ps.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve curve = kind == FxKind.Smoke
                ? new AnimationCurve(new Keyframe(0f, 0.4f), new Keyframe(1f, 1.6f))
                : new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.7f, 0.8f), new Keyframe(1f, 0f));
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

            if (fade)
            {
                ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
                col.enabled = true;
                Gradient g = new Gradient();
                g.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(tint, 1f) },
                          new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
                col.color = new ParticleSystem.MinMaxGradient(g);
            }

            ParticleSystemRenderer pr = go.GetComponent<ParticleSystemRenderer>();
            pr.renderMode = stretch ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard;
            if (stretch) { pr.velocityScale = 0.08f; pr.lengthScale = 1.8f; }
            pr.sharedMaterial = mat;
            ps.Play();

            Emitter e = new Emitter();
            e.ps = ps;
            e.baseSize = size;
            e.baseLife = life;
            e.gravity = gravity;
            emitters[kind] = e;
        }

        private void BuildEmitters()
        {
            Material solid = GameMaterials.ParticleLit();
            Material add = GameMaterials.ParticleUnlit(true);
            Make(FxKind.Spark, add, 0.45f, 1.2f, 0.07f, true, new Color(1f, 0.4f, 0.05f), true);
            Make(FxKind.GoldSpark, add, 0.7f, 1.0f, 0.09f, true, new Color(1f, 0.7f, 0.1f), true);
            Make(FxKind.Splinter, solid, 1.1f, 2.2f, 0.09f, false, Color.gray, false);
            Make(FxKind.Glass, solid, 1.0f, 2.0f, 0.08f, false, Color.white, false);
            Make(FxKind.IceShard, solid, 0.9f, 2.0f, 0.1f, false, Color.white, false);
            Make(FxKind.Smoke, add, 1.6f, -0.05f, 0.55f, true, new Color(0.15f, 0.15f, 0.15f), false);
            Make(FxKind.Fire, add, 0.55f, -0.1f, 0.9f, true, new Color(0.8f, 0.1f, 0.02f), false);
            Make(FxKind.Dust, add, 1.0f, 0.1f, 0.4f, true, new Color(0.5f, 0.4f, 0.3f), false);
        }

        public void Burst(FxKind kind, Vector3 pos, int count, Color color, float speed, float sizeScale)
        {
            Emitter e;
            if (!emitters.TryGetValue(kind, out e)) return;
            for (int i = 0; i < count; i++)
            {
                ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                Vector3 dir = Random.onUnitSphere;
                if (kind == FxKind.Smoke || kind == FxKind.Fire || kind == FxKind.Dust) dir.y = Mathf.Abs(dir.y) * 0.8f + 0.2f;
                float s = speed * (0.35f + Random.value * 0.9f);
                p.position = pos + Random.insideUnitSphere * 0.08f;
                p.velocity = dir * s;
                p.startColor = Color.Lerp(color, Color.white, Random.value * 0.3f);
                p.startSize = e.baseSize * sizeScale * (0.6f + Random.value * 0.8f);
                p.startLifetime = e.baseLife * (0.6f + Random.value * 0.7f);
                e.ps.Emit(p, 1);
            }
        }

        // ------------------------------------------------------------------ composite effects

        public void Sparks(Vector3 pos, int count) { Burst(FxKind.Spark, pos, count, new Color(1f, 0.8f, 0.3f), 5f, 1f); }

        public void GoldSparks(Vector3 pos, int count) { Burst(FxKind.GoldSpark, pos, count, new Color(1f, 0.82f, 0.25f), 4.5f, 1f); }

        public void MetalBreak(Vector3 pos, Color tint)
        {
            Burst(FxKind.Spark, pos, 16, new Color(1f, 0.75f, 0.3f), 6f, 1f);
            Burst(FxKind.Splinter, pos, 10, tint, 4.5f, 1f);
            Burst(FxKind.Smoke, pos, 4, new Color(0.3f, 0.3f, 0.3f), 1.2f, 1f);
        }

        public void GlassBreak(Vector3 pos)
        {
            Burst(FxKind.Glass, pos, 22, new Color(0.8f, 0.95f, 1f), 4.2f, 1f);
            Burst(FxKind.Spark, pos, 6, new Color(0.9f, 1f, 1f), 3f, 0.8f);
        }

        public void IceBreak(Vector3 pos, int count)
        {
            Burst(FxKind.IceShard, pos, count, new Color(0.72f, 0.93f, 1f), 3.4f, 1f);
        }

        public void Explosion(Vector3 pos)
        {
            Burst(FxKind.Fire, pos, 18, new Color(1f, 0.55f, 0.1f), 3.5f, 1.6f);
            Burst(FxKind.Spark, pos, 40, new Color(1f, 0.8f, 0.3f), 9f, 1.2f);
            Burst(FxKind.Smoke, pos, 14, new Color(0.2f, 0.2f, 0.2f), 2.5f, 1.8f);
            Burst(FxKind.Splinter, pos, 18, new Color(0.5f, 0.5f, 0.55f), 7f, 1f);
            flash.transform.position = pos + new Vector3(0f, 1.5f, -1f);
            flash.intensity = 14f;
            flashLeft = 0.35f;
        }

        // ------------------------------------------------------------------ camera shake, hit-stop

        public void Shake(float magnitude, float duration)
        {
            if (cam == null) return;
            if (!shaking) camBase = cam.transform.position;
            shaking = true;
            shakeMag = Mathf.Max(shakeMag * Mathf.Clamp01(1f - shakeTime / Mathf.Max(0.01f, shakeDuration)), magnitude);
            shakeDuration = duration;
            shakeTime = 0f;
        }

        /// <summary>Freeze-frame, then a brief slow-motion tail (used by TNT).</summary>
        public void HitStop(float freezeSeconds, float slowScale, float slowSeconds)
        {
            if (timeBusy) return;
            StartCoroutine(HitStopRoutine(freezeSeconds, slowScale, slowSeconds));
        }

        private IEnumerator HitStopRoutine(float freeze, float slowScale, float slowSeconds)
        {
            timeBusy = true;
            float fixedBase = 0.02f;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(freeze);
            Time.timeScale = slowScale;
            Time.fixedDeltaTime = fixedBase * Mathf.Max(0.05f, slowScale);
            yield return new WaitForSecondsRealtime(slowSeconds);
            Time.timeScale = 1f;
            Time.fixedDeltaTime = fixedBase;
            timeBusy = false;
        }

        // ------------------------------------------------------------------ haptics

        /// <summary>Short Android vibration tick (block snap, ice thaw, turret merge). No-op elsewhere.</summary>
        public void Haptic()
        {
            if (!HapticsEnabled) return;
            if (Time.unscaledTime - lastHaptic < 0.06f) return;
            lastHaptic = Time.unscaledTime;
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        private void LateUpdate()
        {
            if (shaking && cam != null)
            {
                shakeTime += Time.unscaledDeltaTime;
                float k = 1f - shakeTime / Mathf.Max(0.01f, shakeDuration);
                if (k <= 0f)
                {
                    shaking = false;
                    shakeMag = 0f;
                    cam.transform.position = camBase;
                }
                else
                {
                    Vector3 o = Random.insideUnitSphere * shakeMag * k;
                    cam.transform.position = camBase + o;
                }
            }
            if (flashLeft > 0f)
            {
                flashLeft -= Time.unscaledDeltaTime;
                flash.intensity = Mathf.Max(0f, flash.intensity - 40f * Time.unscaledDeltaTime);
            }
        }
    }
}
