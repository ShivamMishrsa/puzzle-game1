using System.Collections.Generic;
using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// Procedural geometry for the workshop: rounded, beveled slabs, rivets (spheres), boxes and flat triangles in a
    /// right-handed "table" space: x right, y UP, z away from the player. A slab sits on y in [y0, y0 + height].
    /// Winding is verified against the primitive's centre, so faces always point outward.
    /// </summary>
    public sealed class MeshBuilder
    {
        private readonly List<Vector3> v = new List<Vector3>();
        private readonly List<int> t = new List<int>();

        public int VertexCount { get { return v.Count; } }

        /// <summary>Rounded slab: centre (x, z), size w (x) by d (z), extruded up by height.</summary>
        public void AddSlab(float cx, float cz, float w, float d, float height, float radius, float bevel, float y0, int seg)
        {
            if (seg < 1) seg = 1;
            float hx = w * 0.5f, hz = d * 0.5f;
            radius = Mathf.Clamp(radius, 0.002f, Mathf.Min(hx, hz));
            bevel = Mathf.Clamp(bevel, 0f, Mathf.Min(height * 0.9f, radius * 0.9f));
            int n = (seg + 1) * 4;
            int baseIndex = v.Count;

            for (int ring = 0; ring < 3; ring++)
            {
                float inset = ring == 2 ? bevel : 0f;
                float y = ring == 0 ? y0 : (ring == 1 ? y0 + height - bevel : y0 + height);
                float rad = Mathf.Max(radius - inset, 0.001f);
                float ihx = hx - inset, ihz = hz - inset;
                for (int k = 0; k < 4; k++)
                {
                    float sx = (k == 0 || k == 3) ? 1f : -1f;
                    float sz = (k <= 1) ? 1f : -1f;
                    float ccx = sx * (ihx - rad), ccz = sz * (ihz - rad);
                    for (int s = 0; s <= seg; s++)
                    {
                        float a = (k * 90f + 90f * s / seg) * Mathf.Deg2Rad;
                        v.Add(new Vector3(cx + ccx + Mathf.Cos(a) * rad, y, cz + ccz + Mathf.Sin(a) * rad));
                    }
                }
            }
            int centerIdx = v.Count;
            v.Add(new Vector3(cx, y0 + height, cz));

            Vector3 reference = new Vector3(cx, y0 + height * 0.5f, cz);
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

        /// <summary>Low-poly sphere (rivets, bolts, bearings).</summary>
        public void AddSphere(Vector3 c, float r, int lon, int lat)
        {
            if (lon < 3) lon = 3;
            if (lat < 2) lat = 2;
            int baseIndex = v.Count;
            for (int i = 0; i <= lat; i++)
            {
                float phi = Mathf.PI * i / lat;
                for (int j = 0; j < lon; j++)
                {
                    float th = 2f * Mathf.PI * j / lon;
                    v.Add(c + new Vector3(Mathf.Sin(phi) * Mathf.Cos(th), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(th)) * r);
                }
            }
            for (int i = 0; i < lat; i++)
            {
                for (int j = 0; j < lon; j++)
                {
                    int j2 = (j + 1) % lon;
                    int a = baseIndex + i * lon + j, b = baseIndex + i * lon + j2;
                    int c2 = baseIndex + (i + 1) * lon + j2, d = baseIndex + (i + 1) * lon + j;
                    AddTri(a, b, c2, c);
                    AddTri(a, c2, d, c);
                }
            }
        }

        /// <summary>Axis-aligned box.</summary>
        public void AddBox(Vector3 c, Vector3 size)
        {
            Vector3 h = size * 0.5f;
            Vector3[] p = new Vector3[8];
            for (int i = 0; i < 8; i++)
                p[i] = c + new Vector3((i & 1) == 0 ? -h.x : h.x, (i & 2) == 0 ? -h.y : h.y, (i & 4) == 0 ? -h.z : h.z);
            AddQuad(p[0], p[1], p[3], p[2], new Vector3(0, 0, -1));
            AddQuad(p[4], p[5], p[7], p[6], new Vector3(0, 0, 1));
            AddQuad(p[0], p[2], p[6], p[4], new Vector3(-1, 0, 0));
            AddQuad(p[1], p[3], p[7], p[5], new Vector3(1, 0, 0));
            AddQuad(p[2], p[3], p[7], p[6], new Vector3(0, 1, 0));
            AddQuad(p[0], p[1], p[5], p[4], new Vector3(0, -1, 0));
        }

        /// <summary>Quad (a,b,c,d) lit from the side given by <paramref name="facing"/>.</summary>
        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 facing)
        {
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(c); v.Add(d);
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(n, facing) < 0f)
            {
                t.Add(i); t.Add(i + 2); t.Add(i + 1);
                t.Add(i); t.Add(i + 3); t.Add(i + 2);
            }
            else
            {
                t.Add(i); t.Add(i + 1); t.Add(i + 2);
                t.Add(i); t.Add(i + 2); t.Add(i + 3);
            }
        }

        /// <summary>Flat triangle that faces up (+y).</summary>
        public void AddTriangleUp(Vector3 a, Vector3 b, Vector3 c)
        {
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(c);
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (n.y < 0f) { t.Add(i); t.Add(i + 2); t.Add(i + 1); }
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

        /// <summary>Builds the mesh. UVs are a planar top-down projection (x, z) * uvScale, enough for the procedural textures.</summary>
        public Mesh ToMesh(string name, float uvScale)
        {
            Mesh m = new Mesh();
            m.name = name;
            Vector2[] uv = new Vector2[v.Count];
            for (int i = 0; i < uv.Length; i++) uv[i] = new Vector2(v[i].x * uvScale, v[i].z * uvScale);
            m.SetVertices(v);
            m.SetTriangles(t, 0);
            m.uv = uv;
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        public Mesh ToMesh(string name) { return ToMesh(name, 0.5f); }
    }

    /// <summary>Cached built-in primitive meshes (no colliders attached).</summary>
    public static class Prims
    {
        private static Mesh cube, sphere, cylinder, capsule;

        public static Mesh Cube { get { if (cube == null) cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx"); return cube; } }
        public static Mesh Sphere { get { if (sphere == null) sphere = Resources.GetBuiltinResource<Mesh>("Sphere.fbx"); return sphere; } }
        public static Mesh Cylinder { get { if (cylinder == null) cylinder = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx"); return cylinder; } }
        public static Mesh Capsule { get { if (capsule == null) capsule = Resources.GetBuiltinResource<Mesh>("Capsule.fbx"); return capsule; } }

        /// <summary>Creates a child object showing a mesh with a material (no collider).</summary>
        public static GameObject Make(string name, Transform parent, Mesh mesh, Material mat, Vector3 localPos, Vector3 localScale)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        public static GameObject Make(string name, Transform parent, Mesh mesh, Material mat, Vector3 localPos, Vector3 localScale, Vector3 eulerAngles)
        {
            GameObject go = Make(name, parent, mesh, mat, localPos, localScale);
            go.transform.localEulerAngles = eulerAngles;
            return go;
        }

        /// <summary>World-space label (legacy TextMesh). height = approximate glyph height in world units.</summary>
        public static TextMesh Label(Transform parent, string text, float height, Color color, Vector3 localPos)
        {
            GameObject go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            TextMesh tm = go.AddComponent<TextMesh>();
            tm.font = GameMaterials.DefaultFont();
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
            return tm;
        }
    }
}
