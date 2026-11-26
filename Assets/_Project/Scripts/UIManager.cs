using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace ColorGateBlast
{
    /// <summary>
    /// HUD + overlays on a 1080x1920 canvas (legacy uGUI Text, so no TextMeshPro essentials are needed):
    /// level tracker with a Gold/Red boss banner, remaining target pieces, moves, track capacity, Restart,
    /// Hint (with an "AD" badge), "+1 Track Slot" rescue, win overlay (Next / Replay) and lose overlay
    /// (Revive +5 moves / +1 slot / Retry).
    /// <see cref="BuildUI"/> creates everything; the Setup tool calls it in the Editor and the scene saves the result.
    /// </summary>
    public sealed class UIManager : MonoBehaviour
    {
        public event Action RestartClicked;
        public event Action NextClicked;
        public event Action HintClicked;
        public event Action ExtraSlotClicked;
        public event Action ReviveClicked;

        [SerializeField] private Text levelText;
        [SerializeField] private GameObject bannerRoot;
        [SerializeField] private Image bannerImage;
        [SerializeField] private Text bannerText;
        [SerializeField] private Text targetsText;
        [SerializeField] private Text movesText;
        [SerializeField] private Text trackText;
        [SerializeField] private Text toastText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button hintButton;
        [SerializeField] private Button extraSlotButton;
        [SerializeField] private GameObject rotateHint;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Text winTitle;
        [SerializeField] private Text winDetail;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private Text loseTitle;
        [SerializeField] private Text loseDetail;
        [SerializeField] private Button reviveButton;
        [SerializeField] private Text reviveButtonText;
        [SerializeField] private Button retryButton;

        private float toastUntil;
        private float lastToastTime = -10f;
        private string lastToast;

        public bool IsBuilt { get { return levelText != null && winPanel != null && losePanel != null && hintButton != null && extraSlotButton != null; } }

        private void Awake()
        {
            if (!IsBuilt) BuildUI(null);
            Wire();
        }

        private void Wire()
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(delegate { if (RestartClicked != null) RestartClicked(); });
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(delegate { if (RestartClicked != null) RestartClicked(); });
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(delegate { if (RestartClicked != null) RestartClicked(); });
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(delegate { if (NextClicked != null) NextClicked(); });
            hintButton.onClick.RemoveAllListeners();
            hintButton.onClick.AddListener(delegate { if (HintClicked != null) HintClicked(); });
            extraSlotButton.onClick.RemoveAllListeners();
            extraSlotButton.onClick.AddListener(delegate { if (ExtraSlotClicked != null) ExtraSlotClicked(); });
            reviveButton.onClick.RemoveAllListeners();
            reviveButton.onClick.AddListener(delegate { if (ReviveClicked != null) ReviveClicked(); });
        }

        private void Update()
        {
            if (toastText != null && toastText.gameObject.activeSelf && Time.unscaledTime > toastUntil)
                toastText.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ public API

        public void SetLevel(int number, string title, LevelTier tier)
        {
            levelText.text = "LEVEL " + number;
            bool boss = tier != LevelTier.Normal;
            bannerRoot.SetActive(boss);
            if (boss)
            {
                bannerText.text = tier == LevelTier.ChapterBoss ? "CHAPTER BOSS  -  " + title.ToUpper() : "MINI-BOSS  -  " + title.ToUpper();
                bannerImage.color = tier == LevelTier.ChapterBoss ? new Color(0.78f, 0.12f, 0.14f, 0.95f) : new Color(0.95f, 0.72f, 0.12f, 0.95f);
                bannerText.color = tier == LevelTier.ChapterBoss ? new Color(1f, 0.9f, 0.45f) : new Color(0.45f, 0.08f, 0.08f);
            }
        }

        public void SetTargets(int remaining) { targetsText.text = "TARGETS " + remaining; }

        public void SetMoves(int left)
        {
            movesText.text = "MOVES " + left;
            movesText.color = left <= 2 ? new Color(1f, 0.45f, 0.45f) : Color.white;
        }

        public void SetTrack(int used, int max, bool unlimited)
        {
            trackText.text = unlimited ? "TRACK " + used + "/INF" : "TRACK " + used + "/" + max;
            trackText.color = (!unlimited && used >= max) ? new Color(1f, 0.45f, 0.45f) : Color.white;
        }

        public void SetHintAvailable(bool interactable) { hintButton.interactable = interactable; }

        /// <summary>The "+1 Track Slot" rescue button only appears when the conveyor is at capacity.</summary>
        public void SetExtraSlotVisible(bool visible) { extraSlotButton.gameObject.SetActive(visible); }

        public void SetRotateHint(bool visible) { if (rotateHint != null) rotateHint.SetActive(visible); }

        public void ShowToast(string message)
        {
            if (message == lastToast && Time.unscaledTime - lastToastTime < 1.0f) return;
            lastToast = message;
            lastToastTime = Time.unscaledTime;
            toastText.text = message;
            toastText.gameObject.SetActive(true);
            toastUntil = Time.unscaledTime + 1.8f;
        }

        public void HidePanels()
        {
            winPanel.SetActive(false);
            losePanel.SetActive(false);
        }

        public void ShowWin(int stars, int movesUsed, int par, bool hasNext, string reward)
        {
            losePanel.SetActive(false);
            winPanel.SetActive(true);
            winTitle.text = "LEVEL CLEAR!";
            string extra = string.IsNullOrEmpty(reward) ? "" : "\n\n" + reward;
            winDetail.text = "Stars: " + stars + " / 3\nMoves used: " + movesUsed + "   Par: " + par + extra;
            nextButton.gameObject.SetActive(hasNext);
        }

        /// <summary>Lose overlay. reviveLabel empty = no rescue button.</summary>
        public void ShowLose(string title, string detail, string reviveLabel)
        {
            winPanel.SetActive(false);
            losePanel.SetActive(true);
            loseTitle.text = title;
            loseDetail.text = detail;
            bool canRevive = !string.IsNullOrEmpty(reviveLabel);
            reviveButton.gameObject.SetActive(canRevive);
            if (canRevive) reviveButtonText.text = reviveLabel;
        }

        // ------------------------------------------------------------------ construction

        private static readonly Color PanelColor = new Color(0.06f, 0.07f, 0.12f, 0.92f);
        private static readonly Color ButtonBlue = new Color(0.25f, 0.5f, 0.96f, 1f);
        private static readonly Color ButtonGray = new Color(0.4f, 0.43f, 0.55f, 1f);
        private static readonly Color ButtonGold = new Color(0.96f, 0.68f, 0.16f, 1f);

        /// <summary>Creates the canvas hierarchy under this object. <paramref name="sprite"/> may be null (flat rectangles).</summary>
        public void BuildUI(Sprite sprite)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            RectTransform root = canvasGo.GetComponent<RectTransform>();

            Vector2 topC = new Vector2(0.5f, 1f);
            // --- header ---
            levelText = MakeText(root, "LevelText", font, 54, TextAnchor.MiddleCenter, topC, new Vector2(0, -60), new Vector2(900, 80));
            GameObject banner = new GameObject("BossBanner", typeof(RectTransform));
            banner.transform.SetParent(root, false);
            RectTransform brt = banner.GetComponent<RectTransform>();
            brt.anchorMin = topC; brt.anchorMax = topC; brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = new Vector2(0, -130); brt.sizeDelta = new Vector2(900, 64);
            bannerImage = banner.AddComponent<Image>();
            bannerImage.sprite = sprite;
            bannerImage.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            bannerRoot = banner;
            bannerText = MakeText(brt, "BannerText", font, 34, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880, 60));
            banner.SetActive(false);

            targetsText = MakeText(root, "TargetsText", font, 34, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(40, -215), new Vector2(330, 60));
            targetsText.rectTransform.pivot = new Vector2(0f, 0.5f);
            movesText = MakeText(root, "MovesText", font, 34, TextAnchor.MiddleCenter, topC, new Vector2(0, -215), new Vector2(330, 60));
            trackText = MakeText(root, "TrackText", font, 34, TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(-40, -215), new Vector2(360, 60));
            trackText.rectTransform.pivot = new Vector2(1f, 0.5f);

            // --- Restart + Hint (with AD badge), side by side ---
            restartButton = MakeButton(root, "RestartButton", font, sprite, ButtonGray, "RESTART", 30, new Vector2(0f, 1f), new Vector2(150, -300), new Vector2(240, 80));
            hintButton = MakeButton(root, "HintButton", font, sprite, ButtonBlue, "HINT", 34, new Vector2(0f, 1f), new Vector2(420, -300), new Vector2(240, 80));
            MakeBadge(hintButton.GetComponent<RectTransform>(), font, sprite);

            // --- rescue: +1 track slot (only while at capacity) ---
            extraSlotButton = MakeButton(root, "ExtraSlotButton", font, sprite, ButtonGold, "+1 TRACK SLOT", 36, new Vector2(0.5f, 0f), new Vector2(0, 90), new Vector2(560, 100));
            MakeBadge(extraSlotButton.GetComponent<RectTransform>(), font, sprite);
            extraSlotButton.gameObject.SetActive(false);

            // --- 360 rotate hint (shown next to the monument) ---
            GameObject rot = new GameObject("RotateHint", typeof(RectTransform));
            rot.transform.SetParent(root, false);
            RectTransform rr = rot.GetComponent<RectTransform>();
            rr.anchorMin = new Vector2(1f, 1f); rr.anchorMax = new Vector2(1f, 1f); rr.pivot = new Vector2(0.5f, 0.5f);
            rr.anchoredPosition = new Vector2(-110, -520); rr.sizeDelta = new Vector2(130, 130);
            Image ri = rot.AddComponent<Image>();
            ri.sprite = sprite; ri.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            ri.color = new Color(0.1f, 0.12f, 0.2f, 0.7f);
            ri.raycastTarget = false;
            Text rt = MakeText(rr, "Label", font, 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(130, 130));
            rt.text = "360\n<->\nDRAG";
            rotateHint = rot;
            rot.SetActive(false);

            // --- toast ---
            toastText = MakeText(root, "Toast", font, 44, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(980, 140));
            toastText.gameObject.SetActive(false);

            // --- win overlay ---
            winPanel = MakePanel(root, "WinPanel", sprite);
            RectTransform wr = winPanel.GetComponent<RectTransform>();
            winTitle = MakeText(wr, "Title", font, 78, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 300), new Vector2(950, 120));
            winDetail = MakeText(wr, "Detail", font, 44, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 90), new Vector2(950, 360));
            nextButton = MakeButton(wr, "NextButton", font, sprite, ButtonBlue, "NEXT LEVEL", 46, new Vector2(0.5f, 0.5f), new Vector2(0, -170), new Vector2(560, 120));
            replayButton = MakeButton(wr, "ReplayButton", font, sprite, ButtonGray, "REPLAY", 38, new Vector2(0.5f, 0.5f), new Vector2(0, -320), new Vector2(400, 100));
            winPanel.SetActive(false);

            // --- lose overlay ---
            losePanel = MakePanel(root, "LosePanel", sprite);
            RectTransform lr = losePanel.GetComponent<RectTransform>();
            loseTitle = MakeText(lr, "Title", font, 72, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 280), new Vector2(980, 120));
            loseDetail = MakeText(lr, "Detail", font, 42, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 90), new Vector2(950, 260));
            reviveButton = MakeButton(lr, "ReviveButton", font, sprite, ButtonGold, "+5 MOVES / REVIVE", 40, new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(680, 120));
            reviveButtonText = reviveButton.transform.Find("Label").GetComponent<Text>();
            MakeBadge(reviveButton.GetComponent<RectTransform>(), font, sprite);
            retryButton = MakeButton(lr, "RetryButton", font, sprite, ButtonBlue, "TRY AGAIN", 42, new Vector2(0.5f, 0.5f), new Vector2(0, -300), new Vector2(460, 110));
            losePanel.SetActive(false);

            EnsureEventSystem();
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        private static Text MakeText(RectTransform parent, string name, Font font, int size, TextAnchor anchor, Vector2 anchorPoint, Vector2 pos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorPoint;
            rt.anchorMax = anchorPoint;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            Text t = go.AddComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.fontStyle = FontStyle.Bold;
            t.alignment = anchor;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            Outline o = go.AddComponent<Outline>();
            o.effectColor = new Color(0.04f, 0.05f, 0.1f, 0.9f);
            o.effectDistance = new Vector2(2.5f, -2.5f);
            return t;
        }

        private static Button MakeButton(RectTransform parent, string name, Font font, Sprite sprite, Color color, string label,
            int fontSize, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = color;
            Button b = go.AddComponent<Button>();
            b.targetGraphic = img;
            ColorBlock cb = b.colors;
            cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            cb.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            cb.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.7f);
            b.colors = cb;
            Text t = MakeText(rt, "Label", font, fontSize, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            t.text = label;
            return b;
        }

        /// <summary>Small orange "AD" video badge in the corner of a rewarded-ad button.</summary>
        private static void MakeBadge(RectTransform button, Font font, Sprite sprite)
        {
            GameObject go = new GameObject("AdBadge", typeof(RectTransform));
            go.transform.SetParent(button, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(14, 14);
            rt.sizeDelta = new Vector2(84, 44);
            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = new Color(0.95f, 0.38f, 0.08f, 1f);
            img.raycastTarget = false;
            Text t = MakeText(rt, "AdText", font, 26, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(84, 44));
            t.text = "> AD";
        }

        private static GameObject MakePanel(RectTransform parent, string name, Sprite sprite)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image img = go.AddComponent<Image>();
            img.color = PanelColor; // full-screen dimmer; also blocks board input behind it
            return go;
        }
    }
}
