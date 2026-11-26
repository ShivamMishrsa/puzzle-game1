using System.Collections;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// The shield-brick picture in the middle of the conveyor loop. Wraps <see cref="CoreModel"/>
    /// (which decides what can be hit) and animates projectiles and brick layers.
    /// </summary>
    public sealed class TargetCoreManager : MonoBehaviour
    {
        private sealed class BrickView
        {
            public GameObject[] layers;
        }

        public CoreModel Model { get; private set; }
        /// <summary>Projectiles currently flying (their hits are already applied to the model).</summary>
        public int InFlight { get; private set; }
        public bool IsCleared { get { return Model != null && Model.RemainingLayers == 0; } }

        private Transform root;
        private BrickView[] bricks;
        private Vector2 center;
        private float cellSize;
        private int cw, ch;
        private Mesh brickMesh;
        private Mesh pipMesh;
        private Mesh shotMesh;

        public Vector3 CenterWorld { get { return new Vector3(center.x, center.y, 0f); } }

        public Vector3 CellWorld(int x, int y)
        {
            return new Vector3(center.x + (x - (cw - 1) * 0.5f) * cellSize, center.y + (y - (ch - 1) * 0.5f) * cellSize, 0f);
        }

        public void Clear()
        {
            StopAllCoroutines();
            InFlight = 0;
            if (root != null) Destroy(root.gameObject);
            root = null;
            bricks = null;
            Model = null;
        }

        public void Build(LevelDefinition def, Vector2 centerPos, float cell)
        {
            Clear();
            center = centerPos;
            cellSize = cell;
            cw = def.coreWidth;
            ch = def.coreHeight;
            Model = new CoreModel(def);

            root = new GameObject("CoreRoot").transform;
            root.SetParent(transform, false);

            float depth = cell * 0.30f;
            float size = cell * 0.9f;
            brickMesh = ProceduralMeshes.Slab(size, size, depth, size * 0.26f, size * 0.1f, "Brick");
            pipMesh = ProceduralMeshes.Slab(size * 0.34f, size * 0.34f, 0.04f, size * 0.17f, size * 0.04f, "Pip");
            shotMesh = ProceduralMeshes.Slab(cell * 0.28f, cell * 0.28f, cell * 0.18f, cell * 0.14f, cell * 0.04f, "Shot");

            // recessed backdrop behind the bricks
            GameObject plate = new GameObject("CorePlate");
            plate.transform.SetParent(root, false);
            plate.transform.position = new Vector3(center.x, center.y, 0.6f);
            plate.AddComponent<MeshFilter>().sharedMesh = ProceduralMeshes.Slab((cw + 0.35f) * cell, (ch + 0.35f) * cell, 0.1f, cell * 0.35f, cell * 0.08f, "CorePlate");
            plate.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.CorePlate, 0.05f);

            bricks = new BrickView[Model.Bricks.Length];
            for (int b = 0; b < Model.Bricks.Length; b++)
            {
                BrickSpec spec = Model.Bricks[b];
                BrickView bv = new BrickView();
                bv.layers = new GameObject[spec.layers.Length];
                Vector3 pos = CellWorld(spec.x, spec.y);
                for (int k = 0; k < spec.layers.Length; k++)
                {
                    GameObject go = new GameObject("Brick_" + b + "_L" + k);
                    go.transform.SetParent(root, false);
                    go.transform.position = pos + new Vector3(0, 0, 0.3f + k * 0.18f);
                    go.AddComponent<MeshFilter>().sharedMesh = brickMesh;
                    go.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.Get(spec.layers[k]), 0.12f);
                    bv.layers[k] = go;

                    // A pip on the outer layer hints at the colour hiding underneath.
                    if (k == 0 && spec.layers.Length > 1)
                    {
                        GameObject pip = new GameObject("Pip");
                        pip.transform.SetParent(go.transform, false);
                        pip.transform.localPosition = new Vector3(0, 0, -depth - 0.01f);
                        pip.AddComponent<MeshFilter>().sharedMesh = pipMesh;
                        pip.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.Get(spec.layers[1]), 0.2f);
                    }
                }
                bricks[b] = bv;
            }
        }

        /// <summary>
        /// A turret of <paramref name="color"/> standing at (<paramref name="side"/>, <paramref name="lane"/>) tries to shoot.
        /// Returns true when a projectile was launched (the hit is applied to the model immediately).
        /// </summary>
        public bool TryFire(int color, Side side, int lane, Vector3 from)
        {
            if (Model == null) return false;
            int b = Model.FindHitAt(color, side, lane);
            if (b < 0) return false;

            int layer = Model.Layer[b];
            Model.Hit(b);
            StartCoroutine(Fly(from, b, layer, color));
            return true;
        }

        private IEnumerator Fly(Vector3 from, int brick, int layer, int color)
        {
            InFlight++;
            GameObject shot = new GameObject("Shot");
            shot.transform.SetParent(root, false);
            shot.AddComponent<MeshFilter>().sharedMesh = shotMesh;
            shot.AddComponent<MeshRenderer>().sharedMaterial = Mats.Lit(ColorPalette.Lighten(ColorPalette.Get(color), 0.25f), 0.4f);

            BrickSpec spec = Model.Bricks[brick];
            Vector3 to = CellWorld(spec.x, spec.y) + new Vector3(0, 0, -0.1f);
            from.z = -0.1f;
            float dist = Vector3.Distance(from, to);
            float dur = Mathf.Clamp(dist / 22f, 0.07f, 0.22f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / dur;
                shot.transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(t));
                yield return null;
            }
            Destroy(shot);

            BrickView bv = bricks[brick];
            GameObject hitLayer = bv.layers[layer];
            Vector3 hitPos = hitLayer.transform.position;
            Destroy(hitLayer);
            if (JuiceManager.Instance != null)
            {
                JuiceManager.Instance.BrickHit(hitPos, ColorPalette.Get(color));
                if (layer + 1 < bv.layers.Length) JuiceManager.Instance.Pop(bv.layers[layer + 1].transform, 1.15f, 0.22f);
            }
            InFlight--;
        }
    }
}
