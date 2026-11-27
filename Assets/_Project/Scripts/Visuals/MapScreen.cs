using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateBlast
{
    /// <summary>
    /// Friendly front end: a splash / loading screen followed by a scrolling story map with one node for each of the 150 levels
    /// (stars earned, the current level highlighted by a bobbing buddy, padlocks on levels not reached yet, a world banner per
    /// chapter). Everything is drawn from code on top of the HUD canvas.
    /// </summary>
    public sealed class MapScreen : MonoBehaviour
    {
        public const int TotalLevels = 150;
        private const string StarPref = "cgb_star_";
        private const string UnlockAllPref = "cgb_unlock_all";

        public event Action<int> LevelChosen;
        public event Action<string> Toast;

        private static readonly string[] WorldNames =
        {
            "Alarm Clock Alley", "Cassette Cove", "Rotary Ridge", "Pocket Watch Pond", "Espresso Estate", "Camera Canyon", "Typewriter Town",
            "Mystery World", "Mystery World", "Mystery World", "Mystery World", "Mystery World", "Mystery World", "Mystery World", "Mystery World"
        };
        private static readonly Color[] WorldTints =
        {
            new Color(0.80f, 0.94f, 0.78f), new Color(0.98f, 0.90f, 0.74f), new Color(0.86f, 0.82f, 0.98f), new Color(0.78f, 0.90f, 0.98f),
            new Color(0.97f, 0.84f, 0.74f), new Color(0.84f, 0.94f, 0.92f), new Color(0.96f, 0.86f, 0.92f), new Color(0.90f, 0.90f, 0.92f),
            new Color(0.90f, 0.90f, 0.92f), new Color(0.90f, 0.90f, 0.92f), new Color(0.90f, 0.90f, 0.92f), new Color(0.90f, 0.90f, 0.92f),
            new Color(0.90f, 0.90f, 0.92f), new Color(0.90f, 0.90f, 0.92f), new Color(0.90f, 0.90f, 0.92f)
        };
        private static readonly string[] Tips =
        {
            "Drag a block into the gate of its own colour - it becomes a turret!",
            "Same-colour turrets merge into a laser Mega-Turret.",
            "Hint costs a short video but shows the exact next move.",
            "Hard levels allow exactly the shortest solution - plan ahead!",
            "Rotate the monument with your finger to find hidden screws."
        };

        private RectTransform root;
        private Font font;
        private RectTransform splash, mapRoot, content, settingsPanel;
        private ScrollRect scroll;
        private Image splashFill;
        private Text splashText, starsText, trophyText, playText, soundLabel;
        private Button playButton;
        private RectTransform buddy;
        private readonly Image[] nodeImage = new Image[TotalLevels + 1];
        private readonly Image[] nodeBase = new Image[TotalLevels + 1];
        private readonly Image[] nodeRing = new Image[TotalLevels + 1];
        private readonly Text[] nodeSubLabel = new Text[TotalLevels + 1];
        private Text coinsText, avatarLevel;
        private readonly Text[] nodeLabel = new Text[TotalLevels + 1];
        private readonly Image[] nodeLock = new Image[TotalLevels + 1];
        private readonly Image[][] nodeStars = new Image[TotalLevels + 1][];
        private readonly Vector2[] nodePos = new Vector2[TotalLevels + 1];
        private int current = 1, available = 70;
        private float contentHeight;

        // ------------------------------------------------------------------ persistence
        public static int GetStars(int level) { return PlayerPrefs.GetInt(StarPref + level, 0); }
        public static void SetStars(int level, int stars)
        {
            if (stars > GetStars(level)) { PlayerPrefs.SetInt(StarPref + level, stars); PlayerPrefs.Save(); }
        }
        public static bool UnlockAll { get { return PlayerPrefs.GetInt(UnlockAllPref, 0) == 1; } }
        public bool IsOpen { get { return mapRoot != null && mapRoot.gameObject.activeSelf; } }

        // ------------------------------------------------------------------ construction helpers
        private static Image Img(RectTransform parent, string name, Sprite sprite, Color color, Vector2 size, Vector2 anchor, Vector2 pos)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            Image im = go.AddComponent<Image>();
            im.sprite = sprite;
            im.color = color;
            im.raycastTarget = false;
            return im;
        }

        private Text Label(RectTransform parent, string text, int size, Color color, Vector2 anchor, Vector2 pos, Vector2 box, bool outline)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = box;
            rt.anchoredPosition = pos;
            Text t = go.AddComponent<Text>();
            t.font = font; t.fontSize = size; t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter; t.color = color; t.text = text;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            if (outline)
            {
                Outline o = go.AddComponent<Outline>();
                o.effectColor = new Color(0.32f, 0.17f, 0.06f, 0.95f);
                o.effectDistance = new Vector2(3f, -3f);
            }
            return t;
        }

        private Button MakeButton(RectTransform parent, string label, int fontSize, Color tint, Vector2 anchor, Vector2 pos, Vector2 size, Action click)
        {
            Image im = Img(parent, label + "Button", HudSkin.Plate, tint, size, anchor, pos);
            im.type = Image.Type.Sliced;
            im.raycastTarget = true;
            Button b = im.gameObject.AddComponent<Button>();
            b.targetGraphic = im;
            ColorBlock cb = b.colors;
            cb.highlightedColor = new Color(0.94f, 0.94f, 0.94f, 1f);
            cb.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            b.colors = cb;
            b.onClick.AddListener(delegate { if (click != null) click(); });
            Text t = Label(im.rectTransform, label, fontSize, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, size, true);
            return b;
        }

        private static Sprite GradientSprite()
        {
            Texture2D t = new Texture2D(2, 64, TextureFormat.RGBA32, false);
            t.wrapMode = TextureWrapMode.Clamp;
            Color[] px = new Color[128];
            for (int y = 0; y < 64; y++)
            {
                float k = y / 63f;
                Color c = Color.Lerp(new Color(0.9f, 0.9f, 0.9f), Color.white, k);
                px[y * 2] = c; px[y * 2 + 1] = c;
            }
            t.SetPixels(px);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, 2, 64), new Vector2(0.5f, 0.5f), 100f);
        }

        // ------------------------------------------------------------------ build
        public void Build(RectTransform canvasRoot, Font f)
        {
            root = canvasRoot;
            font = f;
            BuildMap();
            BuildSplash();
            splash.SetAsLastSibling();
            mapRoot.gameObject.SetActive(false);
            splash.gameObject.SetActive(false);
        }

        private RectTransform Panel(string name, Color color)
        {
            Image im = Img(root, name, null, color, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero);
            im.raycastTarget = true;
            RectTransform rt = im.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return rt;
        }

        private RectTransform MakeBuddy(RectTransform parent, float size, Color tint)
        {
            Image body = Img(parent, "Buddy", HudSkin.Plate, tint, new Vector2(size, size), new Vector2(0.5f, 0.5f), Vector2.zero);
            body.type = Image.Type.Sliced;
            for (int s = -1; s <= 1; s += 2)
            {
                Img(body.rectTransform, "Eye", HudSkin.Disc, new Color(0.15f, 0.12f, 0.2f), Vector2.one * size * 0.17f, new Vector2(0.5f, 0.5f), new Vector2(s * size * 0.2f, size * 0.08f));
                Img(body.rectTransform, "Glint", HudSkin.Disc, Color.white, Vector2.one * size * 0.06f, new Vector2(0.5f, 0.5f), new Vector2(s * size * 0.2f + size * 0.03f, size * 0.12f));
            }
            Img(body.rectTransform, "Mouth", HudSkin.Disc, new Color(0.45f, 0.18f, 0.18f), new Vector2(size * 0.2f, size * 0.1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -size * 0.17f));
            return body.rectTransform;
        }

        private void BuildSplash()
        {
            splash = Panel("Splash", new Color(0.62f, 0.83f, 0.95f));
            Image grad = Img(splash, "Sky", GradientSprite(), new Color(0.55f, 0.8f, 0.96f), Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero);
            grad.rectTransform.anchorMin = Vector2.zero; grad.rectTransform.anchorMax = Vector2.one; grad.rectTransform.offsetMin = Vector2.zero; grad.rectTransform.offsetMax = Vector2.zero;
            Image meadow = Img(splash, "Meadow", HudSkin.Blob, new Color(0.62f, 0.86f, 0.55f), new Vector2(2600, 1100), new Vector2(0.5f, 0f), new Vector2(0, -520));
            for (int i = 0; i < 4; i++)
                Img(splash, "Cloud" + i, HudSkin.Blob, new Color(1f, 1f, 1f, 0.9f), new Vector2(220 + i * 40, 90 + i * 12), new Vector2(i % 2 == 0 ? 0.12f : 0.88f, 0.9f - i * 0.1f), Vector2.zero);

            RectTransform b1 = MakeBuddy(splash, 230, new Color(1f, 0.72f, 0.32f)); b1.anchorMin = b1.anchorMax = new Vector2(0.5f, 0.66f); b1.anchoredPosition = new Vector2(-150, 0); b1.localRotation = Quaternion.Euler(0, 0, 8f);
            RectTransform b2 = MakeBuddy(splash, 230, new Color(1f, 0.52f, 0.68f)); b2.anchorMin = b2.anchorMax = new Vector2(0.5f, 0.66f); b2.anchoredPosition = new Vector2(90, 70); b2.localRotation = Quaternion.Euler(0, 0, -10f);
            RectTransform b3 = MakeBuddy(splash, 200, new Color(0.55f, 0.62f, 0.96f)); b3.anchorMin = b3.anchorMax = new Vector2(0.5f, 0.66f); b3.anchoredPosition = new Vector2(-30, -150); b3.localRotation = Quaternion.Euler(0, 0, 4f);
            splashBuddies = new[] { b1, b2, b3 };

            Text t1 = Label(splash, "COLOR GATE", 128, new Color(1f, 0.62f, 0.12f), new Vector2(0.5f, 0.42f), new Vector2(0, 40), new Vector2(1000, 150), true);
            Text t2 = Label(splash, "BLAST", 170, new Color(1f, 0.62f, 0.12f), new Vector2(0.5f, 0.42f), new Vector2(0, -105), new Vector2(1000, 190), true);
            Image pill = Img(splash, "Pill", HudSkin.Plate, new Color(0.18f, 0.2f, 0.4f), new Vector2(620, 84), new Vector2(0.5f, 0.42f), new Vector2(0, -280));
            pill.type = Image.Type.Sliced;
            Label(pill.rectTransform, "BRAIN PUZZLE GAME", 38, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 80), false);

            Image track = Img(splash, "Track", HudSkin.Plate, new Color(0.55f, 0.58f, 0.62f), new Vector2(760, 44), new Vector2(0.5f, 0.2f), new Vector2(0, 60));
            track.type = Image.Type.Sliced;
            splashFill = Img(track.rectTransform, "Fill", HudSkin.Plate, new Color(1f, 0.62f, 0.12f), new Vector2(20, 30), new Vector2(0f, 0.5f), new Vector2(18, 0));
            splashFill.type = Image.Type.Sliced;
            splashFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            splashText = Label(splash, "Warming up your gears... 0%", 40, new Color(0.12f, 0.32f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0, -30), new Vector2(900, 60), false);
            Image tip = Img(splash, "Tip", HudSkin.Plate, new Color(1f, 1f, 1f, 0.95f), new Vector2(900, 120), new Vector2(0.5f, 0.07f), Vector2.zero);
            tip.type = Image.Type.Sliced;
            Img(tip.rectTransform, "Bulb", HudSkin.Disc, new Color(1f, 0.82f, 0.25f), new Vector2(62, 62), new Vector2(0f, 0.5f), new Vector2(70, 0));
            splashTip = Label(tip.rectTransform, Tips[0], 30, new Color(0.25f, 0.22f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(40, 0), new Vector2(740, 100), false);
            splashTip.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private RectTransform[] splashBuddies;
        private Text splashTip;

        public void ShowSplash(float seconds, Action done)
        {
            splash.gameObject.SetActive(true);
            splash.SetAsLastSibling();
            splashTip.text = "Tip: " + Tips[UnityEngine.Random.Range(0, Tips.Length)];
            StartCoroutine(SplashRoutine(seconds, done));
        }

        private IEnumerator SplashRoutine(float seconds, Action done)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += Mathf.Min(Time.unscaledDeltaTime, 0.05f); // the first frames after loading are long: do not let them skip the splash
                float p = Mathf.Clamp01(t / seconds);
                float eased = 1f - (1f - p) * (1f - p);
                int pct = Mathf.RoundToInt(eased * 100f);
                splashFill.rectTransform.sizeDelta = new Vector2(Mathf.Max(20f, 724f * eased), 30f);
                splashText.text = "Warming up your gears... " + pct + "%";
                for (int i = 0; i < splashBuddies.Length; i++)
                {
                    Vector2 ap = splashBuddies[i].anchoredPosition;
                    splashBuddies[i].localScale = Vector3.one * (1f + 0.04f * Mathf.Sin(Time.unscaledTime * 3f + i * 1.7f));
                }
                yield return null;
            }
            splash.gameObject.SetActive(false);
            if (done != null) done();
        }

        // ------------------------------------------------------------------ the story map
        private void BuildMap()
        {
            mapRoot = Panel("StoryMap", new Color(0.82f, 0.94f, 0.82f));

            // scroll view
            GameObject sv = new GameObject("Scroll", typeof(RectTransform));
            sv.transform.SetParent(mapRoot, false);
            RectTransform svr = sv.GetComponent<RectTransform>();
            svr.anchorMin = Vector2.zero; svr.anchorMax = Vector2.one;
            svr.offsetMin = new Vector2(0f, 150f); svr.offsetMax = new Vector2(0f, 0f);
            Image svBg = sv.AddComponent<Image>();
            svBg.color = new Color(0f, 0f, 0f, 0f);
            sv.AddComponent<RectMask2D>();
            scroll = sv.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 60f;
            scroll.viewport = svr;

            contentHeight = 240f + TotalLevels * 170f + 700f;
            GameObject c = new GameObject("Content", typeof(RectTransform));
            c.transform.SetParent(svr, false);
            content = c.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 0f); content.anchorMax = new Vector2(1f, 0f);
            content.pivot = new Vector2(0.5f, 0f);
            content.sizeDelta = new Vector2(0f, contentHeight);
            content.anchoredPosition = Vector2.zero;
            scroll.content = content;

            for (int i = 1; i <= TotalLevels; i++)
                nodePos[i] = new Vector2(540f + 290f * Mathf.Sin((i - 1) * 0.62f), 300f + (i - 1) * 170f);

            System.Random rng = new System.Random(5);
            // world bands, scenery and banners
            for (int w = 0; w < 15; w++)
            {
                float y0 = w == 0 ? 0f : nodePos[w * 10 + 1].y - 120f;
                float y1 = w == 14 ? contentHeight : nodePos[(w + 1) * 10 + 1].y - 120f;
                Image band = Img(content, "World" + (w + 1), GradientSprite(), WorldTints[w], new Vector2(0f, y1 - y0), new Vector2(0.5f, 0f), Vector2.zero);
                band.rectTransform.anchorMin = new Vector2(0f, 0f); band.rectTransform.anchorMax = new Vector2(1f, 0f);
                band.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                band.rectTransform.offsetMin = new Vector2(0f, y0); band.rectTransform.offsetMax = new Vector2(0f, y1);
                // ponds first, then trees and flowers, so nothing grows out of the water
                for (int k = 0; k < 2; k++)
                {
                    float py = y0 + 250f + k * ((y1 - y0 - 500f) * 0.55f) + (float)rng.NextDouble() * 120f;
                    bool left = (w + k) % 2 == 0;
                    float px = left ? 40f : 1040f;
                    Img(content, "PondEdge", HudSkin.Blob, new Color(0.45f, 0.72f, 0.92f), new Vector2(360f, 230f), Vector2.zero, new Vector2(px, py));
                    Img(content, "Pond", HudSkin.Blob, new Color(0.62f, 0.84f, 0.98f), new Vector2(330f, 205f), Vector2.zero, new Vector2(px, py + 4f));
                    Img(content, "Glint", HudSkin.Blob, new Color(1f, 1f, 1f, 0.55f), new Vector2(120f, 34f), Vector2.zero, new Vector2(px + (left ? 40f : -40f), py + 38f));
                }
                for (int k = 0; k < 16; k++)
                {
                    float ty = y0 + 90f + (float)rng.NextDouble() * (y1 - y0 - 180f);
                    bool left = rng.NextDouble() < 0.5;
                    float tx = left ? 60f + (float)rng.NextDouble() * 170f : 1020f - (float)rng.NextDouble() * 170f;
                    float ps = 76f + (float)rng.NextDouble() * 56f;
                    if (k % 6 == 5)
                    {
                        for (int f = 0; f < 3; f++)
                            Img(content, "Flower", HudSkin.Blob, new Color(1f, 0.8f + 0.2f * (float)rng.NextDouble(), 0.4f + 0.5f * (float)rng.NextDouble()), Vector2.one * 14f, Vector2.zero, new Vector2(tx + f * 20f, ty + (f % 2) * 10f));
                        continue;
                    }
                    Img(content, "TreeShadow", HudSkin.Blob, new Color(0.3f, 0.5f, 0.3f, 0.35f), new Vector2(ps * 0.9f, ps * 0.3f), Vector2.zero, new Vector2(tx, ty - ps * 0.62f));
                    Img(content, "Trunk", HudSkin.Plate, new Color(0.5f, 0.32f, 0.18f), new Vector2(16f, 40f), Vector2.zero, new Vector2(tx, ty - ps * 0.42f));
                    Img(content, "Tree", HudSkin.Blob, new Color(0.34f + 0.1f * (float)rng.NextDouble(), 0.72f, 0.38f), new Vector2(ps, ps), Vector2.zero, new Vector2(tx, ty));
                    Img(content, "TreeLight", HudSkin.Blob, new Color(0.62f, 0.9f, 0.5f, 0.8f), new Vector2(ps * 0.45f, ps * 0.45f), Vector2.zero, new Vector2(tx - ps * 0.12f, ty + ps * 0.14f));
                }
            }
            // winding path: Catmull-Rom curve through the nodes, drawn as short overlapping plates
            for (int i = 1; i < TotalLevels; i++)
            {
                Vector2 p0 = nodePos[Mathf.Max(1, i - 1)], p1 = nodePos[i], p2 = nodePos[i + 1], p3 = nodePos[Mathf.Min(TotalLevels, i + 2)];
                const int sub = 5;
                Vector2 prev = p1;
                for (int s = 1; s <= sub; s++)
                {
                    float t = s / (float)sub, t2 = t * t, t3 = t2 * t;
                    Vector2 cur = 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
                    Vector2 mid = (prev + cur) * 0.5f;
                    float len = Vector2.Distance(prev, cur);
                    float ang = Mathf.Atan2(cur.y - prev.y, cur.x - prev.x) * Mathf.Rad2Deg;
                    Image edge = Img(content, "PathEdge", HudSkin.Blob, new Color(0.88f, 0.72f, 0.46f), new Vector2(len + 30f, 60f), Vector2.zero, mid);
                    edge.rectTransform.localRotation = Quaternion.Euler(0, 0, ang);
                    Image road = Img(content, "Path", HudSkin.Blob, new Color(1f, 0.96f, 0.82f), new Vector2(len + 24f, 42f), Vector2.zero, mid);
                    road.rectTransform.localRotation = Quaternion.Euler(0, 0, ang);
                    prev = cur;
                }
            }
            // world banners
            for (int w = 0; w < 15; w++)
            {
                Vector2 p = nodePos[w * 10 + 1];
                Image pill = Img(content, "Banner" + w, HudSkin.Plate, new Color(0.2f, 0.22f, 0.36f), new Vector2(600f, 70f), Vector2.zero, new Vector2(540f, p.y - 105f));
                pill.type = Image.Type.Sliced;
                Img(pill.rectTransform, "Globe", HudSkin.Disc, new Color(0.5f, 0.75f, 0.95f), new Vector2(34f, 34f), new Vector2(0f, 0.5f), new Vector2(44f, 0f));
                Label(pill.rectTransform, "World " + (w + 1) + "  -  " + WorldNames[w], 30, Color.white, new Vector2(0.5f, 0.5f), new Vector2(22f, 0f), new Vector2(520f, 60f), false);
            }
            // nodes: raised 3D buttons with a ring, golden stars above, padlocks on the locked ones
            for (int i = 1; i <= TotalLevels; i++)
            {
                int level = i;
                bool boss = i % 10 == 0;
                nodeBase[i] = Img(content, "NodeBase" + i, HudSkin.Disc, Color.white, new Vector2(124f, 124f), Vector2.zero, nodePos[i] + new Vector2(0f, -10f));
                nodeRing[i] = Img(content, "NodeRing" + i, HudSkin.Disc, Color.white, new Vector2(132f, 132f), Vector2.zero, nodePos[i]);
                Image n = Img(content, "Node" + i, HudSkin.Disc, Color.white, new Vector2(120f, 120f), Vector2.zero, nodePos[i]);
                n.raycastTarget = true;
                Button b = n.gameObject.AddComponent<Button>();
                b.targetGraphic = n;
                b.onClick.AddListener(delegate { OnNode(level); });
                nodeImage[i] = n;
                nodeLabel[i] = Label(n.rectTransform, "" + i, boss ? 52 : 58, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120f, 120f), true);
                nodeLock[i] = Img(n.rectTransform, "Lock", HudSkin.Padlock, new Color(0.95f, 0.95f, 1f), new Vector2(58f, 58f), new Vector2(0.5f, 0.5f), Vector2.zero);
                nodeSubLabel[i] = Label(content, "" + i, 26, new Color(0.45f, 0.48f, 0.58f), Vector2.zero, nodePos[i] + new Vector2(0f, -82f), new Vector2(100f, 36f), false);
                nodeStars[i] = new Image[3];
                for (int s = 0; s < 3; s++)
                    nodeStars[i][s] = Img(n.rectTransform, "Star" + s, HudSkin.Star, Color.white, new Vector2(48f, 48f), new Vector2(0.5f, 1f), new Vector2((s - 1) * 46f, 14f - Mathf.Abs(s - 1) * 14f + 12f));
                if (boss)
                {
                    // treasure chest beside every chapter-boss node
                    float side = nodePos[i].x > 540f ? -1f : 1f;
                    Vector2 cp = nodePos[i] + new Vector2(side * 150f, 20f);
                    Img(content, "ChestBody", HudSkin.Plate, new Color(0.62f, 0.36f, 0.16f), new Vector2(84f, 58f), Vector2.zero, cp).type = Image.Type.Sliced;
                    Img(content, "ChestLid", HudSkin.Plate, new Color(0.74f, 0.45f, 0.2f), new Vector2(88f, 30f), Vector2.zero, cp + new Vector2(0f, 26f)).type = Image.Type.Sliced;
                    Img(content, "ChestBand", HudSkin.Plate, new Color(1f, 0.82f, 0.3f), new Vector2(16f, 66f), Vector2.zero, cp + new Vector2(0f, 8f));
                    Img(content, "ChestLock", HudSkin.Disc, new Color(1f, 0.9f, 0.4f), new Vector2(20f, 20f), Vector2.zero, cp + new Vector2(0f, 12f));
                    Image tag = Img(content, "ChestTag", HudSkin.Plate, new Color(0.2f, 0.22f, 0.36f), new Vector2(86f, 30f), Vector2.zero, cp + new Vector2(0f, -44f));
                    tag.type = Image.Type.Sliced;
                    Label(tag.rectTransform, "LV " + i, 20, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(86f, 30f), false);
                }
            }
            // the buddy in a speech bubble above the current level
            Image bubble = Img(content, "Bubble", HudSkin.Blob, Color.white, new Vector2(120f, 120f), Vector2.zero, Vector2.zero);
            Image tail = Img(bubble.rectTransform, "Tail", HudSkin.Blob, Color.white, new Vector2(34f, 34f), new Vector2(0.5f, 0f), new Vector2(0f, 2f));
            tail.rectTransform.localRotation = Quaternion.Euler(0, 0, 45f);
            RectTransform face = MakeBuddy(bubble.rectTransform, 84f, new Color(1f, 0.72f, 0.32f));
            buddy = bubble.rectTransform;
            buddy.SetAsLastSibling();

            BuildTopBar();
            BuildBottomBar();
            BuildSideButtons();
            BuildSettings();
        }

        private Image nodeBaseProxy;

        private Button Chip(RectTransform parent, string name, Color color, Vector2 anchor, Vector2 pos, Vector2 size, out Text value, Action click, bool plus)
        {
            Image chip = Img(parent, name, HudSkin.Plate, color, size, anchor, pos);
            chip.type = Image.Type.Sliced;
            chip.raycastTarget = true;
            Button b = chip.gameObject.AddComponent<Button>();
            b.targetGraphic = chip;
            b.onClick.AddListener(delegate { if (click != null) click(); });
            value = Label(chip.rectTransform, "0", 40, Color.white, new Vector2(0.5f, 0.5f), new Vector2(plus ? 14f : 30f, 0f), new Vector2(size.x - 100f, size.y), false);
            if (plus)
            {
                Image pl = Img(chip.rectTransform, "Plus", HudSkin.Plate, new Color(0.35f, 0.85f, 0.4f), new Vector2(52f, 52f), new Vector2(1f, 0.5f), new Vector2(-36f, 0f));
                pl.type = Image.Type.Sliced;
                Label(pl.rectTransform, "+", 44, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(52f, 52f), false);
            }
            return b;
        }

        private void BuildTopBar()
        {
            RectTransform top = Img(mapRoot, "TopBar", null, new Color(0f, 0f, 0f, 0f), new Vector2(0f, 150f), new Vector2(0.5f, 1f), new Vector2(0f, -75f)).rectTransform;
            top.anchorMin = new Vector2(0f, 1f); top.anchorMax = new Vector2(1f, 1f); top.offsetMin = new Vector2(0f, -150f); top.offsetMax = Vector2.zero;
            // avatar with level badge
            Image avRing = Img(top, "AvatarRing", HudSkin.Disc, new Color(1f, 0.95f, 0.85f), new Vector2(112f, 112f), new Vector2(0f, 0.5f), new Vector2(76f, -4f));
            RectTransform av = MakeBuddy(avRing.rectTransform, 88f, new Color(1f, 0.72f, 0.32f));
            Image lvBadge = Img(top, "AvatarLevel", HudSkin.Disc, new Color(0.3f, 0.35f, 0.7f), new Vector2(40f, 40f), new Vector2(0f, 0.5f), new Vector2(76f, -62f));
            avatarLevel = Label(lvBadge.rectTransform, "1", 24, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40f, 40f), false);
            Color chipCol = new Color(0.42f, 0.24f, 0.12f);
            Chip(top, "CoinChip", chipCol, new Vector2(0f, 0.5f), new Vector2(330f, 0f), new Vector2(300f, 84f), out coinsText, delegate { if (Toast != null) Toast("Coins come from stars: 10 per star, 50 per chapter boss!"); }, true);
            Img(top, "CoinIcon", HudSkin.Disc, new Color(1f, 0.8f, 0.2f), new Vector2(50f, 50f), new Vector2(0f, 0.5f), new Vector2(205f, 0f));
            Chip(top, "StarChip", chipCol, new Vector2(0f, 0.5f), new Vector2(610f, 0f), new Vector2(200f, 84f), out starsText, null, false);
            Img(top, "StarIcon", HudSkin.Star, new Color(1f, 0.84f, 0.2f), new Vector2(54f, 54f), new Vector2(0f, 0.5f), new Vector2(536f, 0f));
            Chip(top, "TrophyChip", chipCol, new Vector2(0f, 0.5f), new Vector2(835f, 0f), new Vector2(210f, 84f), out trophyText, null, false);
            Img(top, "TrophyIcon", HudSkin.Disc, new Color(1f, 0.78f, 0.28f), new Vector2(40f, 40f), new Vector2(0f, 0.5f), new Vector2(750f, 0f));
            MakeButton(top, "SET", 26, new Color(1f, 0.85f, 0.7f), new Vector2(1f, 0.5f), new Vector2(-62f, 0f), new Vector2(92f, 84f), ToggleSettings);
        }

        private void NavTab(RectTransform bar, string label, int index, bool selected, Action click)
        {
            float x = (index - 1.5f) * 250f;
            Image tab = Img(bar, "Tab" + label, HudSkin.Plate, selected ? new Color(1f, 0.6f, 0.15f) : new Color(0.42f, 0.24f, 0.12f), selected ? new Vector2(130f, 130f) : new Vector2(90f, 90f), new Vector2(0.5f, 0.5f), new Vector2(x, selected ? 22f : 10f));
            tab.type = Image.Type.Sliced;
            tab.raycastTarget = true;
            Button b = tab.gameObject.AddComponent<Button>();
            b.targetGraphic = tab;
            b.onClick.AddListener(delegate { if (click != null) click(); });
            Color ic = Color.white;
            RectTransform t = tab.rectTransform;
            if (index == 0)
            {
                Img(t, "Roof", HudSkin.Plate, ic, new Vector2(60f, 22f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f));
                Img(t, "House", HudSkin.Plate, ic, new Vector2(44f, 34f), new Vector2(0.5f, 0.5f), new Vector2(0f, -6f));
            }
            else if (index == 1)
            {
                Img(t, "Cal", HudSkin.Plate, ic, new Vector2(50f, 46f), new Vector2(0.5f, 0.5f), new Vector2(0f, 4f));
                Img(t, "CalTop", HudSkin.Plate, new Color(1f, 0.55f, 0.55f), new Vector2(50f, 14f), new Vector2(0.5f, 0.5f), new Vector2(0f, 22f));
            }
            else if (index == 2)
            {
                Img(t, "Bag", HudSkin.Plate, ic, new Vector2(46f, 42f), new Vector2(0.5f, 0.5f), new Vector2(0f, 2f));
                Img(t, "BagHandle", HudSkin.Disc, ic, new Vector2(24f, 18f), new Vector2(0.5f, 0.5f), new Vector2(0f, 26f));
            }
            else
            {
                Img(t, "Head", HudSkin.Disc, ic, new Vector2(24f, 24f), new Vector2(0.5f, 0.5f), new Vector2(0f, 14f));
                Img(t, "Body", HudSkin.Blob, ic, new Vector2(46f, 30f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f));
            }
            Label(bar, label, 26, selected ? Color.white : new Color(1f, 0.9f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(x, selected ? -50f : -54f), new Vector2(200f, 36f), false);
        }

        private void BuildBottomBar()
        {
            Image bar = Img(mapRoot, "BottomBar", HudSkin.Plate, new Color(0.5f, 0.29f, 0.15f), new Vector2(0f, 150f), new Vector2(0.5f, 0f), new Vector2(0f, 75f));
            bar.type = Image.Type.Sliced;
            bar.rectTransform.anchorMin = new Vector2(0f, 0f); bar.rectTransform.anchorMax = new Vector2(1f, 0f);
            bar.rectTransform.offsetMin = Vector2.zero; bar.rectTransform.offsetMax = new Vector2(0f, 150f);
            bar.raycastTarget = true;
            RectTransform br = bar.rectTransform;
            NavTab(br, "Map", 0, true, null);
            NavTab(br, "Daily", 1, false, delegate { if (Toast != null) Toast("Daily challenges are coming soon!"); });
            NavTab(br, "Shop", 2, false, delegate { if (Toast != null) Toast("The shop is coming soon!"); });
            NavTab(br, "Profile", 3, false, delegate { if (Toast != null) Toast("Profiles are coming soon!"); });
            playButton = MakeButton(mapRoot, "PLAY", 60, new Color(0.3f, 0.82f, 0.36f), new Vector2(0.5f, 0f), new Vector2(0f, 215f), new Vector2(760f, 130f), delegate { OnNode(current); });
            playText = playButton.GetComponentInChildren<Text>();
            Img(playButton.GetComponent<Image>().rectTransform, "PlayTri", HudSkin.Star, new Color(1f, 1f, 1f, 0f), Vector2.one, new Vector2(0f, 0.5f), Vector2.zero);
        }

        private void BuildSideButtons()
        {
            SideBadge("Daily", new Color(0.55f, 0.4f, 0.95f), new Vector2(0f, 1f), new Vector2(80f, -240f), "!", delegate { if (Toast != null) Toast("Daily challenges are coming soon!"); });
            SideBadge("Streak", new Color(1f, 0.6f, 0.25f), new Vector2(0f, 1f), new Vector2(80f, -400f), "0", delegate { if (Toast != null) Toast("Play every day to build a streak - coming soon!"); });
        }

        private void SideBadge(string label, Color col, Vector2 anchor, Vector2 pos, string badge, Action click)
        {
            Image b = Img(mapRoot, "Side" + label, HudSkin.Plate, col, new Vector2(112f, 112f), anchor, pos);
            b.type = Image.Type.Sliced;
            b.raycastTarget = true;
            Button bt = b.gameObject.AddComponent<Button>();
            bt.targetGraphic = b;
            bt.onClick.AddListener(delegate { if (click != null) click(); });
            Img(b.rectTransform, "Icon", HudSkin.Disc, new Color(1f, 1f, 1f, 0.9f), new Vector2(46f, 46f), new Vector2(0.5f, 0.5f), new Vector2(0f, 6f));
            Label(b.rectTransform, label, 22, Color.white, new Vector2(0.5f, 0f), new Vector2(0f, -16f), new Vector2(150f, 30f), true);
            Image dot = Img(b.rectTransform, "Dot", HudSkin.Disc, label == "Daily" ? new Color(1f, 0.3f, 0.35f) : new Color(0.4f, 0.3f, 0.7f), new Vector2(40f, 40f), new Vector2(1f, 1f), new Vector2(-6f, -6f));
            Label(dot.rectTransform, badge, 24, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40f, 40f), false);
        }

        private void BuildSettings()
        {
            Image dim = Img(mapRoot, "SettingsDim", null, new Color(0f, 0f, 0f, 0.55f), Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero);
            dim.rectTransform.anchorMin = Vector2.zero; dim.rectTransform.anchorMax = Vector2.one; dim.rectTransform.offsetMin = Vector2.zero; dim.rectTransform.offsetMax = Vector2.zero;
            dim.raycastTarget = true;
            settingsPanel = dim.rectTransform;
            Image card = Img(settingsPanel, "Card", HudSkin.Plate, new Color(0.96f, 0.92f, 0.82f), new Vector2(820f, 620f), new Vector2(0.5f, 0.5f), Vector2.zero);
            card.type = Image.Type.Sliced;
            Label(card.rectTransform, "SETTINGS", 64, new Color(0.3f, 0.18f, 0.1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(700f, 90f), false);
            Button snd = MakeButton(card.rectTransform, "SOUND: ON", 44, new Color(0.35f, 0.62f, 0.95f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(600f, 110f), delegate
            {
                AudioListener.volume = AudioListener.volume > 0.01f ? 0f : 1f;
                soundLabel.text = AudioListener.volume > 0.01f ? "SOUND: ON" : "SOUND: OFF";
            });
            soundLabel = snd.GetComponentInChildren<Text>();
            MakeButton(card.rectTransform, "UNLOCK ALL (TESTING)", 36, new Color(0.9f, 0.55f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(0f, -85f), new Vector2(600f, 110f), delegate
            {
                PlayerPrefs.SetInt(UnlockAllPref, UnlockAll ? 0 : 1);
                PlayerPrefs.Save();
                Refresh();
                if (Toast != null) Toast(UnlockAll ? "All levels unlocked" : "Back to normal progress");
            });
            MakeButton(card.rectTransform, "CLOSE", 44, new Color(0.45f, 0.45f, 0.5f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(400f, 100f), ToggleSettings);
            settingsPanel.gameObject.SetActive(false);
        }

        private void ToggleSettings() { settingsPanel.gameObject.SetActive(!settingsPanel.gameObject.activeSelf); if (settingsPanel.gameObject.activeSelf) settingsPanel.SetAsLastSibling(); }

        private int reachedLevel = 1;

        private bool Reachable(int level)
        {
            return level <= available && (UnlockAll || level <= reachedLevel);
        }

        private void OnNode(int level)
        {
            if (level > available) { if (Toast != null) Toast("Level " + level + " is coming soon!"); return; }
            if (!Reachable(level)) { if (Toast != null) Toast("Finish level " + reachedLevel + " first!"); return; }
            if (LevelChosen != null) LevelChosen(level);
        }

        private void Refresh()
        {
            int stars = 0, trophies = 0;
            for (int i = 1; i <= TotalLevels; i++)
            {
                int st = GetStars(i);
                stars += st;
                if (i % 10 == 0 && st > 0) trophies++;
                bool reach = Reachable(i);
                bool isCurrent = i == current;
                bool boss = i % 10 == 0;
                Color col;
                if (!reach) col = new Color(0.74f, 0.77f, 0.86f);
                else if (isCurrent) col = new Color(0.52f, 0.36f, 0.96f);
                else if (st > 0) col = boss ? new Color(0.96f, 0.46f, 0.2f) : new Color(1f, 0.62f, 0.2f);
                else col = new Color(0.98f, 0.72f, 0.3f);
                float size = isCurrent ? 150f : (boss ? 136f : 120f);
                nodeImage[i].color = col;
                nodeImage[i].rectTransform.sizeDelta = Vector2.one * size;
                nodeBase[i].color = new Color(col.r * 0.62f, col.g * 0.62f, col.b * 0.62f);
                nodeBase[i].rectTransform.sizeDelta = Vector2.one * (size + 4f);
                nodeRing[i].color = reach ? Color.white : new Color(0.9f, 0.92f, 0.97f);
                nodeRing[i].rectTransform.sizeDelta = Vector2.one * (size + 12f);
                nodeLock[i].gameObject.SetActive(!reach);
                nodeLabel[i].gameObject.SetActive(reach);
                nodeSubLabel[i].gameObject.SetActive(!reach);
                for (int s = 0; s < 3; s++)
                {
                    nodeStars[i][s].gameObject.SetActive(reach && i <= available && !isCurrent);
                    nodeStars[i][s].color = s < st ? new Color(1f, 0.84f, 0.2f) : new Color(0.62f, 0.5f, 0.42f, 0.5f);
                }
            }
            starsText.text = stars.ToString();
            coinsText.text = (stars * 10 + trophies * 50).ToString("N0");
            trophyText.text = trophies + " / 15";
            avatarLevel.text = Mathf.Max(1, Mathf.Min(reachedLevel, TotalLevels)).ToString();
            playText.text = current <= available ? "PLAY LEVEL " + current : "COMING SOON";
            buddy.anchoredPosition = nodePos[current] + new Vector2(0f, 150f);
        }

        public void ShowMap(int reached, int levelsAvailable, int focus)
        {
            reachedLevel = Mathf.Max(1, reached);
            available = levelsAvailable;
            current = Mathf.Clamp(UnlockAll ? focus : Mathf.Min(reachedLevel, levelsAvailable), 1, TotalLevels);
            if (!UnlockAll && reachedLevel > levelsAvailable) current = levelsAvailable;
            settingsPanel.gameObject.SetActive(false);
            Refresh();
            mapRoot.gameObject.SetActive(true);
            mapRoot.SetAsLastSibling();
            StartCoroutine(FocusNextFrame());
        }

        private IEnumerator FocusNextFrame()
        {
            yield return null;
            yield return null;
            float viewH = ((RectTransform)scroll.viewport).rect.height;
            float target = nodePos[current].y - viewH * 0.4f;
            float denom = Mathf.Max(1f, contentHeight - viewH);
            scroll.verticalNormalizedPosition = Mathf.Clamp01(target / denom);
        }

        public void Hide() { mapRoot.gameObject.SetActive(false); splash.gameObject.SetActive(false); }

        private void Update()
        {
            if (buddy != null && mapRoot != null && mapRoot.gameObject.activeSelf)
                buddy.anchoredPosition = nodePos[current] + new Vector2(0f, 150f + 12f * Mathf.Sin(Time.unscaledTime * 3.2f));
        }
    }
}
