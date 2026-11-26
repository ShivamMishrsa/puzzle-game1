using System;
using System.Collections;
using UnityEngine;

namespace ColorGateBlast
{
    public enum RewardPlacement
    {
        /// <summary>HUD button: +1 conveyor track slot for the current level.</summary>
        ExtraTrackSlot,
        /// <summary>Fail screen: continue the level (+moves, or +1 slot when the track jammed).</summary>
        ContinueAfterFail
    }

    /// <summary>Everything the game needs from an ad network. Swap the implementation, not the callers.</summary>
    public interface IAdService
    {
        void Initialize();
        bool IsRewardedReady(RewardPlacement placement);
        /// <summary>Shows a rewarded ad; <paramref name="onComplete"/> receives true only if the reward was earned.</summary>
        void ShowRewarded(RewardPlacement placement, Action<bool> onComplete);
    }

    /// <summary>
    /// Development stand-in: pretends to show an ad for a moment, then grants the reward.
    /// TODO(AdMob): create GoogleMobileAdsService : IAdService that wraps RewardedAd.Load / Show from the
    /// Google Mobile Ads Unity plugin and call SetService(...) from Awake. Nothing else in the game changes.
    /// </summary>
    public sealed class StubAdService : IAdService
    {
        private readonly MonoBehaviour runner;

        public StubAdService(MonoBehaviour runner) { this.runner = runner; }

        public void Initialize() { Debug.Log("[Ads] Stub ad service initialised (no real ads)."); }

        public bool IsRewardedReady(RewardPlacement placement) { return true; }

        public void ShowRewarded(RewardPlacement placement, Action<bool> onComplete)
        {
            runner.StartCoroutine(Fake(placement, onComplete));
        }

        private IEnumerator Fake(RewardPlacement placement, Action<bool> onComplete)
        {
            Debug.Log("[Ads] (stub) showing rewarded ad for " + placement + " ...");
            yield return new WaitForSecondsRealtime(0.8f);
            Debug.Log("[Ads] (stub) reward granted.");
            if (onComplete != null) onComplete(true);
        }
    }

    /// <summary>
    /// Monetization facade: AdMob rewarded ads (extra track slot / continue on fail) plus an optional
    /// "Remove Ads" purchase. When ads are removed, rewards are granted instantly without watching anything.
    /// The IAP side is stubbed until Unity IAP / Google Play Billing is wired up.
    /// </summary>
    public sealed class MonetizationManager : MonoBehaviour
    {
        private const string AdsRemovedKey = "cgb_ads_removed";

        public static MonetizationManager Instance { get; private set; }

        private IAdService service;

        public bool AdsRemoved
        {
            get { return PlayerPrefs.GetInt(AdsRemovedKey, 0) == 1; }
            private set { PlayerPrefs.SetInt(AdsRemovedKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        private void Awake()
        {
            Instance = this;
            if (service == null) service = new StubAdService(this);
            service.Initialize();
        }

        public void SetService(IAdService s)
        {
            service = s;
            service.Initialize();
        }

        public bool IsRewardedAvailable(RewardPlacement placement)
        {
            return AdsRemoved || (service != null && service.IsRewardedReady(placement));
        }

        public void ShowRewarded(RewardPlacement placement, Action<bool> onComplete)
        {
            if (AdsRemoved)
            {
                if (onComplete != null) onComplete(true);
                return;
            }
            if (service == null || !service.IsRewardedReady(placement))
            {
                if (onComplete != null) onComplete(false);
                return;
            }
            service.ShowRewarded(placement, onComplete);
        }

        /// <summary>Stub purchase flow. Replace the body with Unity IAP / Play Billing and call the callback from its result.</summary>
        public void PurchaseRemoveAds(Action<bool> onComplete)
        {
            Debug.Log("[IAP] (stub) Remove Ads purchased.");
            AdsRemoved = true;
            if (onComplete != null) onComplete(true);
        }

        public void RestorePurchases(Action<bool> onComplete)
        {
            if (onComplete != null) onComplete(AdsRemoved);
        }
    }
}
