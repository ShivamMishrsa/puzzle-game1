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
        public event Action PliersClicked;

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
        [SerializeField] private Button pliersButton;
        private Image pliersImage;
        private RectTransform pipRow;
        private Image[] pipLit = new Image[0];
        private int pipCount = -1;
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

        private Text timerText;
        private Image clockRing;
        private Outline movesWarning;
        public MapScreen Map { get; private set; }
        public event Action MapClicked;
        private float toastUntil;
        private float lastToastTime = -10f;
        private string lastToast;

        public bool IsBuilt { get { return levelText != null && winPanel != null && losePanel != null && hintButton != null && extraSlotButton != null; } }

        private void Awake()
        {
            BuildUI(null); // the industrial HUD is generated from code at runtime (HudSkin sprites are not serializable)
            Wire();
            // every button gets a tactile click
            Button[] all = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < all.Length; i++)
                all[i].onClick.AddListener(delegate { if (SoundSynth.Instance != null) SoundSynth.Instance.Play(Sfx.Click, 1f, 0.7f); });
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
            pliersButton.onClick.RemoveAllListeners();
            pliersButton.onClick.AddListener(delegate { if (PliersClicked != null) PliersClicked(); });
        }

        private void Update()
        {
            if (movesWarning != null && movesWarning.enabled)
            {
                Color wc = movesWarning.effectColor;
                wc.a = 0.35f + 0.55f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 7f));
                movesWarning.effectColor = wc;
            }
            if (toastText != null && toastText.gameObject.activeSelf && Time.unscaledTime > toastUntil)
                toastText.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ public API

        public void SetLevel(int number, string title, LevelTier tier)
        {
            levelText.text = "LEVEL " + number;
            bool boss = tier != LevelTier.Normal;
            levelText.rectTransform.anchoredPosition = new Vector2(0f, boss ? -46f : -62f);
            levelText.fontSize = boss ? 70 : 80;
            bannerRoot.SetActive(boss);
            if (boss)
            {
                bannerText.text = tier == LevelTier.ChapterBoss ? "CHAPTER BOSS  -  " + title.ToUpper() : "MINI-BOSS  -  " + title.ToUpper();
                bannerImage.color = tier == LevelTier.ChapterBoss ? new Color(0.78f, 0.12f, 0.14f, 0.95f) : new Color(0.95f, 0.72f, 0.12f, 0.95f);
                bannerText.color = tier == LevelTier.ChapterBoss ? new Color(1f, 0.9f, 0.45f) : new Color(0.45f, 0.08f, 0.08f);
            }
        }

        /// <summary>Stopwatch between the two plaques: a brass-rimmed clock face inside a ring that drains from green to red. seconds &lt; 0 hides it.</summary>
        public void SetTime(float seconds, float total)
        {
            if (timerText == null) return;
            GameObject clock = timerText.transform.parent.parent.gameObject;
            clock.SetActive(seconds >= 0f);
            if (seconds < 0f) return;
            int sec = Mathf.CeilToInt(seconds);
            timerText.text = (sec / 60) + ":" + (sec % 60).ToString("00");
            float k = total > 0f ? Mathf.Clamp01(seconds / total) : 1f;
            clockRing.fillAmount = k;
            Color ring = k > 0.5f ? Color.Lerp(new Color(0.98f, 0.8f, 0.2f), new Color(0.3f, 0.85f, 0.4f), (k - 0.5f) * 2f) : Color.Lerp(new Color(0.9f, 0.15f, 0.1f), new Color(0.98f, 0.8f, 0.2f), k * 2f);
            clockRing.color = ring;
            bool low = sec <= 10;
            timerText.color = low ? new Color(0.8f, 0.05f, 0.05f) : PlaqueText;
            float pulse = low ? 1f + 0.07f * Mathf.Sin(Time.unscaledTime * 10f) : 1f;
            clock.transform.localScale = Vector3.one * pulse;
        }

        public void SetTargets(int remaining) { targetsText.text = "REMAINING: " + remaining; }

        public void SetMoves(int left)
        {
            movesText.text = "MOVES: " + left;
            movesText.color = left <= 3 ? new Color(1f, 0.32f, 0.2f) : NixieAmber;
            if (movesWarning != null) movesWarning.enabled = left <= 3;
        }

        /// <summary>Track Capacity label + recessed LED pips (amber when occupied, dark socket when empty).</summary>
        public void SetTrack(int used, int max, bool unlimited)
        {
            trackText.text = unlimited ? "Track Capacity: " + used + "/INF" : "Track Capacity: " + used + "/" + max;
            trackText.color = (!unlimited && used >= max) ? new Color(1f, 0.62f, 0.45f) : CreamText;
            int want = unlimited ? 0 : max;
            if (want != pipCount) RebuildPips(want);
            for (int i = 0; i < pipLit.Length; i++) pipLit[i].enabled = i < used;
        }

        private void RebuildPips(int count)
        {
            pipCount = count;
            for (int i = pipRow.childCount - 1; i >= 0; i--) Destroy(pipRow.GetChild(i).gameObject);
            pipLit = new Image[count];
            for (int i = 0; i < count; i++)
            {
                GameObject sock = new GameObject("LedSocket" + i, typeof(RectTransform));
                sock.transform.SetParent(pipRow, false);
                RectTransform rt = sock.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = new Vector2(i * 62f, 0f);
                rt.sizeDelta = new Vector2(52f, 52f);
                Image si = sock.AddComponent<Image>();
                si.sprite = HudSkin.LedSocket;
                si.raycastTarget = false;
                GameObject led = new GameObject("Led", typeof(RectTransform));
                led.transform.SetParent(rt, false);
                RectTransform lr = led.GetComponent<RectTransform>();
                lr.anchorMin = lr.anchorMax = new Vector2(0.5f, 0.5f);
                lr.sizeDelta = new Vector2(92f, 92f);
                Image li = led.AddComponent<Image>();
                li.sprite = HudSkin.LedLit;
                li.raycastTarget = false;
                li.enabled = false;
                pipLit[i] = li;
            }
        }

        /// <summary>Pliers booster armed: the medallion glows gold until a piece is snipped or it is tapped again.</summary>
        public void SetPliersArmed(bool armed)
        {
            if (pliersImage == null) return;
            pliersImage.color = armed ? new Color(1f, 0.82f, 0.35f) : Color.white;
            pliersImage.rectTransform.localScale = Vector3.one * (armed ? 1.12f : 1f);
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
            bool centennial = !string.IsNullOrEmpty(reward) && reward.StartsWith("CENTENNIAL");
            winTitle.text = centennial ? "CENTENNIAL MILESTONE!" : "LEVEL CLEAR!";
            winTitle.fontSize = centennial ? 62 : 78;
            winTitle.color = centennial ? new Color(1f, 0.85f, 0.3f) : Color.white;
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

        private static readonly Color PlaqueText = new Color(0.1f, 0.1f, 0.12f);
        private static readonly Color NixieAmber = new Color(1f, 0.74f, 0.25f);
        private static readonly Color CreamText = new Color(0.96f, 0.9f, 0.74f);
        private Image hintImage;
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
            // --- header: embossed gold-beige LEVEL, then two riveted steel plaques with coloured badges ---
            levelText = MakeText(root, "LevelText", font, 80, TextAnchor.MiddleCenter, topC, new Vector2(0, -62), new Vector2(900, 100));
            levelText.color = new Color(0.95f, 0.86f, 0.62f);
            Outline lo = levelText.GetComponent<Outline>();
            lo.effectColor = new Color(0.32f, 0.2f, 0.06f, 0.9f);
            lo.effectDistance = new Vector2(2f, -2f);
            Shadow ls = levelText.gameObject.AddComponent<Shadow>();
            ls.effectColor = new Color(0f, 0f, 0f, 0.75f);
            ls.effectDistance = new Vector2(0f, -8f);

            GameObject banner = new GameObject("BossBanner", typeof(RectTransform));
            banner.transform.SetParent(root, false);
            RectTransform brt = banner.GetComponent<RectTransform>();
            brt.anchorMin = topC; brt.anchorMax = topC; brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = new Vector2(0, -112); brt.sizeDelta = new Vector2(640, 40);
            bannerImage = banner.AddComponent<Image>();
            bannerImage.sprite = HudSkin.Plate;
            bannerImage.type = Image.Type.Sliced;
            bannerRoot = banner;
            bannerText = MakeText(brt, "BannerText", font, 26, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620, 40));
            banner.SetActive(false);

            movesText = MakePlaque(root, "MovesPlaque", font, new Vector2(0f, 1f), new Vector2(240, -190), new Vector2(430, 108), new Color(0.78f, 0.16f, 0.12f));
            Image movesPlate = movesText.transform.parent.GetComponent<Image>();
            movesWarning = movesPlate.gameObject.AddComponent<Outline>();
            movesWarning.effectColor = new Color(1f, 0.1f, 0.05f, 0.9f);
            movesWarning.effectDistance = new Vector2(7f, -7f);
            movesWarning.enabled = false;
            targetsText = MakePlaque(root, "RemainingPlaque", font, new Vector2(1f, 1f), new Vector2(-240, -190), new Vector2(430, 108), new Color(0.2f, 0.45f, 0.85f));
            targetsText.fontSize = 32;

            // --- stopwatch between the plaques ---
            GameObject tp = new GameObject("Stopwatch", typeof(RectTransform));
            tp.transform.SetParent(root, false);
            RectTransform tpr = tp.GetComponent<RectTransform>();
            tpr.anchorMin = tpr.anchorMax = topC; tpr.pivot = new Vector2(0.5f, 0.5f);
            tpr.anchoredPosition = new Vector2(0, -192); tpr.sizeDelta = new Vector2(138, 138);
            Image track = tp.AddComponent<Image>();
            track.sprite = HudSkin.Ring; track.color = new Color(0.12f, 0.1f, 0.1f, 0.85f); track.raycastTarget = false;
            GameObject fill = new GameObject("Arc", typeof(RectTransform));
            fill.transform.SetParent(tpr, false);
            RectTransform fr = fill.GetComponent<RectTransform>();
            fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.offsetMin = Vector2.zero; fr.offsetMax = Vector2.zero;
            clockRing = fill.AddComponent<Image>();
            clockRing.sprite = HudSkin.Ring; clockRing.type = Image.Type.Filled; clockRing.fillMethod = Image.FillMethod.Radial360;
            clockRing.fillOrigin = (int)Image.Origin360.Top; clockRing.fillClockwise = true; clockRing.fillAmount = 1f; clockRing.raycastTarget = false;
            GameObject crown = new GameObject("Crown", typeof(RectTransform));
            crown.transform.SetParent(tpr, false);
            RectTransform cr = crown.GetComponent<RectTransform>();
            cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 1f); cr.sizeDelta = new Vector2(34, 22); cr.anchoredPosition = new Vector2(0, 8);
            Image ci = crown.AddComponent<Image>(); ci.sprite = HudSkin.Plate; ci.type = Image.Type.Sliced; ci.color = new Color(1f, 0.82f, 0.4f); ci.raycastTarget = false;
            GameObject face = new GameObject("Face", typeof(RectTransform));
            face.transform.SetParent(tpr, false);
            RectTransform fcr = face.GetComponent<RectTransform>();
            fcr.anchorMin = fcr.anchorMax = new Vector2(0.5f, 0.5f); fcr.sizeDelta = new Vector2(106, 106);
            Image fci = face.AddComponent<Image>(); fci.sprite = HudSkin.ClockFace; fci.raycastTarget = false;
            timerText = MakeText(fcr, "TimerText", font, 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(100, 50));
            timerText.color = PlaqueText;
            timerText.GetComponent<Outline>().enabled = false;
            tp.SetActive(false);

            // --- back to the story map ---
            Button mapBtn = MakeButton(root, "MapButton", font, null, new Color(1f, 0.85f, 0.7f), "MAP", 28, new Vector2(0f, 1f), new Vector2(80, -46), new Vector2(110, 58));
            mapBtn.GetComponentInChildren<Text>().color = PlaqueText;
            mapBtn.GetComponentInChildren<Outline>().enabled = false;
            mapBtn.onClick.AddListener(delegate { if (MapClicked != null) MapClicked(); });

            // --- bottom row: track capacity LEDs (left), Restart plaque (centre), booster medallions (right) ---
            trackText = MakeText(root, "TrackText", font, 34, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(44, 178), new Vector2(520, 50));
            trackText.rectTransform.pivot = new Vector2(0f, 0.5f);
            trackText.rectTransform.anchoredPosition = new Vector2(44, 178);
            trackText.color = CreamText;
            Shadow ts = trackText.gameObject.AddComponent<Shadow>();
            ts.effectColor = new Color(0f, 0f, 0f, 0.8f);
            ts.effectDistance = new Vector2(0f, -4f);
            GameObject row = new GameObject("PipRow", typeof(RectTransform));
            row.transform.SetParent(root, false);
            pipRow = row.GetComponent<RectTransform>();
            pipRow.anchorMin = pipRow.anchorMax = new Vector2(0f, 0f);
            pipRow.pivot = new Vector2(0f, 0.5f);
            pipRow.anchoredPosition = new Vector2(44, 108);
            pipRow.sizeDelta = new Vector2(380, 56);
            pipCount = -1;
            pipLit = new Image[0];

            restartButton = MakeButton(root, "RestartButton", font, null, Color.white, "Restart", 38, new Vector2(0.5f, 0f), new Vector2(-20, 108), new Vector2(270, 100));
            restartButton.GetComponentInChildren<Text>().color = new Color(0.1f, 0.1f, 0.12f);
            Shadow rs = restartButton.GetComponentInChildren<Text>().gameObject.AddComponent<Shadow>();
            rs.effectColor = new Color(1f, 1f, 1f, 0.45f);
            rs.effectDistance = new Vector2(0f, -2f);
            restartButton.GetComponentInChildren<Outline>().enabled = false;

            pliersButton = MakeMedallion(root, "PliersButton", HudSkin.Pliers, new Vector2(1f, 0f), new Vector2(-270, 112), out pliersImage);
            hintButton = MakeMedallion(root, "HintButton", HudSkin.Gear, new Vector2(1f, 0f), new Vector2(-100, 112), out hintImage);

            // --- rescue: +1 track slot (only while at capacity) ---
            extraSlotButton = MakeButton(root, "ExtraSlotButton", font, null, new Color(1f, 0.82f, 0.4f), "+1 TRACK SLOT", 36, new Vector2(0.5f, 0f), new Vector2(0, 262), new Vector2(560, 100));
            MakeBadge(extraSlotButton.GetComponent<RectTransform>(), font, null);
            extraSlotButton.gameObject.SetActive(false);

            // --- 360 rotate hint (shown next to the monument) ---
            GameObject rot = new GameObject("RotateHint", typeof(RectTransform));
            rot.transform.SetParent(root, false);
            RectTransform rr = rot.GetComponent<RectTransform>();
            rr.anchorMin = new Vector2(1f, 0f); rr.anchorMax = new Vector2(1f, 0f); rr.pivot = new Vector2(0.5f, 0.5f);
            rr.anchoredPosition = new Vector2(-388, 108); rr.sizeDelta = new Vector2(76, 76);
            Image ri = rot.AddComponent<Image>();
            ri.sprite = HudSkin.Plate; ri.type = Image.Type.Sliced;
            ri.color = new Color(1f, 1f, 1f, 0.8f);
            ri.raycastTarget = false;
            Text rt = MakeText(rr, "Label", font, 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(76, 76));
            rt.text = "360\n<->\nDRAG";
            rt.color = PlaqueText;
            rt.GetComponent<Outline>().enabled = false;
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
            Button winMap = MakeButton(wr, "WinMapButton", font, sprite, ButtonGold, "MAP", 38, new Vector2(0.5f, 0.5f), new Vector2(0, -450), new Vector2(400, 100));
            winMap.onClick.AddListener(delegate { if (MapClicked != null) MapClicked(); });
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
            Button loseMap = MakeButton(lr, "LoseMapButton", font, sprite, ButtonGold, "MAP", 38, new Vector2(0.5f, 0.5f), new Vector2(0, -440), new Vector2(400, 100));
            loseMap.onClick.AddListener(delegate { if (MapClicked != null) MapClicked(); });
            losePanel.SetActive(false);

            Map = canvasGo.AddComponent<MapScreen>();
            Map.Build(root, font);
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
            img.sprite = HudSkin.Plate;
            img.type = Image.Type.Sliced;
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

        /// <summary>Riveted brushed-steel plaque with a coloured round badge on the left and the text to its right.</summary>
        private static Text MakePlaque(RectTransform parent, string name, Font font, Vector2 anchor, Vector2 pos, Vector2 size, Color badge)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            Image img = go.AddComponent<Image>();
            img.sprite = HudSkin.Plate; img.type = Image.Type.Sliced; img.raycastTarget = false;

            GameObject bd = new GameObject("Badge", typeof(RectTransform));
            bd.transform.SetParent(rt, false);
            RectTransform br = bd.GetComponent<RectTransform>();
            br.anchorMin = br.anchorMax = new Vector2(0f, 0.5f);
            br.pivot = new Vector2(0.5f, 0.5f);
            br.anchoredPosition = new Vector2(62f, 0f);
            br.sizeDelta = new Vector2(62f, 62f);
            Image bi = bd.AddComponent<Image>();
            bi.sprite = HudSkin.Disc; bi.color = badge; bi.raycastTarget = false;
            GameObject sheen = new GameObject("BadgeRing", typeof(RectTransform));
            sheen.transform.SetParent(br, false);
            RectTransform sr = sheen.GetComponent<RectTransform>();
            sr.anchorMin = sr.anchorMax = new Vector2(0.5f, 0.5f);
            sr.sizeDelta = new Vector2(30f, 30f);
            Image si = sheen.AddComponent<Image>();
            si.sprite = HudSkin.Disc; si.color = new Color(1f, 1f, 1f, 0.35f); si.raycastTarget = false;

            // recessed dark tube window with warm amber (Nixie / incandescent) numerals
            GameObject win = new GameObject("TubeWindow", typeof(RectTransform));
            win.transform.SetParent(rt, false);
            RectTransform wr = win.GetComponent<RectTransform>();
            wr.anchorMin = wr.anchorMax = new Vector2(0.5f, 0.5f);
            wr.anchoredPosition = new Vector2(52f, 0f); wr.sizeDelta = new Vector2(290f, 70f);
            Image wi = win.AddComponent<Image>();
            wi.sprite = HudSkin.Plate; wi.type = Image.Type.Sliced; wi.color = new Color(0.07f, 0.05f, 0.04f, 0.96f); wi.raycastTarget = false;
            Text t = MakeText(wr, "Text", font, 38, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 80));
            t.color = NixieAmber;
            Outline glow = t.GetComponent<Outline>();
            glow.effectColor = new Color(1f, 0.45f, 0.05f, 0.55f);
            glow.effectDistance = new Vector2(2.5f, -2.5f);
            return t;
        }

        /// <summary>Round metallic medallion with an icon and a padlock badge (the booster needs a rewarded ad to unlock).</summary>
        private static Button MakeMedallion(RectTransform parent, string name, Sprite icon, Vector2 anchor, Vector2 pos, out Image medalImage)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(150f, 150f);
            Image img = go.AddComponent<Image>();
            img.sprite = HudSkin.Medallion;
            medalImage = img;
            Button b = go.AddComponent<Button>();
            b.targetGraphic = img;
            ColorBlock cb = b.colors;
            cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            cb.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            cb.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.7f);
            b.colors = cb;

            GameObject ic = new GameObject("Icon", typeof(RectTransform));
            ic.transform.SetParent(rt, false);
            RectTransform ir = ic.GetComponent<RectTransform>();
            ir.anchorMin = ir.anchorMax = new Vector2(0.5f, 0.5f);
            ir.sizeDelta = new Vector2(92f, 92f);
            Image ii = ic.AddComponent<Image>();
            ii.sprite = icon; ii.raycastTarget = false;

            GameObject lk = new GameObject("PadlockBadge", typeof(RectTransform));
            lk.transform.SetParent(rt, false);
            RectTransform lr = lk.GetComponent<RectTransform>();
            lr.anchorMin = lr.anchorMax = new Vector2(1f, 1f);
            lr.pivot = new Vector2(0.5f, 0.5f);
            lr.anchoredPosition = new Vector2(-14f, -14f);
            lr.sizeDelta = new Vector2(54f, 54f);
            Image lbg = lk.AddComponent<Image>();
            lbg.sprite = HudSkin.Disc; lbg.color = new Color(0.16f, 0.17f, 0.2f); lbg.raycastTarget = false;
            GameObject lp = new GameObject("Lock", typeof(RectTransform));
            lp.transform.SetParent(lr, false);
            RectTransform lpr = lp.GetComponent<RectTransform>();
            lpr.anchorMin = lpr.anchorMax = new Vector2(0.5f, 0.5f);
            lpr.sizeDelta = new Vector2(34f, 34f);
            Image li = lp.AddComponent<Image>();
            li.sprite = HudSkin.Padlock; li.color = new Color(1f, 0.8f, 0.3f); li.raycastTarget = false;
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
            img.sprite = HudSkin.Plate;
            img.type = Image.Type.Sliced;
            img.color = new Color(1f, 0.5f, 0.2f, 1f);
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
