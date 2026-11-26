using System;
using System.Collections;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>Visual of one polyomino block on the board. All logic lives in BoardModel; this only animates.</summary>
    public sealed class BlockView : MonoBehaviour
    {
        public int Index;

        private BlockSpec spec;
        private float cs;
        private float depth;
        private MeshRenderer body;
        private Material realMaterial;
        private GameObject frostRoot;
        private TextMesh frostLabel;
        private TextMesh centerLabel;
        private Vector3 target;
        private Vector3 smoothed;
        private Vector3 shakeOffset;
        private float lift;
        private bool revealed;
        private bool exiting;

        public bool IsMystery { get { return spec.kind == BlockKind.Mystery && !revealed; } }

        public void Init(int index, BlockSpec blockSpec, float cellSize, Vector3 worldPos)
        {
            Index = index;
            spec = blockSpec;
            cs = cellSize;
            depth = cs * 0.30f;
            target = worldPos;
            smoothed = worldPos;
            transform.position = worldPos;

            MeshFilter mf = gameObject.AddComponent<MeshFilter>();
            body = gameObject.AddComponent<MeshRenderer>();
            mf.sharedMesh = ProceduralMeshes.Polyomino(spec.cells, cs, depth, "Block_" + index);

            Color baseColor = ColorPalette.Get(spec.color);
            if (spec.kind == BlockKind.Ice) baseColor = ColorPalette.Lighten(baseColor, 0.45f);
            realMaterial = Mats.Lit(baseColor);

            Vector3 centroid = ProceduralMeshes.Centroid(spec.cells, cs);
            Vector3 labelPos = centroid + new Vector3(0, 0, -depth - 0.02f);

            if (spec.kind == BlockKind.Mystery)
            {
                body.sharedMaterial = Mats.Lit(ColorPalette.Mystery);
                centerLabel = Mats.Label(transform, "?", cs * 0.55f, ColorPalette.Ink, labelPos);
            }
            else
            {
                body.sharedMaterial = realMaterial;
                revealed = true;
            }

            if (spec.kind == BlockKind.Key)
                centerLabel = Mats.Label(transform, "K", cs * 0.55f, ColorPalette.Ink, labelPos);

            if (spec.kind == BlockKind.Ice) BuildFrost(centroid);
            if (spec.axis != MoveAxis.Free) BuildArrows(centroid);
        }

        private void BuildFrost(Vector3 centroid)
        {
            frostRoot = new GameObject("Frost");
            frostRoot.transform.SetParent(transform, false);
            frostRoot.transform.localPosition = new Vector3(0, 0, -depth - 0.015f);
            MeshBuilder mb = new MeshBuilder();
            float s = cs * 0.62f;
            for (int i = 0; i < spec.cells.Length; i++)
                mb.AddSlab(new Vector2(spec.cells[i].x * cs, spec.cells[i].y * cs), s, s, 0.03f, s * 0.3f, s * 0.1f);
            frostRoot.AddComponent<MeshFilter>().sharedMesh = mb.ToMesh("Frost");
            frostRoot.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.Frost, 0.6f);
            frostLabel = Mats.Label(frostRoot.transform, spec.iceThawExits.ToString(), cs * 0.45f, ColorPalette.Ink, centroid + new Vector3(0, 0, -0.04f));
        }

        private void BuildArrows(Vector3 centroid)
        {
            GameObject go = new GameObject("Arrows");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, 0, -depth - 0.012f);
            MeshBuilder mb = new MeshBuilder();
            float a = cs * 0.16f;
            float off = cs * 0.30f;
            if (spec.axis == MoveAxis.Horizontal)
            {
                mb.AddTriangle(centroid + new Vector3(-off - a, 0, 0), centroid + new Vector3(-off + a * 0.6f, a, 0), centroid + new Vector3(-off + a * 0.6f, -a, 0));
                mb.AddTriangle(centroid + new Vector3(off + a, 0, 0), centroid + new Vector3(off - a * 0.6f, a, 0), centroid + new Vector3(off - a * 0.6f, -a, 0));
            }
            else
            {
                mb.AddTriangle(centroid + new Vector3(0, off + a, 0), centroid + new Vector3(a, off - a * 0.6f, 0), centroid + new Vector3(-a, off - a * 0.6f, 0));
                mb.AddTriangle(centroid + new Vector3(0, -off - a, 0), centroid + new Vector3(a, -off + a * 0.6f, 0), centroid + new Vector3(-a, -off + a * 0.6f, 0));
            }
            go.AddComponent<MeshFilter>().sharedMesh = mb.ToMesh("Arrows");
            go.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(new Color(1f, 1f, 1f), 0.2f);
        }

        public void SetTarget(Vector3 worldPos) { target = worldPos; }

        public void SetLifted(bool on)
        {
            lift = on ? -0.25f : 0f;
            transform.localScale = on ? Vector3.one * 1.05f : Vector3.one;
        }

        /// <summary>Reveals a mystery block's colour the first time it is touched.</summary>
        public void Reveal()
        {
            if (revealed) return;
            revealed = true;
            body.sharedMaterial = realMaterial;
            if (centerLabel != null && spec.kind == BlockKind.Mystery) { Destroy(centerLabel.gameObject); centerLabel = null; }
            if (JuiceManager.Instance != null)
            {
                JuiceManager.Instance.Pop(transform, 1.18f, 0.2f);
                JuiceManager.Instance.Burst(transform.position, ColorPalette.Get(spec.color), 8, cs * 0.12f);
            }
        }

        public void SetIceRemaining(int remaining)
        {
            if (frostRoot == null) return;
            if (remaining <= 0)
            {
                Destroy(frostRoot);
                frostRoot = null;
                if (JuiceManager.Instance != null)
                {
                    JuiceManager.Instance.Pop(transform, 1.15f, 0.25f);
                    JuiceManager.Instance.Burst(transform.position, ColorPalette.Frost, 10, cs * 0.1f);
                    JuiceManager.Instance.PlayUnlock();
                }
            }
            else if (frostLabel != null)
            {
                frostLabel.text = remaining.ToString();
            }
        }

        public void Shake() { if (!exiting) StartCoroutine(ShakeRoutine()); }

        private IEnumerator ShakeRoutine()
        {
            float t = 0f;
            while (t < 0.28f)
            {
                t += Time.deltaTime;
                float k = 1f - t / 0.28f;
                shakeOffset = new Vector3(Mathf.Sin(t * 70f) * 0.09f * k, 0f, 0f);
                yield return null;
            }
            shakeOffset = Vector3.zero;
        }

        /// <summary>Slides out through the gate in <paramref name="worldDir"/> and destroys itself.</summary>
        public void PlayExit(Vector3 worldDir, float travel, Action onDone)
        {
            exiting = true;
            StopAllCoroutines();
            shakeOffset = Vector3.zero;
            StartCoroutine(ExitRoutine(worldDir, travel, onDone));
        }

        private IEnumerator ExitRoutine(Vector3 dir, float travel, Action onDone)
        {
            Vector3 start = smoothed;
            float t = 0f, dur = 0.3f;
            while (t < 1f)
            {
                t += Time.deltaTime / dur;
                float e = t * t;
                smoothed = start + dir * (travel * e);
                transform.position = smoothed + new Vector3(0, 0, lift);
                transform.localScale = Vector3.one * Mathf.Lerp(1.05f, 0.7f, e);
                yield return null;
            }
            if (onDone != null) onDone();
            Destroy(gameObject);
        }

        private void Update()
        {
            if (exiting) return;
            float k = 1f - Mathf.Exp(-26f * Time.deltaTime);
            smoothed = Vector3.Lerp(smoothed, target, k);
            transform.position = smoothed + shakeOffset + new Vector3(0, 0, lift);
        }
    }
}
