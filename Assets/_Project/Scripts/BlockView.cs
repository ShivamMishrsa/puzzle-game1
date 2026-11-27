using System;
using System.Collections;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>3D visual of one block on the recess tray. All rules live in BoardModel; this only animates.</summary>
    public sealed class BlockView : MonoBehaviour
    {
        public int Index;

        private BlockInfo info;
        private float cs;
        private float height;
        private MeshRenderer body;
        private MeshFilter bodyFilter;
        private GameObject iceShell;
        private GameObject outline;
        private int iceLayers;
        private Vector3 target, smoothed, shakeOffset;
        private float lift;
        private bool exiting;
        private bool hint;
        private float hintPhase;
        private MaterialPropertyBlock mpb;
        private Vector3 centroidLocal;

        public Vector3 CentroidWorld { get { return transform.position + transform.rotation * (centroidLocal * transform.localScale.x); } }

        /// <summary>Block body mesh: one rounded slab per cell plus bridges so it reads as a single machined piece.</summary>
        public static MeshBuilder Poly(Int2[] cells, float cs, float sizeFactor, float height, float y0, int seg)
        {
            float size = cs * sizeFactor;
            float radius = size * 0.24f;
            float bevel = size * 0.1f;
            MeshBuilder b = new MeshBuilder();
            for (int i = 0; i < cells.Length; i++)
                b.AddSlab(cells[i].x * cs, cells[i].y * cs, size, size, height, radius, bevel, y0, seg);
            for (int i = 0; i < cells.Length; i++)
            {
                for (int j = 0; j < cells.Length; j++)
                {
                    if (cells[j].y == cells[i].y && cells[j].x == cells[i].x + 1)
                        b.AddSlab((cells[i].x + 0.5f) * cs, cells[i].y * cs, cs, size, height - 0.004f, radius, bevel, y0, seg);
                    if (cells[j].x == cells[i].x && cells[j].y == cells[i].y + 1)
                        b.AddSlab(cells[i].x * cs, (cells[i].y + 0.5f) * cs, size, cs, height - 0.004f, radius, bevel, y0, seg);
                }
            }
            return b;
        }

        public void Init(int index, BlockInfo blockInfo, float cellSize, Vector3 worldPos)
        {
            Index = index;
            info = blockInfo;
            cs = cellSize;
            height = cs * 0.42f;
            target = worldPos;
            smoothed = worldPos;
            transform.position = worldPos;
            mpb = new MaterialPropertyBlock();

            float cx = 0, cz = 0;
            for (int i = 0; i < info.cells.Length; i++) { cx += info.cells[i].x; cz += info.cells[i].y; }
            centroidLocal = new Vector3(cx / info.cells.Length * cs, height, cz / info.cells.Length * cs);

            bodyFilter = gameObject.AddComponent<MeshFilter>();
            body = gameObject.AddComponent<MeshRenderer>();
            bodyFilter.sharedMesh = Poly(info.cells, cs, 0.9f, height, 0f, 3).ToMesh("Block", 0.6f);
            body.sharedMaterial = info.IsPrism ? GameMaterials.Brass : GameMaterials.ForBlock(info.color);

            if (info.IsPrism) BuildPrism();
            else if (!info.hasInner && info.axis == MoveAxis.Free) BuildPanels();
            BuildRivets();
            if (info.hasInner) BuildStripe();
            if (info.axis != MoveAxis.Free) BuildArrows();
            if (info.hasKey) BuildKey();
            if (info.iceLayer > 0) SetIce(info.iceLayer, false);
        }

        private Renderer prismCore, prismNext;
        private float prismFlash;

        /// <summary>Colour-shifting prism: brushed brass frame around a faceted glass core that glows with the active colour; a corner pip shows the next colour.</summary>
        private void BuildPrism()
        {
            Vector3 c = new Vector3(centroidLocal.x, height + 0.07f * cs, centroidLocal.z);
            GameObject core = new GameObject("PrismCore");
            core.transform.SetParent(transform, false);
            core.transform.localPosition = c;
            Prims.Make("GemA", core.transform, Prims.Cube, GameMaterials.Glow(info.color), Vector3.zero, Vector3.one * (0.36f * cs), new Vector3(45f, 0f, 45f));
            GameObject b = Prims.Make("GemB", core.transform, Prims.Cube, GameMaterials.Glow(info.color), Vector3.zero, Vector3.one * (0.36f * cs), new Vector3(0f, 45f, 0f));
            prismCore = core.transform.GetChild(0).GetComponent<Renderer>();
            Renderer r2 = b.GetComponent<Renderer>();
            core.AddComponent<KeySpin>();
            GameObject pip = Prims.Make("NextPip", transform, Prims.Sphere, GameMaterials.Glow(info.color), new Vector3(centroidLocal.x + 0.3f * cs, height + 0.04f * cs, centroidLocal.z + 0.3f * cs), Vector3.one * (0.11f * cs));
            prismNext = pip.GetComponent<Renderer>();
            prismExtra = r2;
        }

        private Renderer prismExtra;

        /// <summary>Show the active colour in the glass and the next colour on the corner pip.</summary>
        public void SetPrism(BlockColor current, BlockColor next, bool flash)
        {
            if (prismCore == null) return;
            Material m = GameMaterials.Glow(current);
            prismCore.sharedMaterial = m;
            if (prismExtra != null) prismExtra.sharedMaterial = m;
            if (prismNext != null) prismNext.sharedMaterial = GameMaterials.Glow(next);
            if (flash) prismFlash = 0.35f;
        }

        private void LateUpdate()
        {
            if (prismFlash > 0f && prismCore != null)
            {
                prismFlash -= Time.deltaTime;
                float k = 1f + Mathf.Sin(Mathf.Clamp01(prismFlash / 0.35f) * Mathf.PI) * 0.45f;
                prismCore.transform.parent.localScale = Vector3.one * k;
            }
        }

        /// <summary>Square top panel pressed into each cell (darker, bevelled) so every piece reads as machined metal.</summary>
        private void BuildPanels()
        {
            MeshBuilder pb = new MeshBuilder();
            float s = cs * 0.52f;
            for (int i = 0; i < info.cells.Length; i++)
                pb.AddSlab(info.cells[i].x * cs, info.cells[i].y * cs, s, s, 0.02f, cs * 0.07f, 0.012f, height - 0.004f, 2);
            GameObject go = new GameObject("TopPanels");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = pb.ToMesh("TopPanels", 0.6f);
            go.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.PanelOf(info.color);
        }

        private void BuildRivets()
        {
            MeshBuilder rb = new MeshBuilder();
            float o = cs * 0.3f, r = cs * 0.04f;
            for (int i = 0; i < info.cells.Length; i++)
                for (int sx = -1; sx <= 1; sx += 2)
                    for (int sz = -1; sz <= 1; sz += 2)
                        rb.AddSphere(new Vector3(info.cells[i].x * cs + sx * o, height - r * 0.2f, info.cells[i].y * cs + sz * o), r, 6, 3);
            GameObject go = new GameObject("Rivets");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = rb.ToMesh("Rivets");
            go.AddComponent<MeshRenderer>().sharedMaterial = info.color == BlockColor.Gray ? GameMaterials.Gunmetal : GameMaterials.Brass;
        }

        /// <summary>Recessed window showing the inner colour: dark frame + glowing inner-colour plate on the top face.</summary>
        private void BuildStripe()
        {
            GameObject frame = new GameObject("StripeFrame");
            frame.transform.SetParent(transform, false);
            frame.AddComponent<MeshFilter>().sharedMesh = Poly(info.cells, cs, 0.5f, 0.012f, height - 0.004f, 2).ToMesh("StripeFrame");
            frame.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.Rubber;

            GameObject inner = new GameObject("InnerColor");
            inner.transform.SetParent(transform, false);
            inner.AddComponent<MeshFilter>().sharedMesh = Poly(info.cells, cs, 0.4f, 0.014f, height + 0.004f, 2).ToMesh("InnerColor");
            inner.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.Glow(info.innerColor);
        }

        /// <summary>A gleaming 3D brass key lying on top of the block.</summary>
        private void BuildKey()
        {
            GameObject key = new GameObject("BrassKey");
            key.transform.SetParent(transform, false);
            key.transform.localPosition = new Vector3(centroidLocal.x, height + 0.08f, centroidLocal.z);
            Material brass = GameMaterials.Brass;
            Prims.Make("Bow", key.transform, Prims.Sphere, brass, new Vector3(-0.22f * cs, 0f, 0f), new Vector3(0.26f * cs, 0.07f * cs, 0.26f * cs));
            Prims.Make("Hole", key.transform, Prims.Sphere, GameMaterials.Rubber, new Vector3(-0.22f * cs, 0.03f * cs, 0f), new Vector3(0.12f * cs, 0.05f * cs, 0.12f * cs));
            Prims.Make("Shaft", key.transform, Prims.Cube, brass, new Vector3(0.1f * cs, 0f, 0f), new Vector3(0.5f * cs, 0.06f * cs, 0.07f * cs));
            Prims.Make("ToothA", key.transform, Prims.Cube, brass, new Vector3(0.3f * cs, 0f, 0.07f * cs), new Vector3(0.07f * cs, 0.06f * cs, 0.12f * cs));
            Prims.Make("ToothB", key.transform, Prims.Cube, brass, new Vector3(0.2f * cs, 0f, 0.06f * cs), new Vector3(0.07f * cs, 0.06f * cs, 0.1f * cs));
            key.AddComponent<KeySpin>();
        }

        private void BuildArrows()
        {
            MeshBuilder mb = new MeshBuilder();
            float a = cs * 0.14f, off = cs * 0.28f, y = height + 0.012f;
            for (int i = 0; i < info.cells.Length; i++)
            {
                Vector3 c = new Vector3(info.cells[i].x * cs, y, info.cells[i].y * cs);
                if (info.axis == MoveAxis.Horizontal)
                {
                    mb.AddTriangleUp(c + new Vector3(-off - a, 0, 0), c + new Vector3(-off + a * 0.6f, 0, a), c + new Vector3(-off + a * 0.6f, 0, -a));
                    mb.AddTriangleUp(c + new Vector3(off + a, 0, 0), c + new Vector3(off - a * 0.6f, 0, a), c + new Vector3(off - a * 0.6f, 0, -a));
                }
                else
                {
                    mb.AddTriangleUp(c + new Vector3(0, 0, off + a), c + new Vector3(a, 0, off - a * 0.6f), c + new Vector3(-a, 0, off - a * 0.6f));
                    mb.AddTriangleUp(c + new Vector3(0, 0, -off - a), c + new Vector3(a, 0, -off + a * 0.6f), c + new Vector3(-a, 0, -off + a * 0.6f));
                }
            }
            GameObject go = new GameObject("Arrows");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mb.ToMesh("Arrows");
            go.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.Steel;
        }

        // ------------------------------------------------------------------ ice

        public int IceLayers { get { return iceLayers; } }

        /// <summary>Sets the frosted shell: 2 = thick, 1 = thinner with fracture lines, 0 = gone (shards).</summary>
        public void SetIce(int layers, bool animate)
        {
            int before = iceLayers;
            iceLayers = layers;
            if (layers <= 0)
            {
                if (iceShell != null)
                {
                    if (FXManager.Instance != null) FXManager.Instance.IceBreak(CentroidWorld, 14);
                    Destroy(iceShell);
                    iceShell = null;
                }
                if (animate && before > 0 && FXManager.Instance != null) { FXManager.Instance.Haptic(); }
                return;
            }
            if (iceShell == null)
            {
                iceShell = new GameObject("IceShell");
                iceShell.transform.SetParent(transform, false);
                iceShell.AddComponent<MeshFilter>();
                iceShell.AddComponent<MeshRenderer>();
            }
            float grow = layers >= 2 ? 1.16f : 1.08f;
            iceShell.GetComponent<MeshFilter>().sharedMesh = Poly(info.cells, cs, grow, height * (layers >= 2 ? 1.5f : 1.28f), -0.02f, 3).ToMesh("Ice");
            iceShell.GetComponent<MeshRenderer>().sharedMaterial = layers >= 2 ? GameMaterials.Ice : GameMaterials.IceCracked;
            if (animate && before > layers)
            {
                if (FXManager.Instance != null) { FXManager.Instance.IceBreak(CentroidWorld, 8); FXManager.Instance.Haptic(); }
                if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.IceCrack);
                StartCoroutine(Pop(1.08f, 0.2f));
            }
        }

        // ------------------------------------------------------------------ motion

        public void SetTarget(Vector3 worldPos) { target = worldPos; }

        public void SetLifted(bool on)
        {
            lift = on ? 0.18f : 0f;
            transform.localScale = on ? Vector3.one * 1.04f : Vector3.one;
        }

        public void Shake() { if (!exiting) StartCoroutine(ShakeRoutine()); }

        private IEnumerator ShakeRoutine()
        {
            float t = 0f;
            while (t < 0.28f)
            {
                t += Time.deltaTime;
                float k = 1f - t / 0.28f;
                shakeOffset = new Vector3(Mathf.Sin(t * 70f) * 0.08f * k, 0f, 0f);
                yield return null;
            }
            shakeOffset = Vector3.zero;
        }

        private IEnumerator Pop(float peak, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float s = 1f + (peak - 1f) * Mathf.Sin(Mathf.Clamp01(t / dur) * Mathf.PI);
                transform.localScale = Vector3.one * s;
                yield return null;
            }
            transform.localScale = Vector3.one;
        }

        /// <summary>A dormant 1x1 companion drops into the slot its peeled parent vacated.</summary>
        public void Appear(Vector3 worldPos)
        {
            gameObject.SetActive(true);
            smoothed = worldPos + new Vector3(0f, 1.2f, 0f);
            target = worldPos;
            transform.position = smoothed;
            StartCoroutine(Pop(1.15f, 0.3f));
        }

        public void SetHidden() { gameObject.SetActive(false); }

        /// <summary>Portal warp: appear instantly at <paramref name="worldPos"/> with a pop.</summary>
        public void TeleportTo(Vector3 worldPos)
        {
            smoothed = worldPos;
            target = worldPos;
            transform.position = worldPos + new Vector3(0f, lift, 0f);
            StartCoroutine(Pop(1.25f, 0.28f));
        }

        /// <summary>Zoom-exit: slides out through the gate while scaling down, then destroys itself.</summary>
        public void PlayExit(Vector3 worldDir, float travel, Action onDone)
        {
            exiting = true;
            SetHint(false);
            StopAllCoroutines();
            shakeOffset = Vector3.zero;
            StartCoroutine(ExitRoutine(worldDir, travel, onDone));
        }

        private IEnumerator ExitRoutine(Vector3 dir, float travel, Action onDone)
        {
            Vector3 start = smoothed;
            float t = 0f, dur = 0.32f;
            while (t < 1f)
            {
                t += Time.deltaTime / dur;
                float e = t * t;
                smoothed = start + dir * (travel * e);
                transform.position = smoothed + new Vector3(0f, lift * (1f - e), 0f);
                transform.localScale = Vector3.one * Mathf.Lerp(1.04f, 0.15f, e);
                yield return null;
            }
            if (onDone != null) onDone();
            Destroy(gameObject);
        }

        // ------------------------------------------------------------------ hint highlight

        /// <summary>Pulsing golden outline + emission glow (rewarded-ad hint).</summary>
        public void SetHint(bool on)
        {
            hint = on;
            if (on && outline == null)
            {
                outline = new GameObject("HintOutline");
                outline.transform.SetParent(transform, false);
                float s = 1.09f;
                outline.transform.localScale = new Vector3(s, 1.12f, s);
                outline.transform.localPosition = new Vector3(centroidLocal.x * (1f - s), -height * 0.06f, centroidLocal.z * (1f - s));
                outline.AddComponent<MeshFilter>().sharedMesh = bodyFilter.sharedMesh;
                outline.AddComponent<MeshRenderer>().sharedMaterial = GameMaterials.HintOutline;
            }
            if (outline != null) outline.SetActive(on);
            if (!on && body != null)
            {
                mpb.SetColor("_EmissionColor", Color.black);
                body.SetPropertyBlock(mpb);
            }
        }

        private void Update()
        {
            if (exiting) return;
            float k = 1f - Mathf.Exp(-26f * Time.deltaTime);
            smoothed = Vector3.Lerp(smoothed, target, k);
            transform.position = smoothed + shakeOffset + new Vector3(0f, lift, 0f);

            if (hint)
            {
                hintPhase += Time.deltaTime * 5f;
                float p = 0.5f + 0.5f * Mathf.Sin(hintPhase);
                mpb.SetColor("_EmissionColor", new Color(1f, 0.72f, 0.12f) * (0.35f + p * 0.9f));
                body.SetPropertyBlock(mpb);
                GameMaterials.HintOutline.SetColor("_BaseColor", new Color(2.2f, 1.6f, 0.25f) * (0.6f + p * 0.8f));
            }
        }
    }
}

namespace ColorGateBlast
{
    /// <summary>Gentle bob + spin so a brass key reads as an interactive pickup.</summary>
    public sealed class KeySpin : MonoBehaviour
    {
        private float y0;
        private void Start() { y0 = transform.localPosition.y; }
        private void Update()
        {
            transform.Rotate(0f, 60f * Time.deltaTime, 0f, Space.Self);
            Vector3 p = transform.localPosition;
            p.y = y0 + Mathf.Sin(Time.time * 3f) * 0.03f;
            transform.localPosition = p;
        }
    }
}
