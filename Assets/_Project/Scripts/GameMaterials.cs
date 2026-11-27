using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ColorGateBlast
{
    /// <summary>
    /// Automatic procedural URP Lit material generator: no material or texture asset files are needed.
    /// Everything (metal scratches, rust speckle, wood grain, ice cracks, hazard stripes) is generated at startup
    /// and cached. Required shaders are added to "Always Included Shaders" by the Setup tool so builds keep them.
    /// </summary>
    public static class GameMaterials
    {
        private static bool ready;
        private static Shader lit, unlit, particleLit, particleUnlit;
        private static Font font;
        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();
        private static Texture2D scratchNormal, rustTex, woodTex, woodNormal, noiseTex, crackTex, hazardTex, beltTex, dashTex;

        // ---------------------------------------------------------------- named materials from the spec
        public static Material MetalRed { get { Ensure(); return Named("MetalRed", ColorOf(BlockColor.Red), 0.45f, 0.58f, TexKind.Scratch); } }
        public static Material MetalBlue { get { Ensure(); return Named("MetalBlue", ColorOf(BlockColor.Blue), 0.45f, 0.58f, TexKind.Scratch); } }
        /// <summary>Emerald Enamel Green #10B981: baked-enamel metal (metallic 0.75, smoothness 0.68).</summary>
        public static Material EmeraldEnamel { get { Ensure(); return Named("EmeraldEnamel", ColorOf(BlockColor.Green), 0.45f, 0.58f, TexKind.Scratch); } }
        public static Material PlasticYellow { get { Ensure(); return Named("PlasticYellow", ColorOf(BlockColor.Yellow), 0.45f, 0.58f, TexKind.Scratch); } }
        public static Material ScrapIron { get { Ensure(); return Named("ScrapIron", new Color(0.353f, 0.345f, 0.337f), 0.85f, 0.25f, TexKind.Rust); } }
        public static Material WorkshopWood { get { Ensure(); return Named("WorkshopWood", new Color(0.82f, 0.76f, 0.72f), 0.02f, 0.45f, TexKind.Wood); } }
        public static Material MetallicBelt { get { Ensure(); return Named("MetallicBelt", Color.white, 0.8f, 0.45f, TexKind.Belt); } }
        public static Material Glass { get { Ensure(); return Transparent("Glass", new Color(0.8f, 0.93f, 1f, 0.22f), 0.05f, 0.97f); } }
        // ---------------------------------------------------------------- extras
        public static Material Copper { get { Ensure(); return Plain("Copper", new Color(0.78f, 0.45f, 0.22f), 0.92f, 0.55f); } }
        public static Material Brass { get { Ensure(); return Plain("Brass", new Color(0.9f, 0.7f, 0.28f), 0.9f, 0.62f); } }
        /// <summary>Satin silver for big flat casings (no hot specular spots).</summary>
        public static Material Silver { get { Ensure(); return Plain("Silver", new Color(0.52f, 0.54f, 0.58f), 0.45f, 0.22f); } }
        public static Material Steel { get { Ensure(); return Plain("Steel", new Color(0.6f, 0.62f, 0.66f), 0.85f, 0.42f); } }
        public static Material Charcoal { get { Ensure(); return Plain("Charcoal", new Color(0.13f, 0.14f, 0.16f), 0.05f, 0.4f); } }
        public static Material Cream { get { Ensure(); return Plain("Cream", new Color(0.96f, 0.93f, 0.84f), 0.02f, 0.5f); } }
        public static Material Rubber { get { Ensure(); return Plain("Rubber", new Color(0.07f, 0.07f, 0.09f), 0.0f, 0.15f); } }
        public static Material Gunmetal { get { Ensure(); return Plain("Gunmetal", new Color(0.22f, 0.25f, 0.3f), 0.85f, 0.4f); } }
        public static Material PcbGreen { get { Ensure(); return Plain("PcbGreen", new Color(0.1f, 0.5f, 0.25f), 0.2f, 0.6f); } }
        public static Material TapeBrown { get { Ensure(); return Plain("TapeBrown", new Color(0.22f, 0.12f, 0.07f), 0.1f, 0.5f); } }
        public static Material TntRed { get { Ensure(); return Emissive("TntRed", new Color(0.75f, 0.08f, 0.05f), new Color(1f, 0.15f, 0.05f), 1.6f); } }
        public static Material Hazard { get { Ensure(); return Named("Hazard", Color.white, 0.7f, 0.4f, TexKind.Hazard); } }
        /// <summary>Frosted ice shell: pale cyan #B8EEFF, alpha 0.55, smoothness 0.92.</summary>
        public static Material Ice { get { Ensure(); return Transparent("Ice", new Color(0.722f, 0.933f, 1f, 0.55f), 0f, 0.92f); } }
        /// <summary>Same shell with procedural fracture lines (shown when one layer is left).</summary>
        public static Material IceCracked
        {
            get
            {
                Ensure();
                Material m = Transparent("IceCracked", new Color(1f, 1f, 1f, 1f), 0f, 0.92f);
                if (m.GetTexture("_BaseMap") == null) { m.SetTexture("_BaseMap", crackTex); m.SetTextureScale("_BaseMap", new Vector2(1f, 1f)); }
                return m;
            }
        }
        // ---- chapters 3 / 4 ----
        public static Material Bakelite { get { Ensure(); return Plain("Bakelite", new Color(0.09f, 0.05f, 0.05f), 0.15f, 0.82f); } }
        public static Material Gold { get { Ensure(); return Plain("Gold", new Color(1.0f, 0.78f, 0.28f), 0.95f, 0.7f); } }
        public static Material Porcelain { get { Ensure(); return Plain("Porcelain", new Color(0.97f, 0.97f, 0.95f), 0.0f, 0.9f); } }
        public static Material RubyJewel { get { Ensure(); return Emissive("RubyJewel", new Color(0.55f, 0.02f, 0.08f), new Color(1f, 0.1f, 0.2f), 1.2f); } }
        public static Material PortalCyan { get { Ensure(); return Emissive("PortalCyan", new Color(0.05f, 0.4f, 0.5f), new Color(0.2f, 0.95f, 1f), 2.4f); } }
        public static Material PortalViolet { get { Ensure(); return Emissive("PortalViolet", new Color(0.3f, 0.1f, 0.5f), new Color(0.75f, 0.35f, 1f), 2.4f); } }
        public static Material PortalAmber { get { Ensure(); return Emissive("PortalAmber", new Color(0.5f, 0.28f, 0.02f), new Color(1f, 0.6f, 0.05f), 2.4f); } }
        public static Material LedGreen { get { Ensure(); return Emissive("LedGreen", new Color(0.05f, 0.4f, 0.1f), new Color(0.2f, 1f, 0.3f), 2.2f); } }
        public static Material LedAmber { get { Ensure(); return Emissive("LedAmber", new Color(0.5f, 0.28f, 0.02f), new Color(1f, 0.6f, 0.05f), 2.2f); } }

        /// <summary>Block body material for a colour (Gray = scrap iron).</summary>
        public static Material ForBlock(BlockColor c)
        {
            switch (c)
            {
                case BlockColor.Red: return MetalRed;
                case BlockColor.Blue: return MetalBlue;
                case BlockColor.Yellow: return PlasticYellow;
                case BlockColor.Green: return EmeraldEnamel;
                default: return ScrapIron;
            }
        }

        public static Color ColorOf(BlockColor c)
        {
            switch (c)
            {
                case BlockColor.Red: return new Color(0.722f, 0.2f, 0.165f);      // #B8332A
                case BlockColor.Blue: return new Color(0.18f, 0.392f, 0.62f);     // #2E649E
                case BlockColor.Yellow: return new Color(0.788f, 0.635f, 0.212f); // #C9A236
                case BlockColor.Green: return new Color(0.118f, 0.51f, 0.361f);   // #1E825C
                default: return new Color(0.353f, 0.345f, 0.337f);                // scrap iron #5A5856
            }
        }

        /// <summary>Glowing emissive material in the colour of a gate / turret / part marker.</summary>
        public static Material Glow(BlockColor c)
        {
            Ensure();
            Color col = ColorOf(c);
            return Emissive("Glow_" + c, col * 0.6f, col, 1.8f);
        }

        /// <summary>Saturated neon tone of a block colour, used for gate chevrons, the floor arrow and lights.</summary>
        public static Color NeonOf(BlockColor c)
        {
            switch (c)
            {
                case BlockColor.Red: return new Color(1f, 0.22f, 0.18f);
                case BlockColor.Blue: return new Color(0.2f, 0.55f, 1f);
                case BlockColor.Yellow: return new Color(1f, 0.85f, 0.2f);
                case BlockColor.Green: return new Color(0.15f, 1f, 0.55f);
                default: return new Color(0.7f, 0.7f, 0.75f);
            }
        }

        /// <summary>Glowing neon chevron material (bloom picks it up).</summary>
        public static Material Neon(BlockColor c)
        {
            Ensure();
            Color col = NeonOf(c);
            return Emissive("Neon_" + c, col * 0.5f, col, 2.6f);
        }

        /// <summary>Gunmetal riveted tray frame.</summary>
        public static Material TrayFrame { get { Ensure(); return Named("TrayFrame", new Color(0.33f, 0.35f, 0.39f), 0.85f, 0.42f, TexKind.Scratch); } }
        /// <summary>Recessed dark-grey tray tiles.</summary>
        public static Material TrayTile { get { Ensure(); return Named("TrayTile", new Color(0.13f, 0.14f, 0.16f), 0.55f, 0.3f, TexKind.Scratch); } }
        /// <summary>Flat brushed-steel belt with a white dashed centre line (scroll via ScrollBelt).</summary>
        public static Material BeltDashed
        {
            get
            {
                Ensure();
                Material m;
                if (Cache.TryGetValue("BeltDashed", out m) && m != null) return m;
                m = NewLit("BeltDashed", new Color(0.8f, 0.8f, 0.82f), 0.7f, 0.3f);
                m.SetTexture("_BaseMap", dashTex);
                m.SetTexture("_BumpMap", scratchNormal);
                m.SetFloat("_BumpScale", 0.4f);
                m.EnableKeyword("_NORMALMAP");
                Cache["BeltDashed"] = m;
                return m;
            }
        }

        /// <summary>Slightly darker copy of a block colour: the recessed square top panel.</summary>
        public static Material PanelOf(BlockColor c)
        {
            Ensure();
            string key = "Panel_" + c;
            Material m;
            if (Cache.TryGetValue(key, out m) && m != null) return m;
            Material b = ForBlock(c);
            m = new Material(b);
            m.name = key;
            Color col = b.GetColor("_BaseColor");
            m.SetColor("_BaseColor", new Color(col.r * 0.72f, col.g * 0.72f, col.b * 0.72f, 1f));
            m.SetFloat("_Smoothness", 0.4f);
            Cache[key] = m;
            return m;
        }

        /// <summary>Tinted translucent acrylic (cassette door).</summary>
        public static Material Acrylic(BlockColor c)
        {
            Ensure();
            Color col = ColorOf(c);
            return Transparent("Acrylic_" + c, new Color(col.r, col.g, col.b, 0.42f), 0f, 0.95f);
        }

        // ---------------------------------------------------------------- unlit / particle materials
        public static Material UnlitGlow(Color c, float intensity)
        {
            Ensure();
            string key = "Unlit_" + c.r.ToString("F2") + c.g.ToString("F2") + c.b.ToString("F2") + intensity.ToString("F1");
            Material m;
            if (Cache.TryGetValue(key, out m) && m != null) return m;
            m = new Material(unlit);
            m.name = key;
            m.SetColor("_BaseColor", new Color(c.r * intensity, c.g * intensity, c.b * intensity, 1f));
            Cache[key] = m;
            return m;
        }

        /// <summary>Gold, front-face-culled hull used as the pulsing hint outline.</summary>
        public static Material HintOutline
        {
            get
            {
                Ensure();
                Material m;
                if (Cache.TryGetValue("HintOutline", out m) && m != null) return m;
                m = new Material(unlit);
                m.name = "HintOutline";
                m.SetColor("_BaseColor", new Color(2.2f, 1.6f, 0.25f, 1f));
                m.SetFloat("_Cull", 1f); // cull FRONT faces -> only the rim of the inflated hull shows
                Cache["HintOutline"] = m;
                return m;
            }
        }

        public static Material ParticleLit()
        {
            Ensure();
            Material m;
            if (Cache.TryGetValue("ParticleLit", out m) && m != null) return m;
            m = new Material(particleLit != null ? particleLit : lit);
            m.name = "ParticleLit";
            Cache["ParticleLit"] = m;
            return m;
        }

        public static Material ParticleUnlit(bool transparent)
        {
            Ensure();
            string key = transparent ? "ParticleUnlitT" : "ParticleUnlit";
            Material m;
            if (Cache.TryGetValue(key, out m) && m != null) return m;
            m = new Material(particleUnlit != null ? particleUnlit : unlit);
            m.name = key;
            if (transparent) MakeTransparent(m);
            Cache[key] = m;
            return m;
        }

        public static Font DefaultFont()
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }

        // ---------------------------------------------------------------- construction
        private enum TexKind { None, Scratch, Noise, Rust, Wood, Belt, Hazard }

        private static void Ensure()
        {
            if (ready && lit != null) return;
            lit = Shader.Find("Universal Render Pipeline/Lit");
            unlit = Shader.Find("Universal Render Pipeline/Unlit");
            particleLit = Shader.Find("Universal Render Pipeline/Particles/Lit");
            particleUnlit = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (lit == null) lit = Shader.Find("Standard");
            if (unlit == null) unlit = lit;
            Cache.Clear();
            scratchNormal = MakeScratchNormal(128, 11);
            rustTex = MakeRust(128, 5);
            woodTex = MakeWood(256, 3);
            noiseTex = MakeNoise(64, 9, new Color(0.93f, 0.93f, 0.93f), 0.07f);
            crackTex = MakeCracks(256, 21);
            hazardTex = MakeHazard(128);
            beltTex = MakeBelt(128, 7);
            woodNormal = MakeWoodNormal(256, 3);
            dashTex = MakeDash(64);
            ready = true;
        }

        private static Material NewLit(string name, Color c, float metallic, float smooth)
        {
            Material m = new Material(lit);
            m.name = name;
            m.SetColor("_BaseColor", c);
            m.SetColor("_Color", c);
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_Glossiness", smooth);
            m.EnableKeyword("_EMISSION");
            return m;
        }

        private static Material Plain(string name, Color c, float metallic, float smooth)
        {
            Material m;
            if (Cache.TryGetValue(name, out m) && m != null) return m;
            m = NewLit(name, c, metallic, smooth);
            Cache[name] = m;
            return m;
        }

        private static Material Named(string name, Color c, float metallic, float smooth, TexKind kind)
        {
            Material m;
            if (Cache.TryGetValue(name, out m) && m != null) return m;
            m = NewLit(name, c, metallic, smooth);
            switch (kind)
            {
                case TexKind.Scratch:
                    m.SetTexture("_BumpMap", scratchNormal);
                    m.SetFloat("_BumpScale", 0.55f);
                    m.EnableKeyword("_NORMALMAP");
                    break;
                case TexKind.Noise:
                    m.SetTexture("_BaseMap", noiseTex);
                    m.SetTexture("_BumpMap", scratchNormal);
                    m.SetFloat("_BumpScale", 0.15f);
                    m.EnableKeyword("_NORMALMAP");
                    break;
                case TexKind.Rust:
                    m.SetTexture("_BaseMap", rustTex);
                    m.SetTexture("_BumpMap", scratchNormal);
                    m.SetFloat("_BumpScale", 0.9f);
                    m.EnableKeyword("_NORMALMAP");
                    break;
                case TexKind.Wood:
                    m.SetTexture("_BaseMap", woodTex);
                    m.SetTextureScale("_BaseMap", new Vector2(2f, 2f));
                    m.SetTexture("_BumpMap", woodNormal);
                    m.SetTextureScale("_BumpMap", new Vector2(2f, 2f));
                    m.SetFloat("_BumpScale", 0.7f);
                    m.EnableKeyword("_NORMALMAP");
                    break;
                case TexKind.Belt:
                    m.SetTexture("_BaseMap", beltTex);
                    m.SetTexture("_BumpMap", scratchNormal);
                    m.SetFloat("_BumpScale", 0.7f);
                    m.EnableKeyword("_NORMALMAP");
                    break;
                case TexKind.Hazard:
                    m.SetTexture("_BaseMap", hazardTex);
                    break;
            }
            Cache[name] = m;
            return m;
        }

        private static Material Emissive(string name, Color baseColor, Color emission, float intensity)
        {
            Material m;
            if (Cache.TryGetValue(name, out m) && m != null) return m;
            m = NewLit(name, baseColor, 0.3f, 0.5f);
            m.SetColor("_EmissionColor", new Color(emission.r * intensity, emission.g * intensity, emission.b * intensity, 1f));
            Cache[name] = m;
            return m;
        }

        private static Material Transparent(string name, Color c, float metallic, float smooth)
        {
            Material m;
            if (Cache.TryGetValue(name, out m) && m != null) return m;
            m = NewLit(name, c, metallic, smooth);
            MakeTransparent(m);
            Cache[name] = m;
            return m;
        }

        /// <summary>Standard URP recipe for alpha-blended surfaces set up from script.</summary>
        public static void MakeTransparent(Material m)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.DisableKeyword("_ALPHATEST_ON");
            m.renderQueue = (int)RenderQueue.Transparent;
        }

        // ---------------------------------------------------------------- procedural textures
        private static Texture2D NewTex(int size, bool linear)
        {
            Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, true, linear);
            t.wrapMode = TextureWrapMode.Repeat;
            t.filterMode = FilterMode.Bilinear;
            return t;
        }

        private static float Perlin(float x, float y, float seed) { return Mathf.PerlinNoise(x + seed * 17.3f, y + seed * 9.1f); }

        /// <summary>Micro-scratches: thin random lines + fine noise, converted to a tangent-space normal map (R,G = XY, A = 1).</summary>
        private static Texture2D MakeScratchNormal(int size, int seed)
        {
            float[] h = new float[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    h[y * size + x] = Perlin(x * 0.35f, y * 0.35f, seed) * 0.15f;

            System.Random rng = new System.Random(seed);
            for (int s = 0; s < 70; s++)
            {
                float x0 = (float)rng.NextDouble() * size, y0 = (float)rng.NextDouble() * size;
                float ang = (float)rng.NextDouble() * Mathf.PI;
                float len = 6f + (float)rng.NextDouble() * 26f;
                for (float k = 0; k < len; k += 0.5f)
                {
                    int px = ((int)(x0 + Mathf.Cos(ang) * k) % size + size) % size;
                    int py = ((int)(y0 + Mathf.Sin(ang) * k) % size + size) % size;
                    h[py * size + px] -= 0.5f;
                }
            }
            Texture2D t = NewTex(size, true);
            Color32[] px2 = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float hl = h[y * size + (x + size - 1) % size], hr = h[y * size + (x + 1) % size];
                    float hd = h[((y + size - 1) % size) * size + x], hu = h[((y + 1) % size) * size + x];
                    Vector3 n = new Vector3((hl - hr) * 2f, (hd - hu) * 2f, 1f).normalized;
                    px2[y * size + x] = new Color32((byte)((n.x * 0.5f + 0.5f) * 255f), (byte)((n.y * 0.5f + 0.5f) * 255f), (byte)((n.z * 0.5f + 0.5f) * 255f), 255);
                }
            }
            t.SetPixels32(px2);
            t.Apply(true, false);
            return t;
        }

        /// <summary>Dark scrap iron with rust-orange speckles and grunge.</summary>
        private static Texture2D MakeRust(int size, int seed)
        {
            Texture2D t = NewTex(size, false);
            Color32[] px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float baseN = Perlin(x * 0.08f, y * 0.08f, seed);
                    float speck = Perlin(x * 0.5f, y * 0.5f, seed + 3f);
                    float v = 0.78f + baseN * 0.3f; // multiplies the material's iron tint
                    Color c = new Color(v, v, v);
                    float patch = Perlin(x * 0.05f, y * 0.05f, seed + 8f);
                    if (patch > 0.6f) c = Color.Lerp(c, new Color(1.5f, 0.62f, 0.26f), Mathf.Clamp01((patch - 0.6f) * 3.2f) * 0.85f);
                    if (speck > 0.74f) c = Color.Lerp(c, new Color(1.2f, 0.5f, 0.2f), Mathf.Clamp01((speck - 0.74f) * 5f));
                    if (baseN < 0.3f) c = Color.Lerp(c, new Color(0.35f, 0.35f, 0.37f), 0.5f);
                    c = new Color(Mathf.Min(c.r, 1f), Mathf.Min(c.g, 1f), Mathf.Min(c.b, 1f));
                    px[y * size + x] = c;
                }
            }
            t.SetPixels32(px);
            t.Apply(true, false);
            return t;
        }

        private static Texture2D MakeWood(int size, int seed)
        {
            Texture2D t = NewTex(size, false);
            Color32[] px = new Color32[size * size];
            Color light = new Color(0.36f, 0.22f, 0.12f), dark = new Color(0.17f, 0.095f, 0.05f); // dark walnut / oak
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float warp = Perlin(x * 0.01f, y * 0.04f, seed) * 6f;
                    float grain = 0.5f + 0.5f * Mathf.Sin(y * 0.18f + warp * 3f + Perlin(x * 0.2f, y * 0.02f, seed) * 2f);
                    float plank = ((x / (size / 4)) % 2 == 0) ? 0f : 0.08f;
                    Color c = Color.Lerp(dark, light, grain * 0.7f + plank + 0.15f);
                    px[y * size + x] = c;
                }
            }
            t.SetPixels32(px);
            t.Apply(true, false);
            return t;
        }

        /// <summary>Wood-grain normal map: long fibres along x plus pores, scratches from the generic scratch pass.</summary>
        private static Texture2D MakeWoodNormal(int size, int seed)
        {
            float[] h = new float[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float warp = Perlin(x * 0.01f, y * 0.04f, seed) * 6f;
                    float grain = 0.5f + 0.5f * Mathf.Sin(y * 0.18f + warp * 3f + Perlin(x * 0.2f, y * 0.02f, seed) * 2f);
                    h[y * size + x] = grain * 0.5f + Perlin(x * 0.05f, y * 0.8f, seed + 2f) * 0.35f;
                }
            System.Random rng = new System.Random(seed + 77);
            for (int s = 0; s < 40; s++)
            {
                float x0 = (float)rng.NextDouble() * size, y0 = (float)rng.NextDouble() * size;
                float ang = (float)rng.NextDouble() * Mathf.PI;
                float len = 10f + (float)rng.NextDouble() * 40f;
                for (float k = 0; k < len; k += 0.5f)
                {
                    int px = ((int)(x0 + Mathf.Cos(ang) * k) % size + size) % size;
                    int py = ((int)(y0 + Mathf.Sin(ang) * k) % size + size) % size;
                    h[py * size + px] -= 0.45f;
                }
            }
            Texture2D t = NewTex(size, true);
            Color32[] px2 = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float hl = h[y * size + (x + size - 1) % size], hr = h[y * size + (x + 1) % size];
                    float hd = h[((y + size - 1) % size) * size + x], hu = h[((y + 1) % size) * size + x];
                    Vector3 n = new Vector3((hl - hr) * 1.5f, (hd - hu) * 1.5f, 1f).normalized;
                    px2[y * size + x] = new Color32((byte)((n.x * 0.5f + 0.5f) * 255f), (byte)((n.y * 0.5f + 0.5f) * 255f), (byte)((n.z * 0.5f + 0.5f) * 255f), 255);
                }
            t.SetPixels32(px2);
            t.Apply(true, false);
            return t;
        }

        /// <summary>Brushed steel strip (u across the belt, v along it) with a white dashed centre line.</summary>
        private static Texture2D MakeDash(int size)
        {
            Texture2D t = NewTex(size, false);
            Color32[] px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float brushed = Perlin(x * 0.9f, y * 0.04f, 4f);
                    float v = 0.34f + brushed * 0.14f;
                    Color c = new Color(v, v, v * 1.04f);
                    float cx = Mathf.Abs((x + 0.5f) / size - 0.5f);
                    bool dash = ((y * 2) / size) % 2 == 0 || (y * 2 / size) == 0;
                    dash = (y < size * 0.5f);
                    if (cx < 0.055f && dash) c = new Color(0.95f, 0.95f, 0.92f);
                    if (cx > 0.43f) c *= 0.7f;
                    px[y * size + x] = c;
                }
            t.SetPixels32(px);
            t.wrapMode = TextureWrapMode.Repeat;
            t.Apply(true, false);
            return t;
        }

        private static Texture2D MakeNoise(int size, int seed, Color baseCol, float amp)
        {
            Texture2D t = NewTex(size, false);
            Color32[] px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float n = (Perlin(x * 0.4f, y * 0.4f, seed) - 0.5f) * amp * 2f;
                    px[y * size + x] = new Color(baseCol.r + n, baseCol.g + n, baseCol.b + n);
                }
            t.SetPixels32(px);
            t.Apply(true, false);
            return t;
        }

        private static Texture2D MakeBelt(int size, int seed)
        {
            Texture2D t = NewTex(size, false);
            Color32[] px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float brushed = Perlin(x * 0.02f, y * 0.9f, seed);
                    float v = 0.28f + brushed * 0.16f;
                    Color c = new Color(v, v, v * 1.05f);
                    if (Perlin(x * 0.3f, y * 0.3f, seed + 4f) > 0.8f) c = Color.Lerp(c, new Color(0.4f, 0.2f, 0.1f), 0.5f); // rust
                    px[y * size + x] = c;
                }
            t.SetPixels32(px);
            t.Apply(true, false);
            return t;
        }

        /// <summary>Cyan frosted ice with bright fracture lines.</summary>
        private static Texture2D MakeCracks(int size, int seed)
        {
            Color32[] px = new Color32[size * size];
            Color32 baseC = new Color(0.722f, 0.933f, 1f, 0.55f);
            for (int i = 0; i < px.Length; i++) px[i] = baseC;
            System.Random rng = new System.Random(seed);
            for (int branch = 0; branch < 9; branch++)
            {
                float x = size * 0.5f + (float)(rng.NextDouble() - 0.5) * size * 0.3f;
                float y = size * 0.5f + (float)(rng.NextDouble() - 0.5) * size * 0.3f;
                float ang = (float)rng.NextDouble() * Mathf.PI * 2f;
                int steps = 60 + rng.Next(0, 60);
                for (int s = 0; s < steps; s++)
                {
                    ang += ((float)rng.NextDouble() - 0.5f) * 0.6f;
                    x += Mathf.Cos(ang) * 1.6f;
                    y += Mathf.Sin(ang) * 1.6f;
                    for (int w = 0; w < 2; w++)
                    {
                        int px0 = ((int)x + w) % size, py0 = (int)y % size;
                        if (px0 < 0) px0 += size;
                        if (py0 < 0) py0 += size;
                        px[py0 * size + px0] = new Color(1f, 1f, 1f, 0.98f);
                    }
                }
            }
            Texture2D t = NewTex(size, false);
            t.SetPixels32(px);
            t.Apply(true, false);
            return t;
        }

        private static Texture2D MakeHazard(int size)
        {
            Texture2D t = NewTex(size, false);
            Color32[] px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    px[y * size + x] = (((x + y) / 16) % 2 == 0) ? new Color(0.95f, 0.72f, 0.05f) : new Color(0.08f, 0.08f, 0.09f);
            t.SetPixels32(px);
            t.Apply(true, false);
            return t;
        }
    }
}
