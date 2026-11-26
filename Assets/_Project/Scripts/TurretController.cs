using System.Collections;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// A physical turret riding the conveyor. Ammo = tile count of the block that spawned it (shown on a 3D badge).
    /// It auto-targets monument parts of its colour, rotates to face them and fires pellets. When two turrets of the
    /// same colour meet they merge (magnetic pull + burst) into one bigger Mega-Turret with combined ammo and a
    /// continuous laser.
    /// </summary>
    public sealed class TurretController : MonoBehaviour
    {
        public BlockColor Color { get; private set; }
        public int Ammo { get; private set; }
        public bool IsMega { get; private set; }
        public bool Pulling { get { return pullTarget != null; } }
        public bool Finished { get { return finished; } }
        /// <summary>Clockwork-overcharged turret: its shots pierce the shell AND the inner layer (batch 4).</summary>
        public bool Piercing { get; set; }

        private ConveyorTrackManager track;
        private MonumentManager monument;
        private float s;
        private Transform head;
        private TextMesh badge;
        private Transform badgeT;
        private LineRenderer laser;
        private float armTimer = 0.7f;
        private float fireTimer;
        private float spawnT;
        private Vector3 flyFrom;
        private bool flyDone;
        private TurretController pullTarget;
        private bool finished;
        private float scaleMul = 1f;
        private Vector3 pos;

        public Vector3 Position { get { return pos; } }

        public static TurretController Create(ConveyorTrackManager trackMgr, MonumentManager mon, Transform parent,
            BlockColor color, int ammo, float startS, Vector3 from, bool piercing)
        {
            GameObject go = new GameObject("Turret_" + color);
            go.transform.SetParent(parent, false);
            TurretController t = go.AddComponent<TurretController>();
            t.Setup(trackMgr, mon, color, ammo, startS, from, piercing);
            return t;
        }

        private void Setup(ConveyorTrackManager trackMgr, MonumentManager mon, BlockColor color, int ammo, float startS, Vector3 from, bool piercing)
        {
            track = trackMgr;
            monument = mon;
            Color = color;
            Ammo = ammo;
            s = startS;
            flyFrom = from;
            Piercing = piercing;
            pos = from;

            Material body = GameMaterials.ForBlock(color);
            Material glow = GameMaterials.Glow(color);
            Prims.Make("Base", transform, Prims.Cylinder, GameMaterials.Gunmetal, new Vector3(0, 0.1f, 0), new Vector3(0.9f, 0.1f, 0.9f));
            Prims.Make("Body", transform, Prims.Cylinder, body, new Vector3(0, 0.38f, 0), new Vector3(0.72f, 0.18f, 0.72f));
            Prims.Make("Ring", transform, Prims.Cylinder, glow, new Vector3(0, 0.22f, 0), new Vector3(0.8f, 0.03f, 0.8f));
            head = new GameObject("Head").transform;
            head.SetParent(transform, false);
            head.localPosition = new Vector3(0, 0.62f, 0);
            Prims.Make("Dome", head, Prims.Sphere, body, Vector3.zero, new Vector3(0.62f, 0.5f, 0.62f));
            Prims.Make("Barrel", head, Prims.Cylinder, GameMaterials.Steel, new Vector3(0, 0.02f, 0.5f), new Vector3(0.16f, 0.32f, 0.16f), new Vector3(90f, 0f, 0f));
            Prims.Make("Muzzle", head, Prims.Cylinder, glow, new Vector3(0, 0.02f, 0.8f), new Vector3(0.2f, 0.04f, 0.2f), new Vector3(90f, 0f, 0f));

            badge = Prims.Label(transform, ammo.ToString(), 0.5f, UnityEngine.Color.white, new Vector3(0, 1.25f, 0));
            badgeT = badge.transform;
            transform.localScale = new Vector3(1.4f, 0.4f, 1.4f); // squish before the stretch-in
        }

        /// <summary>Make this turret a Mega-Turret (called on the survivor of a merge).</summary>
        public void BecomeMega()
        {
            if (IsMega) return;
            IsMega = true;
            scaleMul = 1.45f;
            Prims.Make("MegaBarrelL", head, Prims.Cylinder, GameMaterials.Steel, new Vector3(-0.2f, 0.02f, 0.5f), new Vector3(0.12f, 0.3f, 0.12f), new Vector3(90f, 0f, 0f));
            Prims.Make("MegaBarrelR", head, Prims.Cylinder, GameMaterials.Steel, new Vector3(0.2f, 0.02f, 0.5f), new Vector3(0.12f, 0.3f, 0.12f), new Vector3(90f, 0f, 0f));
            Prims.Make("MegaHalo", transform, Prims.Cylinder, GameMaterials.Glow(Color), new Vector3(0, 0.15f, 0), new Vector3(1.15f, 0.02f, 1.15f));
            GameObject lg = new GameObject("Laser");
            lg.transform.SetParent(transform, false);
            laser = lg.AddComponent<LineRenderer>();
            laser.positionCount = 2;
            laser.startWidth = 0.14f;
            laser.endWidth = 0.05f;
            laser.sharedMaterial = GameMaterials.UnlitGlow(GameMaterials.ColorOf(Color), 3f);
            laser.enabled = false;
        }

        public void AddAmmo(int n)
        {
            Ammo += n;
            badge.text = Ammo.ToString();
        }

        /// <summary>Magnetic pull-in: this turret slides to <paramref name="target"/> and is absorbed into it.</summary>
        public void MergeInto(TurretController target)
        {
            pullTarget = target;
            armTimer = 9999f;
        }

        public void Despawn()
        {
            if (finished) return;
            finished = true;
            if (laser != null) laser.enabled = false;
            StartCoroutine(DespawnRoutine());
        }

        private IEnumerator DespawnRoutine()
        {
            float t = 0f;
            Vector3 start = transform.localScale;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.22f;
                transform.localScale = Vector3.Lerp(start, Vector3.zero, t);
                yield return null;
            }
            Destroy(gameObject);
        }

        public bool CanStillHit()
        {
            return Ammo > 0 && monument != null && monument.FindTarget((int)Color) >= 0;
        }

        private void Update()
        {
            if (finished) return;
            float dt = Time.deltaTime;
            spawnT += dt;

            if (pullTarget != null)
            {
                if (pullTarget.finished) { pullTarget = null; return; }
                Vector3 to = pullTarget.pos;
                pos = Vector3.MoveTowards(pos, to, 14f * dt);
                transform.position = pos;
                transform.localScale = Vector3.one * Mathf.Lerp(scaleMul, 0.8f, 0.5f);
                if ((pos - to).sqrMagnitude < 0.06f)
                {
                    pullTarget.AddAmmo(Ammo);
                    pullTarget.BecomeMega();
                    if (FXManager.Instance != null)
                    {
                        FXManager.Instance.Burst(FxKind.Spark, to + new Vector3(0, 0.6f, 0), 26, GameMaterials.ColorOf(Color), 5f, 1.2f);
                        FXManager.Instance.Haptic();
                    }
                    if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.Merge);
                    track.NotifyMerged(this);
                    Despawn();
                    pullTarget.StartCoroutine(pullTarget.MergePop());
                }
                return;
            }

            // follow the conveyor
            s += track.Speed * dt;
            Vector3 ring = track.PointAt(s);
            if (!flyDone)
            {
                float f = Mathf.Clamp01(spawnT / 0.4f);
                float e = 1f - (1f - f) * (1f - f);
                pos = Vector3.Lerp(flyFrom, ring, e) + new Vector3(0f, Mathf.Sin(f * Mathf.PI) * 0.9f, 0f);
                if (f >= 1f) flyDone = true;
            }
            else pos = ring;
            transform.position = pos;

            // squash & stretch on spawn, then settle
            float k = Mathf.Clamp01(spawnT / 0.5f);
            float stretch = k < 1f ? 1f + 0.35f * Mathf.Sin(k * Mathf.PI * 2f) * (1f - k) : 1f;
            float squish = k < 1f ? 1f - 0.35f * Mathf.Sin(k * Mathf.PI * 2f) * (1f - k) : 1f;
            transform.localScale = new Vector3(squish, stretch, squish) * scaleMul * Mathf.Lerp(0.4f, 1f, Mathf.Clamp01(spawnT / 0.2f));

            if (Camera.main != null) badgeT.rotation = Camera.main.transform.rotation;

            // fire
            if (armTimer > 0f) { armTimer -= dt; return; }
            fireTimer -= dt;
            if (fireTimer > 0f) { if (laser != null && fireTimer < 0.05f) laser.enabled = false; return; }

            int part = monument != null ? monument.FindTarget((int)Color) : -1;
            if (part < 0 || Ammo <= 0)
            {
                if (laser != null) laser.enabled = false;
                fireTimer = 0.12f;
                return;
            }

            Vector3 tp = monument.PartWorld(part);
            Vector3 flat = tp - head.position;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.001f) head.rotation = Quaternion.LookRotation(flat.normalized, Vector3.up);

            Vector3 muzzle = head.position + head.forward * 0.9f;
            bool hit = monument.Shoot(part, muzzle, Color, IsMega, Piercing);
            if (hit)
            {
                if (!track.OverdriveActive)
                {
                    Ammo--;
                    badge.text = Ammo.ToString();
                }
                if (IsMega && laser != null)
                {
                    laser.enabled = true;
                    laser.SetPosition(0, muzzle);
                    laser.SetPosition(1, tp);
                }
                if (SoundSynth.Instance != null) SoundSynth.Instance.Play(IsMega ? Sfx.Laser : Sfx.TurretShoot, IsMega ? Random.Range(0.95f, 1.1f) : Random.Range(0.9f, 1.15f), IsMega ? 0.5f : 0.6f);
            }
            fireTimer = IsMega ? 0.09f : (track.OverdriveActive ? 0.18f : 0.42f);
        }

        public IEnumerator MergePop()
        {
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float p = 1f + 0.35f * Mathf.Sin(Mathf.Clamp01(t / 0.35f) * Mathf.PI);
                scaleMul = 1.45f * p;
                yield return null;
            }
            scaleMul = 1.45f;
        }
    }
}
