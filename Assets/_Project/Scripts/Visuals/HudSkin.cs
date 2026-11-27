using UnityEngine;

namespace ColorGateBlast
{
    /// <summary>
    /// Procedural industrial HUD art, generated once at startup (no sprite assets needed):
    /// riveted brushed-steel plaques (9-sliceable), circular metal medallions, recessed LED sockets with a glowing amber
    /// LED, plus the pliers / gear / padlock icons used by the booster buttons.
    /// </summary>
    public static class HudSkin
    {
        private static Sprite plate, medallion, disc, ledSocket, ledLit, padlock, pliers, gear;

        public const float PlateBorder = 26f;

        public static Sprite Plate { get { if (plate == null) plate = MakePlate(96, 22); return plate; } }
        public static Sprite Medallion { get { if (medallion == null) medallion = MakeMedallion(128); return medallion; } }
        public static Sprite Disc { get { if (disc == null) disc = MakeDisc(64); return disc; } }
        public static Sprite LedSocket { get { if (ledSocket == null) ledSocket = MakeSocket(64); return ledSocket; } }
        public static Sprite LedLit { get { if (ledLit == null) ledLit = MakeLed(96); return ledLit; } }
        public static Sprite Padlock { get { if (padlock == null) padlock = MakePadlock(64); return padlock; } }
        public static Sprite Pliers { get { if (pliers == null) pliers = MakePliers(128); return pliers; } }
        public static Sprite Gear { get { if (gear == null) gear = MakeGear(128); return gear; } }

        private static Texture2D NewTex(int size)
        {
            Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            return t;
        }

        private static Sprite ToSprite(Texture2D t, Vector4 border)
        {
            t.Apply(false, true);
            return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        private static float Cov(float d) { return Mathf.Clamp01(d + 0.5f); } // anti-aliased coverage from a signed distance (px)

        private static float Brushed(int x, int y, float seed)
        {
            return Mathf.PerlinNoise(x * 0.015f + seed, y * 0.75f + seed * 2f) - 0.5f;
        }

        // ------------------------------------------------------------------ riveted steel plate
        private static Sprite MakePlate(int size, float radius)
        {
            Texture2D t = NewTex(size);
            Color32[] px = new Color32[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x + 0.5f - half) - (half - radius);
                    float dy = Mathf.Abs(y + 0.5f - half) - (half - radius);
                    float outside = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f));
                    float d = radius - (outside + Mathf.Min(Mathf.Max(dx, dy), 0f)); // >0 inside
                    float a = Cov(d - 0.5f);
                    if (a <= 0f) { px[y * size + x] = new Color32(0, 0, 0, 0); continue; }

                    float v;
                    if (d < 2.2f) v = 0.1f;                                  // dark outline
                    else if (d < 5.5f)                                       // bevel: light from the top-left
                    {
                        float nx = (x + 0.5f - half) / half, ny = (y + 0.5f - half) / half;
                        float lit = Mathf.Clamp01(0.5f + 0.5f * (-nx * 0.6f + ny * 0.8f) * Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny)) * 1.6f);
                        v = Mathf.Lerp(0.3f, 0.92f, lit);
                    }
                    else
                    {
                        float g = (float)y / size;
                        v = Mathf.Lerp(0.5f, 0.74f, g) + Brushed(x, y, 3f) * 0.16f;
                    }
                    // rivets in the four corners
                    float rv = Rivet(x, y, size);
                    if (rv >= 0f && d >= 5.5f) v = rv;
                    byte b = (byte)(Mathf.Clamp01(v) * 255f);
                    px[y * size + x] = new Color32(b, (byte)(b * 1.0f), (byte)Mathf.Min(255, b * 1.04f), (byte)(a * 255f));
                }
            t.SetPixels32(px);
            return ToSprite(t, new Vector4(PlateBorder, PlateBorder, PlateBorder, PlateBorder));
        }

        private static float Rivet(int x, int y, int size)
        {
            float cx = 13.5f, cy = 13.5f;
            for (int k = 0; k < 4; k++)
            {
                float rx = (k & 1) == 0 ? cx : size - cx;
                float ry = (k & 2) == 0 ? cy : size - cy;
                float dx = x + 0.5f - rx, dy = y + 0.5f - ry;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < 5.4f)
                {
                    if (dist > 4.2f) return 0.18f;                          // dark ring around the rivet
                    float hl = Mathf.Clamp01(0.5f + (-dx * 0.7f + dy * 0.7f) / 8f);
                    return Mathf.Lerp(0.45f, 0.95f, hl);
                }
            }
            return -1f;
        }

        // ------------------------------------------------------------------ medallion
        private static Sprite MakeMedallion(int size)
        {
            Texture2D t = NewTex(size);
            Color32[] px = new Color32[size * size];
            float c = size * 0.5f, R = c - 1f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - c, dy = y + 0.5f - c;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Cov(R - r);
                    if (a <= 0f) { px[y * size + x] = new Color32(0, 0, 0, 0); continue; }
                    float ang = Mathf.Atan2(dy, dx);
                    float v;
                    if (r > R - 3f) v = 0.08f;                                   // outer outline
                    else if (r > R - 17f)                                        // bevelled rim (convex)
                    {
                        float k = (r - (R - 17f)) / 14f;                         // 0 inner .. 1 outer
                        float light = Mathf.Cos(ang - 2.35f);                    // light from the top-left
                        v = 0.55f + 0.4f * light * (k > 0.5f ? 1f : -1f) * 0.9f + Brushed(x, y, 5f) * 0.1f;
                    }
                    else if (r > R - 21f) v = 0.14f;                             // groove
                    else
                    {
                        float k = r / (R - 21f);                                 // face: slightly domed
                        float light = Mathf.Cos(ang - 2.35f) * k * 0.18f;
                        v = 0.52f - 0.08f * k + light + Brushed(x, y, 9f) * 0.12f;
                    }
                    byte b = (byte)(Mathf.Clamp01(v) * 255f);
                    px[y * size + x] = new Color32(b, b, (byte)Mathf.Min(255, b * 1.05f), (byte)(a * 255f));
                }
            t.SetPixels32(px);
            return ToSprite(t, Vector4.zero);
        }

        private static Sprite MakeDisc(int size)
        {
            Texture2D t = NewTex(size);
            Color32[] px = new Color32[size * size];
            float c = size * 0.5f, R = c - 1f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - c, dy = y + 0.5f - c;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Cov(R - r);
                    float shade = r > R - 3f ? 0.45f : Mathf.Clamp01(0.82f + (-dx * 0.5f + dy * 0.5f) / size * 0.5f);
                    byte b = (byte)(shade * 255f);
                    px[y * size + x] = new Color32(b, b, b, (byte)(a * 255f));
                }
            t.SetPixels32(px);
            return ToSprite(t, Vector4.zero);
        }

        // ------------------------------------------------------------------ LED socket + LED
        private static Sprite MakeSocket(int size)
        {
            Texture2D t = NewTex(size);
            Color32[] px = new Color32[size * size];
            float c = size * 0.5f, R = c - 1f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - c, dy = y + 0.5f - c;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Cov(R - r);
                    float v;
                    if (r > R - 6f) { float light = Mathf.Cos(Mathf.Atan2(dy, dx) - 2.35f); v = 0.5f + 0.3f * light; } // steel bezel
                    else { float depth = r / (R - 6f); float light = Mathf.Cos(Mathf.Atan2(dy, dx) + 0.8f); v = 0.05f + 0.07f * depth * light; } // dark recess
                    byte b = (byte)(Mathf.Clamp01(v) * 255f);
                    px[y * size + x] = new Color32(b, b, (byte)Mathf.Min(255, b * 1.06f), (byte)(a * 255f));
                }
            t.SetPixels32(px);
            return ToSprite(t, Vector4.zero);
        }

        private static Sprite MakeLed(int size)
        {
            Texture2D t = NewTex(size);
            Color32[] px = new Color32[size * size];
            float c = size * 0.5f, core = size * 0.3f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - c, dy = y + 0.5f - c;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float coreA = Cov(core - r);
                    float halo = Mathf.Clamp01(1f - r / (size * 0.5f));
                    halo = halo * halo * 0.55f;
                    float hl = Mathf.Clamp01(1f - Mathf.Sqrt((dx + core * 0.35f) * (dx + core * 0.35f) + (dy - core * 0.35f) * (dy - core * 0.35f)) / (core * 0.7f));
                    Color body = Color.Lerp(new Color(1f, 0.45f, 0.04f), new Color(1f, 0.82f, 0.3f), 1f - r / core);
                    body = Color.Lerp(body, new Color(1f, 0.97f, 0.8f), hl * 0.8f);
                    Color halo2 = new Color(1f, 0.6f, 0.1f, halo);
                    Color outc = Color.Lerp(new Color(halo2.r, halo2.g, halo2.b, halo2.a), new Color(body.r, body.g, body.b, 1f), coreA);
                    px[y * size + x] = (Color32)outc;
                }
            t.SetPixels32(px);
            return ToSprite(t, Vector4.zero);
        }

        // ------------------------------------------------------------------ icons
        private static float SegDist(float px, float py, float ax, float ay, float bx, float by)
        {
            float vx = bx - ax, vy = by - ay;
            float t = Mathf.Clamp01(((px - ax) * vx + (py - ay) * vy) / (vx * vx + vy * vy));
            float qx = ax + vx * t - px, qy = ay + vy * t - py;
            return Mathf.Sqrt(qx * qx + qy * qy);
        }

        /// <summary>Red-handled pliers (white where the colour is applied by tint is NOT used: colours are baked in).</summary>
        private static Sprite MakePliers(int size)
        {
            Texture2D t = NewTex(size);
            Color32[] px = new Color32[size * size];
            float s = size / 128f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float fx = (x + 0.5f) / s, fy = (y + 0.5f) / s;
                    Color col = new Color(0, 0, 0, 0);
                    // jaws (steel)
                    float dj = Mathf.Min(SegDist(fx, fy, 64, 66, 51, 122), SegDist(fx, fy, 64, 66, 77, 122));
                    float aj = Cov((6.5f - dj) * s);
                    // handles (red)
                    float dh = Mathf.Min(SegDist(fx, fy, 64, 62, 28, 6), SegDist(fx, fy, 64, 62, 100, 6));
                    float ah = Cov((7.5f - dh) * s);
                    if (aj > 0f)
                    {
                        float sh = 0.82f - 0.35f * Mathf.Clamp01(dj / 6.5f);
                        col = new Color(sh, sh, sh * 1.06f, aj);
                    }
                    if (ah > 0f)
                    {
                        float sh = 1f - 0.45f * Mathf.Clamp01(dh / 7.5f);
                        Color red = new Color(0.9f * sh, 0.16f * sh, 0.12f * sh, 1f);
                        col = Color.Lerp(col, new Color(red.r, red.g, red.b, 1f), ah);
                        col.a = Mathf.Max(col.a, ah);
                    }
                    // pivot
                    float dp = Mathf.Sqrt((fx - 64) * (fx - 64) + (fy - 64) * (fy - 64));
                    float ap = Cov((9f - dp) * s);
                    if (ap > 0f)
                    {
                        float hl = Mathf.Clamp01(0.5f + ((64 - fx) * 0.6f + (fy - 64) * 0.6f) / 14f);
                        float shv = dp > 6.5f ? 0.15f : Mathf.Lerp(0.5f, 0.95f, hl);
                        col = Color.Lerp(col, new Color(shv, shv, shv, 1f), ap);
                        col.a = Mathf.Max(col.a, ap);
                    }
                    px[y * size + x] = (Color32)col;
                }
            t.SetPixels32(px);
            return ToSprite(t, Vector4.zero);
        }

        private static Sprite MakeGear(int size)
        {
            Texture2D t = NewTex(size);
            Color32[] px = new Color32[size * size];
            float c = size * 0.5f, s = size / 128f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f - c) / s, dy = (y + 0.5f - c) / s;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float ang = Mathf.Atan2(dy, dx);
                    float tooth = Mathf.Clamp01(Mathf.Sin(ang * 10f) * 5f + 0.5f);
                    float outer = 40f + 12f * tooth;
                    float a = Cov((outer - r) * s) * Cov((r - 15f) * s);
                    if (a <= 0f) { px[y * size + x] = new Color32(0, 0, 0, 0); continue; }
                    float v;
                    if (r > outer - 2.5f) v = 0.22f;
                    else if (r < 18f) v = 0.2f;
                    else if (Mathf.Abs(r - 30f) < 1.6f) v = 0.25f;                 // groove
                    else v = 0.62f + 0.3f * Mathf.Cos(ang - 2.35f) * (r / 50f) * 0.5f + Brushed(x, y, 2f) * 0.1f;
                    byte b = (byte)(Mathf.Clamp01(v) * 255f);
                    px[y * size + x] = new Color32(b, b, (byte)Mathf.Min(255, b * 1.06f), (byte)(a * 255f));
                }
            t.SetPixels32(px);
            return ToSprite(t, Vector4.zero);
        }

        private static Sprite MakePadlock(int size)
        {
            Texture2D t = NewTex(size);
            Color32[] px = new Color32[size * size];
            float s = size / 64f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float fx = (x + 0.5f) / s, fy = (y + 0.5f) / s;
                    // body: rounded box (16..48, 8..34); shackle: ring centred (32, 34) r 11 above y 34
                    float bx = Mathf.Abs(fx - 32f) - 14f, by = Mathf.Abs(fy - 21f) - 9f;
                    float db = Mathf.Sqrt(Mathf.Max(bx, 0f) * Mathf.Max(bx, 0f) + Mathf.Max(by, 0f) * Mathf.Max(by, 0f)) + Mathf.Min(Mathf.Max(bx, by), 0f) - 4f;
                    float ar = Cov(-db * s);
                    float rr = Mathf.Sqrt((fx - 32f) * (fx - 32f) + (fy - 34f) * (fy - 34f));
                    float ring = Mathf.Abs(rr - 11f) - 3f;
                    float ash = (fy >= 33f) ? Cov(-ring * s) : 0f;
                    float a = Mathf.Max(ar, ash);
                    if (a <= 0f) { px[y * size + x] = new Color32(0, 0, 0, 0); continue; }
                    float v = ar >= ash ? 0.95f - 0.3f * Mathf.Clamp01((fy - 8f) / 26f) * -1f : 0.75f;
                    // keyhole
                    float kd = Mathf.Sqrt((fx - 32f) * (fx - 32f) + (fy - 23f) * (fy - 23f));
                    if (ar >= ash && (kd < 3.2f || (Mathf.Abs(fx - 32f) < 1.6f && fy < 23f && fy > 14f))) v = 0.15f;
                    byte b = (byte)(Mathf.Clamp01(v) * 255f);
                    px[y * size + x] = new Color32(b, b, b, (byte)(a * 255f));
                }
            t.SetPixels32(px);
            return ToSprite(t, Vector4.zero);
        }
    }
}
