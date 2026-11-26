using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// Game feel: procedural audio (marimba / pop tones synthesised in code, +1 semitone per consecutive hit),
    /// particle bursts, camera shake and squash-and-pop animations. No audio or particle assets are used.
    /// </summary>
    public sealed class JuiceManager : MonoBehaviour
    {
        public static JuiceManager Instance { get; private set; }

        private const int SampleRate = 44100;
        private const float BaseFreq = 523.25f; // C5

        private AudioSource[] sources;
        private int nextSource;
        private AudioClip pop, tick, whoosh, error, unlock, win, fail;
        private ParticleSystem particles;
        private Camera cam;
        private Vector3 camBasePos;
        private Coroutine shakeRoutine;

        private int combo;
        private float lastHitTime = -10f;
        public float ComboWindow = 0.75f;
        public int MaxComboSemitones = 14;
        public float Volume = 0.8f;

        private void Awake()
        {
            Instance = this;
            sources = new AudioSource[8];
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource s = gameObject.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 0f;
                sources[i] = s;
            }
            BuildClips();
            BuildParticles();
        }

        public void SetCamera(Camera c)
        {
            cam = c;
            camBasePos = c.transform.position;
        }

        // ------------------------------------------------------------------ audio

        private static float[] Note(float freq, float dur, float amp)
        {
            int n = (int)(dur * SampleRate);
            float[] d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SampleRate;
                // marimba: fundamental + a fast-decaying 4th partial, soft attack, exponential decay
                float env = Mathf.Exp(-t * 7f) * Mathf.Min(1f, t * 600f);
                float s = Mathf.Sin(2f * Mathf.PI * freq * t)
                          + 0.38f * Mathf.Sin(2f * Mathf.PI * freq * 4f * t) * Mathf.Exp(-t * 22f)
                          + 0.12f * Mathf.Sin(2f * Mathf.PI * freq * 9.2f * t) * Mathf.Exp(-t * 40f);
                d[i] = s * env * amp;
            }
            return d;
        }

        private static float[] Sweep(float f0, float f1, float dur, float amp, bool square)
        {
            int n = (int)(dur * SampleRate);
            float[] d = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float k = i / (float)n;
                float f = Mathf.Lerp(f0, f1, k);
                phase += 2f * Mathf.PI * f / SampleRate;
                float s = Mathf.Sin(phase);
                if (square) s = Mathf.Sign(s) * 0.5f + s * 0.5f;
                float env = Mathf.Min(1f, i / 200f) * (1f - k) * (1f - k);
                d[i] = s * env * amp;
            }
            return d;
        }

        private static float[] Concat(params float[][] parts)
        {
            int total = 0;
            for (int i = 0; i < parts.Length; i++) total += parts[i].Length;
            float[] r = new float[total];
            int p = 0;
            for (int i = 0; i < parts.Length; i++) { System.Array.Copy(parts[i], 0, r, p, parts[i].Length); p += parts[i].Length; }
            return r;
        }

        private static float[] Delay(float[] a, float sec)
        {
            return Concat(new float[(int)(sec * SampleRate)], a);
        }

        private static float[] Mix(params float[][] parts)
        {
            int len = 0;
            for (int i = 0; i < parts.Length; i++) if (parts[i].Length > len) len = parts[i].Length;
            float[] r = new float[len];
            for (int i = 0; i < parts.Length; i++)
                for (int j = 0; j < parts[i].Length; j++) r[j] += parts[i][j];
            return r;
        }

        private static AudioClip Make(string name, float[] data)
        {
            AudioClip c = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            c.SetData(data, 0);
            return c;
        }

        private void BuildClips()
        {
            pop = Make("pop", Note(BaseFreq, 0.4f, 0.55f));
            tick = Make("tick", Note(BaseFreq * 1.5f, 0.12f, 0.35f));
            whoosh = Make("whoosh", Sweep(280f, 900f, 0.2f, 0.35f, false));
            error = Make("error", Sweep(170f, 110f, 0.18f, 0.4f, true));
            unlock = Make("unlock", Mix(Note(BaseFreq * 2f, 0.35f, 0.4f), Delay(Note(BaseFreq * 3f, 0.4f, 0.4f), 0.08f)));
            float c5 = BaseFreq, e5 = BaseFreq * 1.26f, g5 = BaseFreq * 1.5f, c6 = BaseFreq * 2f;
            win = Make("win", Mix(Note(c5, 0.5f, 0.5f), Delay(Note(e5, 0.5f, 0.5f), 0.13f), Delay(Note(g5, 0.5f, 0.5f), 0.26f), Delay(Note(c6, 0.9f, 0.55f), 0.39f)));
            fail = Make("fail", Mix(Note(g5, 0.5f, 0.5f), Delay(Note(e5 * 0.94f, 0.5f, 0.5f), 0.18f), Delay(Note(c5 * 0.84f, 0.9f, 0.55f), 0.36f)));
        }

        private void Play(AudioClip clip, float pitch, float vol)
        {
            if (clip == null) return;
            AudioSource s = sources[nextSource];
            nextSource = (nextSource + 1) % sources.Length;
            s.pitch = pitch;
            s.volume = vol * Volume;
            s.clip = clip;
            s.Play();
        }

        public void PlayTick() { Play(tick, 1f, 0.6f); }
        public void PlayWhoosh() { Play(whoosh, 1f, 0.6f); }
        public void PlayError() { Play(error, 1f, 0.7f); }
        public void PlayUnlock() { Play(unlock, 1f, 0.7f); }
        public void PlayWin() { Play(win, 1f, 0.9f); }
        public void PlayFail() { Play(fail, 1f, 0.9f); }

        /// <summary>Marimba pop; every hit inside the combo window is one semitone higher than the last.</summary>
        public void PlayComboPop()
        {
            if (Time.unscaledTime - lastHitTime > ComboWindow) combo = 0;
            else combo = Mathf.Min(combo + 1, MaxComboSemitones);
            lastHitTime = Time.unscaledTime;
            Play(pop, Mathf.Pow(2f, combo / 12f), 0.8f);
        }

        public void ResetCombo() { combo = 0; lastHitTime = -10f; }

        // ------------------------------------------------------------------ particles

        private void BuildParticles()
        {
            GameObject go = new GameObject("Particles");
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(0, 0, -2f);
            particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 0.55f;
            main.startSpeed = 0f;
            main.startSize = 0.2f;
            main.maxParticles = 600;
            main.gravityModifier = 0.6f;

            ParticleSystem.EmissionModule em = particles.emission;
            em.enabled = false;
            ParticleSystem.ShapeModule sh = particles.shape;
            sh.enabled = false;

            ParticleSystem.SizeOverLifetimeModule sol = particles.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.6f, 0.7f), new Keyframe(1f, 0f));
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

            ParticleSystemRenderer pr = go.GetComponent<ParticleSystemRenderer>();
            pr.renderMode = ParticleSystemRenderMode.Billboard;
            pr.sharedMaterial = Mats.Particle();
            pr.sortingOrder = 50;
            particles.Play();
        }

        public void Burst(Vector3 pos, Color color, int count = 12, float size = 0.16f, float speed = 3.2f)
        {
            if (particles == null) return;
            pos.z = -2f;
            for (int i = 0; i < count; i++)
            {
                ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                float a = Random.value * Mathf.PI * 2f;
                float s = speed * (0.4f + Random.value * 0.8f);
                p.position = pos;
                p.velocity = new Vector3(Mathf.Cos(a) * s, Mathf.Sin(a) * s, 0f);
                p.startColor = Color.Lerp(color, Color.white, Random.value * 0.35f);
                p.startSize = size * (0.7f + Random.value * 0.7f);
                p.startLifetime = 0.35f + Random.value * 0.35f;
                particles.Emit(p, 1);
            }
        }

        // ------------------------------------------------------------------ composite effects

        /// <summary>A Target Core layer was destroyed.</summary>
        public void BrickHit(Vector3 pos, Color color)
        {
            PlayComboPop();
            Burst(pos, color, 12, 0.17f);
            Shake(0.035f, 0.1f);
        }

        /// <summary>A block left the board through its gate.</summary>
        public void BlockLeft(Vector3 pos, Color color)
        {
            PlayWhoosh();
            Burst(pos, color, 14, 0.15f, 3.8f);
        }

        // ------------------------------------------------------------------ camera shake / squash

        public void Shake(float magnitude, float duration)
        {
            if (cam == null) return;
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(ShakeRoutine(magnitude, duration));
        }

        private IEnumerator ShakeRoutine(float mag, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = 1f - t / dur;
                cam.transform.position = camBasePos + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * mag * k;
                yield return null;
            }
            cam.transform.position = camBasePos;
            shakeRoutine = null;
        }

        /// <summary>Scales a transform up with overshoot and settles back to its original scale.</summary>
        public void Pop(Transform tr, float peak = 1.2f, float duration = 0.22f)
        {
            if (tr == null) return;
            StartCoroutine(PopRoutine(tr, peak, duration));
        }

        private IEnumerator PopRoutine(Transform tr, float peak, float dur)
        {
            Vector3 baseScale = tr.localScale;
            float t = 0f;
            while (t < dur && tr != null)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                float s = 1f + (peak - 1f) * Mathf.Sin(k * Mathf.PI);
                tr.localScale = baseScale * s;
                yield return null;
            }
            if (tr != null) tr.localScale = baseScale;
        }
    }
}
