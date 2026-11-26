using System.Collections.Generic;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>All colours used by the game (zero external art: everything is procedural).</summary>
    public static class ColorPalette
    {
        private static readonly Color[] Blocks =
        {
            new Color(0.93f, 0.29f, 0.31f), // Red
            new Color(0.27f, 0.55f, 0.97f), // Blue
            new Color(0.31f, 0.79f, 0.44f), // Green
            new Color(0.99f, 0.82f, 0.27f), // Yellow
            new Color(0.66f, 0.42f, 0.92f), // Purple
            new Color(1.00f, 0.57f, 0.22f)  // Orange
        };

        public static readonly Color Background = new Color(0.09f, 0.11f, 0.21f);
        public static readonly Color BoardPlate = new Color(0.20f, 0.23f, 0.40f);
        public static readonly Color BoardCell = new Color(0.14f, 0.16f, 0.29f);
        public static readonly Color TrackTile = new Color(0.17f, 0.19f, 0.33f);
        public static readonly Color CorePlate = new Color(0.12f, 0.14f, 0.26f);
        public static readonly Color Mystery = new Color(0.58f, 0.61f, 0.72f);
        public static readonly Color Frost = new Color(0.90f, 0.97f, 1.00f);
        public static readonly Color LockGray = new Color(0.32f, 0.34f, 0.42f);
        public static readonly Color Ink = new Color(0.10f, 0.11f, 0.20f);
        public static readonly Color SlotFree = new Color(0.25f, 0.28f, 0.45f);

        public static Color Get(BlockColor c) { return Blocks[(int)c % Blocks.Length]; }
        public static Color Get(int c) { return Blocks[c % Blocks.Length]; }

        public static Color Lighten(Color c, float t) { return Color.Lerp(c, Color.white, t); }
        public static Color Darken(Color c, float t) { return Color.Lerp(c, Color.black, t); }
    }

    /// <summary>Material cache. Templates (assigned by GameManager) guarantee the URP shaders survive build stripping.</summary>
    public static class Mats
    {
        private static Material litTemplate;
        private static Material particleTemplate;
        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();
        private static Font font;

        public static void Init(Material lit, Material particle)
        {
            litTemplate = lit;
            particleTemplate = particle;
            Cache.Clear();
        }

        public static Material Lit(Color c, float smoothness = 0.12f)
        {
            string key = c.r.ToString("F3") + c.g.ToString("F3") + c.b.ToString("F3") + smoothness.ToString("F2");
            Material m;
            if (Cache.TryGetValue(key, out m) && m != null) return m;

            if (litTemplate != null) m = new Material(litTemplate);
            else m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.name = "Toy_" + key;
            m.SetColor("_BaseColor", c);
            m.SetColor("_Color", c);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
            Cache[key] = m;
            return m;
        }

        public static Material Particle()
        {
            if (particleTemplate != null) return particleTemplate;
            return new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        }

        public static Font DefaultFont()
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }

        /// <summary>Creates a world-space label (legacy TextMesh). <paramref name="height"/> is the approximate glyph height in world units.</summary>
        public static TextMesh Label(Transform parent, string text, float height, Color color, Vector3 localPos)
        {
            GameObject go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            TextMesh tm = go.AddComponent<TextMesh>();
            tm.font = DefaultFont();
            tm.fontSize = 64;
            tm.characterSize = height * 10f / 64f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;
            tm.color = color;
            tm.text = text;
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr == null) mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = tm.font.material;
            mr.sortingOrder = 20;
            return tm;
        }
    }

    /// <summary>
    /// Appends rounded, beveled "toy" slabs to a single mesh. A slab spans z in [-depth, 0]; its top face looks
    /// toward the camera (which sits at negative z). Winding is verified per triangle against the slab centre,
    /// so every face points outward regardless of vertex order.
    /// </summary>
    public sealed class MeshBuilder
    {
        private readonly List<Vector3> v = new List<Vector3>();
        private readonly List<int> t = new List<int>();

        public void AddSlab(Vector2 c, float w, float h, float depth, float radius, float bevel, int seg = 3)
        {
            if (seg < 1) seg = 1;
            float hx = w * 0.5f, hy = h * 0.5f;
            radius = Mathf.Clamp(radius, 0.002f, Mathf.Min(hx, hy));
            bevel = Mathf.Clamp(bevel, 0f, Mathf.Min(depth * 0.9f, radius * 0.9f));
            int n = (seg + 1) * 4;
            int baseIndex = v.Count;

            for (int ring = 0; ring < 3; ring++)
            {
                float inset = ring == 2 ? bevel : 0f;
                float z = ring == 0 ? 0f : (ring == 1 ? -(depth - bevel) : -depth);
                float rad = Mathf.Max(radius - inset, 0.001f);
                float ihx = hx - inset, ihy = hy - inset;
                for (int k = 0; k < 4; k++)
                {
                    float sx = (k == 0 || k == 3) ? 1f : -1f;
                    float sy = (k <= 1) ? 1f : -1f;
                    float ccx = sx * (ihx - rad), ccy = sy * (ihy - rad);
                    for (int s = 0; s <= seg; s++)
                    {
                        float a = (k * 90f + 90f * s / seg) * Mathf.Deg2Rad;
                        v.Add(new Vector3(c.x + ccx + Mathf.Cos(a) * rad, c.y + ccy + Mathf.Sin(a) * rad, z));
                    }
                }
            }
            int centerIdx = v.Count;
            v.Add(new Vector3(c.x, c.y, -depth));

            Vector3 reference = new Vector3(c.x, c.y, -depth * 0.5f);
            for (int ring = 0; ring < 2; ring++)
            {
                int a0 = baseIndex + ring * n;
                int b0 = baseIndex + (ring + 1) * n;
                for (int i = 0; i < n; i++)
                {
                    int i2 = (i + 1) % n;
                    AddTri(a0 + i, a0 + i2, b0 + i2, reference);
                    AddTri(a0 + i, b0 + i2, b0 + i, reference);
                }
            }
            int top = baseIndex + 2 * n;
            for (int i = 0; i < n; i++) AddTri(centerIdx, top + i, top + (i + 1) % n, reference);
        }

        /// <summary>Flat triangle at the given points, facing the camera (-z).</summary>
        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(c);
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (n.z > 0f) { t.Add(i); t.Add(i + 2); t.Add(i + 1); }
            else { t.Add(i); t.Add(i + 1); t.Add(i + 2); }
        }

        private void AddTri(int i0, int i1, int i2, Vector3 reference)
        {
            Vector3 p0 = v[i0], p1 = v[i1], p2 = v[i2];
            Vector3 n = Vector3.Cross(p1 - p0, p2 - p0);
            Vector3 centroid = (p0 + p1 + p2) / 3f;
            if (Vector3.Dot(n, centroid - reference) < 0f)
            {
                int tmp = i1; i1 = i2; i2 = tmp;
            }
            t.Add(i0); t.Add(i1); t.Add(i2);
        }

        public Mesh ToMesh(string name)
        {
            Mesh m = new Mesh();
            m.name = name;
            m.SetVertices(v);
            m.SetTriangles(t, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }

    public static class ProceduralMeshes
    {
        public static Mesh Slab(float w, float h, float depth, float radius, float bevel, string name = "Slab")
        {
            MeshBuilder b = new MeshBuilder();
            b.AddSlab(Vector2.zero, w, h, depth, radius, bevel);
            return b.ToMesh(name);
        }

        /// <summary>
        /// One mesh for a whole polyomino. Cell slabs are joined by "bridge" slabs so the shape reads as a single
        /// toy piece. Local origin = centre of the shape's (0,0) cell.
        /// </summary>
        public static Mesh Polyomino(Int2[] cells, float cs, float depth, string name = "Block")
        {
            float size = cs * 0.9f;
            float radius = size * 0.28f;
            float bevel = size * 0.11f;
            MeshBuilder b = new MeshBuilder();
            for (int i = 0; i < cells.Length; i++)
                b.AddSlab(new Vector2(cells[i].x * cs, cells[i].y * cs), size, size, depth, radius, bevel);

            for (int i = 0; i < cells.Length; i++)
            {
                for (int j = 0; j < cells.Length; j++)
                {
                    if (cells[j].y == cells[i].y && cells[j].x == cells[i].x + 1)
                        b.AddSlab(new Vector2((cells[i].x + 0.5f) * cs, cells[i].y * cs), cs, size, depth - 0.004f, radius, bevel);
                    if (cells[j].x == cells[i].x && cells[j].y == cells[i].y + 1)
                        b.AddSlab(new Vector2(cells[i].x * cs, (cells[i].y + 0.5f) * cs), size, cs, depth - 0.004f, radius, bevel);
                }
            }
            return b.ToMesh(name);
        }

        /// <summary>Centroid of the cells in the block's local space.</summary>
        public static Vector3 Centroid(Int2[] cells, float cs)
        {
            float x = 0, y = 0;
            for (int i = 0; i < cells.Length; i++) { x += cells[i].x; y += cells[i].y; }
            return new Vector3(x / cells.Length * cs, y / cells.Length * cs, 0f);
        }
    }
}
