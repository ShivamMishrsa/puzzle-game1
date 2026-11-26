using System.Collections;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// A coloured turret riding the perimeter conveyor. It carries ammo equal to the tile count of the block that
    /// spawned it and fires at every matching, exposed brick it passes. Driven by <see cref="ConveyorTrackManager"/>.
    /// </summary>
    public sealed class TurretController : MonoBehaviour
    {
        public BlockColor Color { get; private set; }
        public int Ammo { get; private set; }
        public float Progress { get; private set; }

        private ConveyorTrackManager track;
        private TargetCoreManager core;
        private int cw, ch, ringLen;
        private int lastIdx;
        private float speed;
        private Vector3 flyStartOffset;
        private float flyT;
        private Transform body;
        private TextMesh ammoLabel;
        private bool finished;

        public static TurretController Create(ConveyorTrackManager track, TargetCoreManager core, Transform parent,
            BlockColor color, int ammo, float startProgress, Vector3 flyFrom, float cellSize, int coreW, int coreH, float speed)
        {
            GameObject go = new GameObject("Turret_" + color);
            go.transform.SetParent(parent, false);
            TurretController t = go.AddComponent<TurretController>();
            t.Setup(track, core, color, ammo, startProgress, flyFrom, cellSize, coreW, coreH, speed);
            return t;
        }

        private void Setup(ConveyorTrackManager trackMgr, TargetCoreManager coreMgr, BlockColor color, int ammo,
            float startProgress, Vector3 flyFrom, float cellSize, int coreW, int coreH, float spd)
        {
            track = trackMgr;
            core = coreMgr;
            Color = color;
            Ammo = ammo;
            Progress = startProgress;
            cw = coreW;
            ch = coreH;
            ringLen = RingPath.Length(cw, ch);
            lastIdx = Mathf.FloorToInt(startProgress) % ringLen;
            speed = spd;

            float size = cellSize * 0.8f;
            float depth = cellSize * 0.34f;

            GameObject b = new GameObject("Body");
            b.transform.SetParent(transform, false);
            body = b.transform;
            b.AddComponent<MeshFilter>().sharedMesh = ProceduralMeshes.Slab(size, size, depth, size * 0.3f, size * 0.12f, "Turret");
            b.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.Get(color), 0.2f);

            // barrel: a small triangle pointing at the core (+y of the body)
            MeshBuilder mb = new MeshBuilder();
            float a = size * 0.22f;
            mb.AddTriangle(new Vector3(0, size * 0.46f, 0), new Vector3(a, size * 0.12f, 0), new Vector3(-a, size * 0.12f, 0));
            GameObject nose = new GameObject("Nose");
            nose.transform.SetParent(body, false);
            nose.transform.localPosition = new Vector3(0, 0, -depth - 0.01f);
            nose.AddComponent<MeshFilter>().sharedMesh = mb.ToMesh("Nose");
            nose.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.Lighten(ColorPalette.Get(color), 0.7f), 0.3f);

            ammoLabel = Mats.Label(transform, ammo.ToString(), size * 0.42f, ColorPalette.Ink, new Vector3(0, -size * 0.1f, -depth - 0.03f));

            Vector3 entry = track.RingWorld(startProgress);
            flyStartOffset = flyFrom - entry;
            flyStartOffset.z = 0f;
            flyT = 0f;
            transform.position = entry + flyStartOffset;
            transform.localScale = Vector3.one * 0.2f;
        }

        public bool CanStillHit()
        {
            return Ammo > 0 && core.Model != null && core.Model.FindHit((int)Color) >= 0;
        }

        /// <summary>Advance along the loop and fire at any firing cell we arrive on.</summary>
        public void Tick(float dt)
        {
            if (finished) return;

            Progress += speed * dt;
            if (Progress >= ringLen) Progress -= ringLen;
            int idx = Mathf.FloorToInt(Progress) % ringLen;
            int guard = 0;
            while (lastIdx != idx && guard++ < ringLen)
            {
                lastIdx = (lastIdx + 1) % ringLen;
                TryFire(lastIdx);
            }

            flyT = Mathf.Min(1f, flyT + dt / 0.4f);
            float ease = 1f - (1f - flyT) * (1f - flyT);
            Vector3 pos = track.RingWorld(Progress) + flyStartOffset * (1f - ease);
            pos.z = -0.15f;
            transform.position = pos;
            transform.localScale = Vector3.one * Mathf.Lerp(transform.localScale.x, 1f, 1f - Mathf.Exp(-14f * dt));

            // keep the barrel pointed at the core centre
            Vector3 toCore = core.CenterWorld - pos;
            float ang = Mathf.Atan2(toCore.y, toCore.x) * Mathf.Rad2Deg - 90f;
            body.localRotation = Quaternion.Euler(0, 0, ang);
        }

        private void TryFire(int idx)
        {
            if (Ammo <= 0) return;
            Side side;
            int lane;
            if (!RingPath.FireInfo(idx, cw, ch, out side, out lane)) return;
            if (core.TryFire((int)Color, side, lane, transform.position))
            {
                Ammo--;
                ammoLabel.text = Ammo.ToString();
                if (JuiceManager.Instance != null) JuiceManager.Instance.Pop(body, 1.15f, 0.12f);
            }
        }

        /// <summary>Shrink away and destroy (the track has already freed this slot).</summary>
        public void Despawn()
        {
            if (finished) return;
            finished = true;
            StartCoroutine(DespawnRoutine());
        }

        private IEnumerator DespawnRoutine()
        {
            float t = 0f;
            Vector3 start = transform.localScale;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.2f;
                transform.localScale = Vector3.Lerp(start, Vector3.zero, t);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
