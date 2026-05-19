using System;
using UnityEngine;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour
{
    public static AdManager instance;

    [Header("Settings")]
    public bool isTesting = true;

    [Header("Real Ad Unit IDs")]
    public string realBannerId = "ca-app-pub-1369779382804350/2985722182";
    public string realInterstitialId = "ca-app-pub-1369779382804350/2435057847";
    public string realRewardedId = "ca-app-pub-3940256099942544/5224354917"; // Placeholder, update when you have real rewarded ID

    [Header("Test Ad Unit IDs")]
    private const string TestBannerId = "ca-app-pub-3940256099942544/6300978111";
    private const string TestInterstitialId = "ca-app-pub-3940256099942544/1033173712";
    private const string TestRewardedId = "ca-app-pub-3940256099942544/5224354917";

    private string BannerAdUnitId => isTesting ? TestBannerId : realBannerId;
    private string InterstitialAdUnitId => isTesting ? TestInterstitialId : realInterstitialId;
    private string RewardedAdUnitId => isTesting ? TestRewardedId : realRewardedId;

    private BannerView _bannerView;
    private InterstitialAd _interstitialAd;
    private RewardedAd _rewardedAd;

    private int _gameOverCount = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeAuto()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("AdManager (Auto)");
            go.AddComponent<AdManager>();
        }
    }

    void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus status) =>
        {
            Debug.Log("AdMob Initialized.");
            LoadInterstitialAd();
            LoadRewardedAd();
        });
    }

    #region Banner Ads

    public void ShowBanner()
    {
        if (_bannerView == null)
        {
            CreateBannerView();
        }

        Debug.Log("Showing Banner Ad.");
        _bannerView.Show();
    }

    public void HideBanner()
    {
        if (_bannerView != null)
        {
            Debug.Log("Hiding Banner Ad.");
            _bannerView.Hide();
        }
    }

    private void CreateBannerView()
    {
        Debug.Log("Creating banner view.");

        // If we already have a banner, destroy it first.
        if (_bannerView != null)
        {
            _bannerView.Destroy();
        }

        // Create a 320x50 banner at top of the screen
        _bannerView = new BannerView(BannerAdUnitId, AdSize.Banner, AdPosition.Bottom);

        // Load the ad with the request.
        AdRequest adRequest = new AdRequest();
        _bannerView.LoadAd(adRequest);
    }

    #endregion

    #region Interstitial Ads

    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(InterstitialAdUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load with error: " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
                _interstitialAd = ad;

                // Register to events the interstitial ad can raise.
                RegisterEventHandlers(ad);
            });
    }

    public void ShowInterstitial()
    {
        _gameOverCount++;
        Debug.Log("Game Over Count: " + _gameOverCount);

        if (_gameOverCount % 2 != 0)
        {
            Debug.Log("Skipping interstitial (shows every 2nd game over).");
            return;
        }

        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
            LoadInterstitialAd();
        }
    }

    private void RegisterEventHandlers(InterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) => { };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () => { };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () => { };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () => { };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad closed.");
            LoadInterstitialAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content with error: " + error);
            LoadInterstitialAd();
        };
    }

    #endregion

    #region Rewarded Ads

    public void LoadRewardedAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedAd.Load(RewardedAdUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load with error: " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
                _rewardedAd = ad;

                RegisterEventHandlers(ad);
            });
    }

    public void ShowRewardedAd(Action<Reward> onRewardEarned)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                Debug.Log("Rewarded ad granted reward: " + reward.Amount);
                onRewardEarned?.Invoke(reward);
            });
        }
        else
        {
            Debug.LogError("Rewarded ad is not ready yet.");
            LoadRewardedAd();
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) => { };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () => { };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () => { };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () => { };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad closed.");
            LoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content with error: " + error);
            LoadRewardedAd();
        };
    }

    #endregion
}
