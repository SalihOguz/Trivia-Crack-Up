using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdmobManager : MonoBehaviour {
    private RewardBasedVideoAd rewardBasedVideo;

    public void Start()
    {
        #if UNITY_ANDROID
			string appId = "ca-app-pub-7734671340913331~2255058673";
		#elif UNITY_IPHONE
			string appId = "ca-app-pub-7734671340913331~7160002623";
        #else
            string appId = "unexpected_platform";
        #endif

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);

        // Get singleton reward based video ad reference.
        this.rewardBasedVideo = RewardBasedVideoAd.Instance;

        this.RequestRewardBasedVideo();
    }

    private void RequestRewardBasedVideo()
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