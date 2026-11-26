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
    /// HUD + win / fail panels on a 1080x1920 canvas (legacy uGUI Text, no TextMeshPro essentials needed).
    /// <see cref="BuildUI"/> creates the whole hierarchy; the Setup Main Scene tool calls it in the Editor and the
    /// scene saves the result. If the references are missing at runtime it builds itself.
    /// </summary>
    public sealed class UIManager : MonoBehaviour
    {
        public event Action RestartClicked;
        public event Action NextClicked;
        public event Action ContinueAdClicked;
        public event Action ExtraSlotClicked;

        [SerializeField] private Text levelText;
        [SerializeField] private Text movesText;
        [SerializeField] private Text slotsText;
        [SerializeField] private Text toastText;
        [SerializeField] private Button extraSlotButton;
        [SerializeField] private Text extraSlotButtonText;
        [SerializeField] private Button restartButton;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Text winTitle;
        [SerializeField] private Text winDetail;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button winRetryButton;
        [SerializeField] private GameObject failPanel;
        [SerializeField] private Text failTitle;
        [SerializeField] private Text failDetail;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text continueButtonText;
        [SerializeField] private Button failRetryButton;

        private float toastUntil;
        private float lastToastTime = -10f;
        private string lastToast;

        public bool IsBuilt { get { return levelText != null && winPanel != null && failPanel != null; } }

        private void Awake()
        {
            if (!IsBuilt) BuildUI(null);
            Wire();
        }

        private void Wire()
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(delegate { if (RestartClicked != null) RestartClicked(); });
            winRetryButton.onClick.RemoveAllListeners();
            winRetryButton.onClick.AddListener(delegate { if (RestartClicked != null) RestartClicked(); });
            failRetryButton.onClick.RemoveAllListeners();
            failRetryButton.onClick.AddListener(delegate { if (RestartClicked != null) RestartClicked(); });
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(delegate { if (NextClicked != null) NextClicked(); });
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(delegate { if (ContinueAdClicked != null) ContinueAdClicked(); });
            extraSlotButton.onClick.RemoveAllListeners();
            extraSlotButton.onClick.AddListener(delegate { if (ExtraSlotClicked != null) ExtraSlotClicked(); });
        }

        private void Update()
        {
            if (toastText != null && toastText.gameObject.activeSelf && Time.unscaledTime > toastUntil)
                toastText.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ public API

        public void SetLevel(int number, LevelTier tier)
        {
            string t = tier == LevelTier.ChapterBoss ? "  CHAPTER BOSS" : (tier == LevelTier.MiniBoss ? "  MINI BOSS" : "");
            levelText.text = "LEVEL " + number + t;
        }

        public void SetMoves(int left)
        {
            movesText.text = "MOVES " + left;
            movesText.color = left <= 2 ? new Color(1f, 0.45f, 0.45f) : Color.white;
        }

        public void SetSlots(int used, int max)
        {
            slotsText.text = "TRACK " + used + "/" + max;
            slotsText.color = used >= max ? new Color(1f, 0.45f, 0.45f) : Color.white;
        }

        public void SetExtraSlotButton(bool visible, bool interactable)
        {
            extraSlotButton.gameObject.SetActive(visible);
            extraSlotButton.interactable = interactable;
        }

        public void ShowToast(string message)
        {
            // de-bounce identical messages that are raised every frame while dragging
            if (message == lastToast && Time.unscaledTime - lastToastTime < 1.0f) return;
            lastToast = message;
            lastToastTime = Time.unscaledTime;
            toastText.text = message;
            toastText.gameObject.SetActive(true);
            toastUntil = Time.unscaledTime + 1.4f;
        }

        public void HidePanels()
        {
            winPanel.SetActive(false);
            failPanel.SetActive(false);
        }

        public void ShowWin(int stars, int movesUsed, int par, bool hasNext)
        {
            failPanel.SetActive(false);
            winPanel.SetActive(true);
            winTitle.text = "LEVEL CLEAR!";
            winDetail.text = "Stars: " + stars + " / 3\nMoves used: " + movesUsed + "   Par: " + par;
            nextButton.gameObject.SetActive(hasNext);
        }

        public void ShowFail(string reason, string detail, string continueLabel, bool canContinue)
        {
            winPanel.SetActive(false);
            failPanel.SetActive(true);
            failTitle.text = reason;
            failDetail.text = detail;
            continueButtonText.text = continueLabel;
            continueButton.gameObject.SetActive(canContinue);
        }

        // ------------------------------------------------------------------ construction

        private static readonly Color PanelColor = new Color(0.08f, 0.09f, 0.18f, 0.92f);
        private static readonly Color ButtonColor = new Color(0.30f, 0.55f, 0.98f, 1f);
        private static readonly Color ButtonAlt = new Color(0.45f, 0.48f, 0.62f, 1f);
        private static readonly Color ButtonAd = new Color(0.98f, 0.70f, 0.20f, 1f);

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

            // --- top HUD ---
            levelText = MakeText(root, "LevelText", font, 52, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -70), new Vector2(900, 80));
            movesText = MakeText(root, "MovesText", font, 40, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(50, -150), new Vector2(380, 70));
            slotsText = MakeText(root, "SlotsText", font, 40, TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-50, -150), new Vector2(380, 70));
            movesText.rectTransform.pivot = new Vector2(0f, 0.5f);
            slotsText.rectTransform.pivot = new Vector2(1f, 0.5f);

            restartButton = MakeButton(root, "RestartButton", font, sprite, ButtonAlt, "RESTART", 30, new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(200, 70), out _);

            // --- bottom: extra slot (rewarded) ---
            extraSlotButton = MakeButton(root, "ExtraSlotButton", font, sprite, ButtonAd, "+1 TRACK SLOT  (AD)", 36, new Vector2(0.5f, 0f), new Vector2(0, 70), new Vector2(560, 100), out extraSlotButtonText);

            // --- toast ---
            toastText = MakeText(root, "Toast", font, 46, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(980, 120));
            toastText.gameObject.SetActive(false);

            // --- win panel ---
            winPanel = MakePanel(root, "WinPanel", sprite);
            winTitle = MakeText(winPanel.GetComponent<RectTransform>(), "Title", font, 78, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 230), new Vector2(900, 120));
            winDetail = MakeText(winPanel.GetComponent<RectTransform>(), "Detail", font, 46, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(900, 200));
            nextButton = MakeButton(winPanel.GetComponent<RectTransform>(), "NextButton", font, sprite, ButtonColor, "NEXT LEVEL", 46, new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(560, 120), out _);
            winRetryButton = MakeButton(winPanel.GetComponent<RectTransform>(), "ReplayButton", font, sprite, ButtonAlt, "REPLAY", 38, new Vector2(0.5f, 0.5f), new Vector2(0, -270), new Vector2(400, 100), out _);
            winPanel.SetActive(false);

            // --- fail panel ---
            failPanel = MakePanel(root, "FailPanel", sprite);
            failTitle = MakeText(failPanel.GetComponent<RectTransform>(), "Title", font, 72, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 230), new Vector2(950, 120));
            failDetail = MakeText(failPanel.GetComponent<RectTransform>(), "Detail", font, 42, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(900, 200));
            continueButton = MakeButton(failPanel.GetComponent<RectTransform>(), "ContinueButton", font, sprite, ButtonAd, "CONTINUE (AD)", 42, new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(640, 120), out continueButtonText);
            failRetryButton = MakeButton(failPanel.GetComponent<RectTransform>(), "RetryButton", font, sprite, ButtonColor, "RETRY", 42, new Vector2(0.5f, 0.5f), new Vector2(0, -270), new Vector2(420, 110), out _);
            failPanel.SetActive(false);

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

        private static Text MakeText(RectTransform parent, string name, Font font, int size, TextAnchor anchor,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
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
            o.effectColor = new Color(0.05f, 0.06f, 0.14f, 0.9f);
            o.effectDistance = new Vector2(2.5f, -2.5f);
            return t;
        }

        private static Button MakeButton(RectTransform parent, string name, Font font, Sprite sprite, Color color, string label,
            int fontSize, Vector2 anchor, Vector2 pos, Vector2 size, out Text labelText)
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
            labelText = MakeText(rt, "Label", font, fontSize, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            labelText.text = label;
            return b;
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
