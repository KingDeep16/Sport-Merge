using UnityEngine;
using Unity.Services.LevelPlay; // THE NEW MAGIC NAMESPACE!
using System;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    [Header("LevelPlay App Key")]
    public string androidAppKey = "25d9df44d";

    [Header("Ad Unit IDs")]
    [Tooltip("Leave these as default unless you created specific placements on the dashboard")]
    public string bannerAdUnitId = "lvvnp4758yklxagv";
    public string interstitialAdUnitId = "z7ze88z6ffavuw8u";
    public string rewardedAdUnitId = "7e2d2syqcjighgnx";

    [Header("Interstitial Settings")]
    public float timeBetweenAds = 180f; // 180 seconds = 3 minutes
    private float adTimer;

    // The new modern Ad Objects
    private LevelPlayBannerAd bannerAd;
    private LevelPlayInterstitialAd interstitialAd;
    private LevelPlayRewardedAd rewardedVideoAd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        Debug.Log("Starting LevelPlay Initialization...");

        // 1. Register for Success/Fail callbacks BEFORE initializing
        LevelPlay.OnInitSuccess += OnInitializationComplete;
        LevelPlay.OnInitFailed += OnInitializationFailed;

        // 2. Initialize the SDK
        LevelPlay.Init(androidAppKey);

        // Start your 3-minute timer
        adTimer = timeBetweenAds;
    }

    private void OnInitializationComplete(LevelPlayConfiguration config)
    {
        Debug.Log("LevelPlay Initialized Successfully! Loading ads...");

        // --- SETUP REWARDED AD ---
        rewardedVideoAd = new LevelPlayRewardedAd(rewardedAdUnitId);
        rewardedVideoAd.OnAdRewarded += GrantReviveReward; // The magic revive trigger
        rewardedVideoAd.OnAdClosed += (adInfo) => rewardedVideoAd.LoadAd(); // Load the next one when closed
        rewardedVideoAd.LoadAd(); // Load the first one

        // --- SETUP INTERSTITIAL AD ---
        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);
        interstitialAd.OnAdClosed += (adInfo) => interstitialAd.LoadAd(); // Load the next one when closed
        interstitialAd.LoadAd(); // Load the first one

        // --- SETUP BANNER AD ---
        bannerAd = new LevelPlayBannerAd(bannerAdUnitId);
        bannerAd.LoadAd();
    }

    private void OnInitializationFailed(LevelPlayInitError error)
    {
        Debug.Log($"LevelPlay Initialization Failed: {error}");
    }

    private void Update()
    {
        // Manage the 3-minute Interstitial Timer
        adTimer -= Time.deltaTime;
        if (adTimer <= 0)
        {
            ShowInterstitialAd();
            adTimer = timeBetweenAds; // Reset timer
        }
    }

    // --- AD TRIGGER METHODS (Call these from your buttons/game loop) ---

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.IsAdReady())
        {
            Debug.Log("Showing Interstitial Ad...");
            interstitialAd.ShowAd();
        }
    }

    public void ShowRewardedAd()
    {
        if (rewardedVideoAd != null && rewardedVideoAd.IsAdReady())
        {
            Debug.Log("Showing Rewarded Ad...");
            rewardedVideoAd.ShowAd();
        }
        else
        {
            Debug.Log("Rewarded ad is not ready yet! Trying to force a load...");
            rewardedVideoAd?.LoadAd();
        }
    }

    // --- REWARD LOGIC ---

    private void GrantReviveReward(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log("Player watched the ad! Granting revive reward.");
        RevivePlayer();
    }

    private void RevivePlayer()
    {
        // PUT YOUR REVIVE LOGIC HERE
        Debug.Log("DESTROY 10 BALLS AND RESUME GAME!");
    }

    private void OnDisable()
    {
        // Clean up memory when the game closes
        bannerAd?.DestroyAd();
        interstitialAd?.DestroyAd();
    }
}