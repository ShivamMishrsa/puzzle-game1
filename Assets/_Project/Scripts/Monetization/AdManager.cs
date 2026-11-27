using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

namespace ColorGateBlast
{
    public enum RewardPlacement
    {
        /// <summary>Hint button: highlight the next block to move.</summary>
        Hint,
        /// <summary>"+1 Track Slot" button when the conveyor is at capacity.</summary>
        ExtraTrackSlot,
        /// <summary>Out-of-moves popup: +5 moves and resume.</summary>
        Revive,
        /// <summary>Pliers booster: snip one chosen piece off the tray.</summary>
        Pliers
    }

    /// <summary>
    /// Google AdMob wrapper (Rewarded + Interstitial).
    ///
    /// * Everything that touches GoogleMobileAds.Api is inside <c>#if GOOGLE_MOBILE_ADS</c>. Without that scripting
    ///   symbol an Editor / fallback SIMULATOR runs instead, so the project compiles and plays today.
    ///   (After importing the Google Mobile Ads Unity plugin run Tools > Color Gate Blast > AdMob > Enable.)
    /// * ADMOB POLICY: only Google's official TEST ad unit IDs are used during development. Production IDs are used
    ///   only when "Use Test Ids" is unticked AND the build is a non-development player build. Never tap live ads on
    ///   your own device - AdMob suspends accounts for self-clicks.
    /// </summary>
    public sealed class AdManager : MonoBehaviour
    {
        public const string TestRewardedId = "ca-app-pub-3940256099942544/5224354917";
        public const string TestInterstitialId = "ca-app-pub-3940256099942544/1033173712";
        /// <summary>Google's sample Android app id (for GoogleMobileAdsSettings).</summary>
        public const string TestAppId = "ca-app-pub-3940256099942544~3347511713";

        private const string NoAdsKey = "NoAds";
        private const string LastInterstitialLevelKey = "cgb_last_interstitial_level";

        public static AdManager Instance { get; private set; }

        [Header("AdMob ad units")]
        [Tooltip("Keep ticked until you generate the final signed release .aab. Production ids are ignored in the Editor and in development builds anyway.")]
        [SerializeField] private bool useTestIds = true;
        [SerializeField] private string productionRewardedId = "";
        [SerializeField] private string productionInterstitialId = "";

        [Header("Interstitial pacing")]
        public int MinLevelForInterstitial = 6;
        public int MinLevelsBetweenInterstitials = 2;
        public float InterstitialCooldownSeconds = 90f;

        [Header("Editor / fallback simulator")]
        public bool SimulatorGrantsRewards = true;
        public float SimulatedAdSeconds = 0.9f;

        public event Action<bool> RewardedAvailabilityChanged;

        private readonly Queue<Action> mainThread = new Queue<Action>();
        private float lastInterstitialTime = -9999f;
        private bool simShowing;
        private string simLabel = "";

        public string RewardedId { get { return UseTestIds ? TestRewardedId : productionRewardedId; } }
        public string InterstitialId { get { return UseTestIds ? TestInterstitialId : productionInterstitialId; } }

        /// <summary>True whenever test ids are in force (always in the Editor and in development builds).</summary>
        public bool UseTestIds
        {
            get { return useTestIds || Application.isEditor || Debug.isDebugBuild || string.IsNullOrEmpty(productionRewardedId) || string.IsNullOrEmpty(productionInterstitialId); }
        }

        public bool NoAds
        {
            get { return PlayerPrefs.GetInt(NoAdsKey, 0) == 1; }
        }

        public void SetNoAds(bool value)
        {
            PlayerPrefs.SetInt(NoAdsKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

#if GOOGLE_MOBILE_ADS
        private RewardedAd rewarded;
        private InterstitialAd interstitial;
        private bool sdkReady;
#endif

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (UseTestIds) Debug.Log("[AdManager] Using Google TEST ad unit ids. Never tap live ads on your own device.");
            Initialize();
        }

        public void Initialize()
        {
#if GOOGLE_MOBILE_ADS
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.Initialize(delegate (InitializationStatus status)
            {
                sdkReady = true;
                LoadRewarded();
                LoadInterstitial();
            });
#else
            Debug.Log("[AdManager] GOOGLE_MOBILE_ADS not defined: running the ad SIMULATOR (no real ads).");
#endif
        }

        private void Update()
        {
            while (mainThread.Count > 0)
            {
                Action a = mainThread.Dequeue();
                if (a != null) a();
            }
        }

        // ------------------------------------------------------------------ interstitial pacing

        /// <summary>
        /// True when an interstitial may be shown after completing <paramref name="completedLevel"/>:
        /// level &gt;= 6, at least 2 levels since the last one, 90 s cooldown elapsed, and the NoAds flag is not set.
        /// </summary>
        public bool CanShowInterstitial(int completedLevel)
        {
            if (NoAds) return false;
            if (completedLevel < MinLevelForInterstitial) return false;
            int last = PlayerPrefs.GetInt(LastInterstitialLevelKey, 0);
            if (completedLevel - last < MinLevelsBetweenInterstitials) return false;
            if (Time.realtimeSinceStartup - lastInterstitialTime < InterstitialCooldownSeconds) return false;
            return true;
        }

        public bool TryShowInterstitial(int completedLevel) { return TryShowInterstitial(completedLevel, null); }

        /// <summary>Shows an interstitial if the pacing rules allow it. <paramref name="onClosed"/> runs when it closes (not called when nothing was shown).</summary>
        public bool TryShowInterstitial(int completedLevel, Action onClosed)
        {
            if (!CanShowInterstitial(completedLevel)) return false;
            bool shown = ShowInterstitialInternal(onClosed);
            if (shown)
            {
                lastInterstitialTime = Time.realtimeSinceStartup;
                PlayerPrefs.SetInt(LastInterstitialLevelKey, completedLevel);
                PlayerPrefs.Save();
            }
            return shown;
        }

        private bool ShowInterstitialInternal(Action onClosed)
        {
#if GOOGLE_MOBILE_ADS
            if (interstitial != null && interstitial.CanShowAd())
            {
                InterstitialAd ad = interstitial;
                ad.OnAdFullScreenContentClosed += delegate ()
                {
                    ad.Destroy();
                    if (interstitial == ad) interstitial = null;
                    LoadInterstitial(); // preload the next one
                    if (onClosed != null) onClosed();
                };
                ad.OnAdFullScreenContentFailed += delegate (AdError error)
                {
                    Debug.LogWarning("[AdManager] Interstitial failed to show: " + error);
                    if (interstitial == ad) interstitial = null;
                    LoadInterstitial();
                    if (onClosed != null) onClosed();
                };
                ad.Show();
                return true;
            }
            LoadInterstitial();
            return false;
#else
            StartCoroutine(SimulateInterstitial(onClosed));
            return true;
#endif
        }

        // ------------------------------------------------------------------ rewarded

        public bool IsRewardedReady
        {
            get
            {
#if GOOGLE_MOBILE_ADS
                return rewarded != null && rewarded.CanShowAd();
#else
                return true;
#endif
            }
        }

        /// <summary>Shows a rewarded ad. <paramref name="onComplete"/> gets true only if the reward was earned.</summary>
        public void ShowRewarded(RewardPlacement placement, Action<bool> onComplete)
        {
#if GOOGLE_MOBILE_ADS
            if (rewarded == null || !rewarded.CanShowAd())
            {
                LoadRewarded();
                if (onComplete != null) onComplete(false);
                return;
            }
            RewardedAd ad = rewarded;
            bool earned = false;
            ad.OnAdFullScreenContentClosed += delegate ()
            {
                ad.Destroy();
                if (rewarded == ad) rewarded = null;
                LoadRewarded(); // automatically reload the next ad
                if (onComplete != null) onComplete(earned);
            };
            ad.OnAdFullScreenContentFailed += delegate (AdError error)
            {
                Debug.LogWarning("[AdManager] Rewarded failed to show: " + error);
                if (rewarded == ad) rewarded = null;
                LoadRewarded();
                if (onComplete != null) onComplete(false);
            };
            ad.Show(delegate (Reward reward) { earned = true; });
#else
            StartCoroutine(SimulateRewarded(placement, onComplete));
#endif
        }

#if GOOGLE_MOBILE_ADS
        private void LoadRewarded()
        {
            RewardedAd.Load(RewardedId, new AdRequest(), delegate (RewardedAd ad, LoadAdError error)
            {
                if (error != null || ad == null) { Debug.LogWarning("[AdManager] Rewarded load failed: " + error); rewarded = null; }
                else rewarded = ad;
                if (RewardedAvailabilityChanged != null) RewardedAvailabilityChanged(rewarded != null);
            });
        }

        private void LoadInterstitial()
        {
            InterstitialAd.Load(InterstitialId, new AdRequest(), delegate (InterstitialAd ad, LoadAdError error)
            {
                if (error != null || ad == null) { Debug.LogWarning("[AdManager] Interstitial load failed: " + error); interstitial = null; }
                else interstitial = ad;
            });
        }
#endif

        // ------------------------------------------------------------------ simulator (no SDK)

        private IEnumerator SimulateRewarded(RewardPlacement placement, Action<bool> onComplete)
        {
            simShowing = true;
            simLabel = "SIMULATED REWARDED AD (" + placement + ")";
            yield return new WaitForSecondsRealtime(SimulatedAdSeconds);
            simShowing = false;
            if (onComplete != null) onComplete(SimulatorGrantsRewards);
        }

        private IEnumerator SimulateInterstitial(Action onClosed)
        {
            simShowing = true;
            simLabel = "SIMULATED INTERSTITIAL AD";
            yield return new WaitForSecondsRealtime(SimulatedAdSeconds * 0.8f);
            simShowing = false;
            if (onClosed != null) onClosed();
        }

        private void OnGUI()
        {
            if (!simShowing) return;
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUIStyle st = new GUIStyle(GUI.skin.label);
            st.alignment = TextAnchor.MiddleCenter;
            st.fontSize = Mathf.Max(18, Screen.width / 24);
            st.normal.textColor = Color.white;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), simLabel + "\n(test mode - no real ad)", st);
        }
    }
}
