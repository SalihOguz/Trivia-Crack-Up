using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdmobManager : MonoBehaviour {
    private RewardBasedVideoAd rewardBasedVideo;

    public void Start()
    {
        this.rewardBasedVideo = RewardBasedVideoAd.Instance;
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        this.RequestRewardBasedVideo();
    }

    private void RequestRewardBasedVideo()
    {
        #if UNITY_ANDROID
            string adUnitId = "ca-app-pub-3940256099942544/5224354917";
        #elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/1712485313";
        #else
            string adUnitId = "unexpected_platform";
        #endif

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        this.rewardBasedVideo.LoadAd(request, adUnitId);
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        MonoBehaviour.print(
            "HandleRewardBasedVideoRewarded event received for "
                        + amount.ToString() + " " + type);
    }

    public void UserOptToWatchAd()
    {
        if (rewardBasedVideo.IsLoaded()) {
            rewardBasedVideo.Show();
        }
    }

    private void RequestRewardBasedVideoCoin50()
    {
        
        #if UNITY_ANDROID
            string adUnitId = "ca-app-pub-7734671340913331/9151687246";
        #elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-7734671340913331/3404590447";
        #else
            string adUnitId = "unexpected_platform";
        #endif

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        this.rewardBasedVideo.LoadAd(request, adUnitId);
    }
}