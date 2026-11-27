using System.Collections.Generic;
using UnityEngine;

namespace ColorGateBlast
{
    public enum Sfx
    {
        BlockPick, BlockSnap, GateExit, TurretShoot, Laser, MonumentBreak, DebrisClink, GlassSnap,
        IceCrack, ShutterClank, TapeWhir, PlasticPop, Tnt, Win, Fail, Click, ErrorBuzz, Merge, Tick,
        // batch 3 / 4
        Ratchet, MetalPing, PadlockSnap, Overdrive, PortalZap, TickTock, DialClick, Unlock,
        // batch 5 / 6
        WeldBreak, SteamHiss, PrismChime, MagnetClunk
    }

    /// <summary>
    /// Zero-asset audio: every sound is synthesised in code (AudioClip.Create) at startup - rubbery pick-ups,
    /// wooden-plastic snaps, noise bursts, resonant laser sweeps, metal clinks, glass snaps, marimba plucks.
    /// </summary>
    public sealed class SoundSynth : MonoBehaviour
    {
        public static SoundSynth Instance { get; private set; }

        private const int Rate = 44100;
        private const float BaseFreq = 523.25f; // C5
        /// <summary>Consecutive gate exits walk up the pentatonic scale C D E G A (then repeat an octave higher).</summary>
        private static readonly int[] ComboSemitones = { 0, 2, 4, 7, 9, 12, 14, 16, 19, 21 };

        public float MasterVolume = 0.85f;

        private AudioSource[] sources;
        private int next;
        private readonly Dictionary<Sfx, AudioClip[]> clips = new Dictionary<Sfx, AudioClip[]>();
        private AudioClip pluck;
        private System.Random rng = new System.Random(1234);

        private void Awake()
        {
            Instance = this;
            sources = new AudioSource[12];
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource s = gameObject.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 0f;
                sources[i] = s;
            }
            Build();
        }

        // ------------------------------------------------------------------ public API

        public void Play(Sfx sfx) { Play(sfx, 1f, 1f); }

        public void Play(Sfx sfx, float pitch, float volume)
        {
            AudioClip[] list;
            if (!clips.TryGetValue(sfx, out list) || list.Length == 0) return;
            AudioClip c = list[list.Length == 1 ? 0 : rng.Next(0, list.Length)];
            AudioSource s = sources[next];
            next = (next + 1) % sources.Length;
            s.clip = c;
            s.pitch = pitch;
            s.volume = Mathf.Clamp01(volume * MasterVolume);
            s.Play();
        }

        /// <summary>Marimba pluck for the n-th consecutive gate exit.</summary>
        public void PlayCombo(int step)
        {
            int idx = Mathf.Clamp(step, 0, ComboSemitones.Length - 1);
            Play(pluck, Mathf.Pow(2f, ComboSemitones[idx] / 12f), 0.9f);
        }

        private void Play(AudioClip c, float pitch, float volume)
        {
            AudioSource s = sources[next];
            next = (next + 1) % sources.Length;
            s.clip = c;
            s.pitch = pitch;
            s.volume = Mathf.Clamp01(volume * MasterVolume);
            s.Play();
        }

        // ------------------------------------------------------------------ synthesis helpers

        private static float[] Marimba(float freq, float dur, float amp)
        {
            int n = (int)(dur * Rate);
            float[] d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Exp(-t * 6.5f) * Mathf.Min(1f, t * 700f);
                float s = Mathf.Sin(2f * Mathf.PI * freq * t)
                          + 0.4f * Mathf.Sin(2f * Mathf.PI * freq * 4f * t) * Mathf.Exp(-t * 22f)
                          + 0.12f * Mathf.Sin(2f * Mathf.PI * freq * 9.2f * t) * Mathf.Exp(-t * 40f);
                d[i] = s * env * amp;
            }
            return d;
        }

        private static float[] Sweep(float f0, float f1, float dur, float amp, float squareMix, float decayPow)
        {
            int n = (int)(dur * Rate);
            float[] d = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float k = i / (float)n;
                float f = Mathf.Lerp(f0, f1, k);
                phase += 2f * Mathf.PI * f / Rate;
                float s = Mathf.Sin(phase);
                s = Mathf.Lerp(s, Mathf.Sign(s) * 0.6f, squareMix);
                float env = Mathf.Min(1f, i / 150f) * Mathf.Pow(1f - k, decayPow);
                d[i] = s * env * amp;
            }
            return d;
        }

        /// <summary>Decaying noise burst; hp = high-pass amount (0..1), lp = low-pass amount (0..1, 1 = off).</summary>
        private float[] Noise(float dur, float amp, float decay, float hp, float lp)
        {
            int n = (int)(dur * Rate);
            float[] d = new float[n];
            float lpState = 0f, hpState = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float w = (float)(rng.NextDouble() * 2.0 - 1.0);
                lpState += (w - lpState) * lp;
                float x = lpState;
                hpState += (x - hpState) * (1f - hp);
                x -= hpState * hp;
                d[i] = x * amp * Mathf.Exp(-t * decay) * Mathf.Min(1f, i / 40f);
            }
            return d;
        }

        private static float[] Ring(float freq, float dur, float amp, float decay, float inharmonic)
        {
            int n = (int)(dur * Rate);
            float[] d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float s = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.6f * Mathf.Sin(2f * Mathf.PI * freq * inharmonic * t) + 0.35f * Mathf.Sin(2f * Mathf.PI * freq * inharmonic * 1.7f * t);
                d[i] = s * amp * Mathf.Exp(-t * decay) * Mathf.Min(1f, i / 30f);
            }
            return d;
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

        private static float[] Delay(float[] a, float sec)
        {
            int pad = (int)(sec * Rate);
            float[] r = new float[pad + a.Length];
            System.Array.Copy(a, 0, r, pad, a.Length);
            return r;
        }

        private static AudioClip Clip(string name, float[] data)
        {
            for (int i = 0; i < data.Length; i++) data[i] = Mathf.Clamp(data[i], -1f, 1f);
            AudioClip c = AudioClip.Create(name, data.Length, 1, Rate, false);
            c.SetData(data, 0);
            return c;
        }

        private void Add(Sfx s, params AudioClip[] list) { clips[s] = list; }

        // ------------------------------------------------------------------ the sound set

        private void Build()
        {
            pluck = Clip("pluck", Marimba(BaseFreq, 0.55f, 0.6f));

            // Block_Pick: quick upward pitch-bend with rubbery resonance
            Add(Sfx.BlockPick, Clip("pick", Mix(Sweep(220f, 520f, 0.11f, 0.45f, 0f, 1.5f), Ring(330f, 0.12f, 0.15f, 28f, 1.5f))));
            // Block_Snap: high-frequency wooden/plastic tactile pop
            Add(Sfx.BlockSnap, Clip("snap1", Mix(Ring(1200f, 0.08f, 0.4f, 55f, 2.3f), Noise(0.03f, 0.25f, 120f, 0.6f, 0.9f))),
                               Clip("snap2", Mix(Ring(1500f, 0.08f, 0.4f, 60f, 2.1f), Noise(0.03f, 0.25f, 120f, 0.6f, 0.9f))));
            // Gate_Exit: low rising whoosh + mechanical zap
            Add(Sfx.GateExit, Clip("gate", Mix(Sweep(90f, 420f, 0.28f, 0.45f, 0.3f, 1.2f), Noise(0.22f, 0.22f, 9f, 0.1f, 0.25f), Delay(Sweep(900f, 300f, 0.08f, 0.25f, 0.6f, 2f), 0.06f))));
            // Turret_Shoot: quick decaying noise burst; Laser: resonant sweep
            Add(Sfx.TurretShoot, Clip("shoot1", Mix(Noise(0.08f, 0.5f, 45f, 0.35f, 0.8f), Sweep(700f, 180f, 0.07f, 0.3f, 0.2f, 2f))),
                                 Clip("shoot2", Mix(Noise(0.08f, 0.5f, 45f, 0.35f, 0.8f), Sweep(820f, 200f, 0.07f, 0.3f, 0.2f, 2f))));
            Add(Sfx.Laser, Clip("laser", Mix(Sweep(1800f, 700f, 0.16f, 0.3f, 0.1f, 0.7f), Sweep(1810f, 705f, 0.16f, 0.25f, 0.1f, 0.7f))));
            // Monument_Break / Debris: crashing metal clinks and glass snaps
            Add(Sfx.MonumentBreak, Clip("break", Mix(Noise(0.35f, 0.5f, 9f, 0.25f, 0.6f), Ring(220f, 0.4f, 0.3f, 8f, 2.7f), Ring(540f, 0.35f, 0.2f, 12f, 3.1f))));
            Add(Sfx.DebrisClink, Clip("clink1", Ring(2100f, 0.16f, 0.35f, 30f, 2.76f)), Clip("clink2", Ring(2700f, 0.14f, 0.35f, 34f, 2.4f)),
                                 Clip("clink3", Ring(1650f, 0.18f, 0.35f, 26f, 3.2f)), Clip("clink4", Ring(3100f, 0.12f, 0.3f, 38f, 2.5f)));
            Add(Sfx.GlassSnap, Clip("glass1", Mix(Noise(0.09f, 0.45f, 50f, 0.85f, 1f), Ring(4200f, 0.12f, 0.25f, 40f, 1.4f))),
                               Clip("glass2", Mix(Noise(0.09f, 0.45f, 50f, 0.85f, 1f), Ring(3600f, 0.12f, 0.25f, 40f, 1.5f))));
            // Ice crack: high-passed noise transient + crystalline chime
            Add(Sfx.IceCrack, Clip("ice", Mix(Noise(0.07f, 0.6f, 60f, 0.9f, 1f), Delay(Ring(3200f, 0.35f, 0.18f, 9f, 1.6f), 0.01f), Delay(Ring(4800f, 0.3f, 0.12f, 11f, 1.5f), 0.03f))));
            // Shutter clank: low metallic thud + spring ring
            Add(Sfx.ShutterClank, Clip("clank", Mix(Ring(95f, 0.22f, 0.55f, 18f, 2.2f), Noise(0.06f, 0.35f, 70f, 0.1f, 0.5f), Delay(Ring(780f, 0.45f, 0.18f, 7f, 2.9f), 0.015f))));
            // Tape whir / plastic pop
            Add(Sfx.TapeWhir, Clip("tape", Mix(Sweep(160f, 320f, 0.35f, 0.18f, 0.5f, 0.3f), Noise(0.35f, 0.08f, 2f, 0.4f, 0.5f))));
            Add(Sfx.PlasticPop, Clip("ppop", Mix(Ring(700f, 0.07f, 0.4f, 70f, 1.9f), Noise(0.02f, 0.2f, 150f, 0.5f, 0.9f))));
            Add(Sfx.Tnt, Clip("tnt", Mix(Noise(0.7f, 0.8f, 5f, 0f, 0.15f), Sweep(110f, 35f, 0.6f, 0.7f, 0f, 1.2f), Ring(70f, 0.8f, 0.4f, 4f, 2f))));
            Add(Sfx.Merge, Clip("merge", Mix(Sweep(300f, 900f, 0.25f, 0.3f, 0.1f, 1f), Delay(Marimba(BaseFreq * 1.5f, 0.4f, 0.4f), 0.2f))));
            Add(Sfx.Click, Clip("click", Mix(Ring(900f, 0.04f, 0.35f, 90f, 1.7f), Noise(0.015f, 0.2f, 200f, 0.5f, 0.9f))));
            Add(Sfx.ErrorBuzz, Clip("err", Sweep(170f, 110f, 0.18f, 0.4f, 0.8f, 1f)));
            Add(Sfx.Tick, Clip("tick", Mix(Ring(2400f, 0.03f, 0.3f, 130f, 1.5f), Noise(0.01f, 0.15f, 250f, 0.8f, 1f))));
            float c5 = BaseFreq, e5 = BaseFreq * 1.26f, g5 = BaseFreq * 1.5f, c6 = BaseFreq * 2f;
            Add(Sfx.Win, Clip("win", Mix(Marimba(c5, 0.6f, 0.5f), Delay(Marimba(e5, 0.6f, 0.5f), 0.13f), Delay(Marimba(g5, 0.6f, 0.5f), 0.26f), Delay(Marimba(c6, 1f, 0.55f), 0.39f))));
            Add(Sfx.Fail, Clip("fail", Mix(Marimba(g5, 0.5f, 0.5f), Delay(Marimba(e5 * 0.94f, 0.5f, 0.5f), 0.18f), Delay(Marimba(c5 * 0.84f, 0.9f, 0.55f), 0.36f))));
            // batch 3 / 4
            Add(Sfx.Ratchet, Clip("ratchet", Mix(Ring(1800f, 0.03f, 0.3f, 120f, 1.6f), Delay(Ring(1900f, 0.03f, 0.3f, 120f, 1.6f), 0.06f), Delay(Ring(2000f, 0.03f, 0.3f, 120f, 1.6f), 0.12f), Delay(Ring(2100f, 0.04f, 0.3f, 100f, 1.6f), 0.18f))));
            Add(Sfx.MetalPing, Clip("ping", Ring(2600f, 0.35f, 0.4f, 12f, 2.4f)));
            Add(Sfx.PadlockSnap, Clip("padlock", Mix(Ring(1500f, 0.12f, 0.45f, 35f, 2.2f), Delay(Marimba(BaseFreq * 3f, 0.5f, 0.35f), 0.04f), Noise(0.04f, 0.3f, 90f, 0.5f, 0.8f))));
            Add(Sfx.Overdrive, Clip("overdrive", Mix(Marimba(c5, 0.6f, 0.5f), Delay(Marimba(e5, 0.6f, 0.5f), 0.12f), Delay(Marimba(g5, 0.9f, 0.55f), 0.24f))));
            Add(Sfx.PortalZap, Clip("zap", Mix(Sweep(300f, 2400f, 0.18f, 0.35f, 0.4f, 0.6f), Delay(Sweep(2400f, 500f, 0.14f, 0.3f, 0.3f, 1f), 0.08f), Noise(0.1f, 0.15f, 25f, 0.7f, 1f))));
            Add(Sfx.TickTock, Clip("tick", Mix(Ring(2200f, 0.03f, 0.3f, 140f, 1.5f), Noise(0.01f, 0.12f, 250f, 0.8f, 1f))),
                              Clip("tock", Mix(Ring(1500f, 0.04f, 0.3f, 120f, 1.5f), Noise(0.01f, 0.12f, 250f, 0.8f, 1f))));
            Add(Sfx.DialClick, Clip("dial", Mix(Ring(1300f, 0.025f, 0.3f, 150f, 1.6f), Noise(0.008f, 0.12f, 300f, 0.7f, 1f))));
            Add(Sfx.WeldBreak, Clip("weld", Mix(Ring(1900f, 0.1f, 0.45f, 40f, 2.3f), Noise(0.08f, 0.5f, 40f, 0.5f, 0.9f), Delay(Ring(620f, 0.25f, 0.3f, 14f, 2.9f), 0.02f), Delay(Sweep(2600f, 800f, 0.1f, 0.25f, 0.5f, 1.2f), 0.01f))));
            Add(Sfx.SteamHiss, Clip("hiss", Mix(Noise(0.45f, 0.35f, 5f, 0.55f, 0.95f), Delay(Noise(0.3f, 0.12f, 6f, 0.7f, 1f), 0.05f))));
            Add(Sfx.PrismChime, Clip("prism1", Mix(Ring(3300f, 0.45f, 0.22f, 8f, 1.5f), Delay(Ring(4400f, 0.35f, 0.14f, 10f, 1.5f), 0.025f))),
                                Clip("prism2", Mix(Ring(3700f, 0.45f, 0.22f, 8f, 1.5f), Delay(Ring(4900f, 0.35f, 0.14f, 10f, 1.5f), 0.025f))));
            Add(Sfx.MagnetClunk, Clip("clunk", Mix(Ring(130f, 0.22f, 0.6f, 20f, 2.1f), Noise(0.05f, 0.4f, 60f, 0.1f, 0.5f), Delay(Ring(900f, 0.18f, 0.2f, 14f, 2.7f), 0.02f))));
            Add(Sfx.Unlock, Clip("unlock", Mix(Marimba(BaseFreq * 2f, 0.4f, 0.4f), Delay(Marimba(BaseFreq * 3f, 0.5f, 0.4f), 0.08f))));
        }
    }
}
