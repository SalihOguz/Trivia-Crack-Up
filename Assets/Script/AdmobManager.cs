using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using System;

public class AdmobManager : Singleton<AdmobManager> {
    private RewardBasedVideoAd rewardBasedVideo;
    MenuManager menuManager;
    string adType = "";

    public void DelayedStart()
    {
        AdmobStart();

         // Get singleton reward based video ad reference.
        this.rewardBasedVideo = RewardBasedVideoAd.Instance;

        // Called when an ad request has successfully loaded.
        rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
        // Called when an ad request failed to load.
        rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
        // Called when an ad is shown.
        rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
        // Called when the ad starts to play.
        rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
        // Called when the user should be rewarded for watching a video.
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        // Called when the ad is closed.
        rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        // Called when the ad click caused the user to leave the application.
        rewardBasedVideo.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;

        this.RequestRewardBasedVideo();
    }

    public void AdmobStart()
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
    }

    public void RequestRewardBasedVideo()
    {
        // fake ad ids
        // #if UNITY_ANDROID
        //     string adUnitId = "ca-app-pub-3940256099942544/5224354917";
        // #elif UNITY_IPHONE
        //     string adUnitId = "ca-app-pub-3940256099942544/1712485313";
        // #else
        //     string adUnitId = "unexpected_platform";
        // #endif

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

    public void ShowAd(string type)
    {
        this.adType = type;
        if (rewardBasedVideo.IsLoaded()) {
            rewardBasedVideo.Show();
        }
    }

    void SendUserData()
    {
        PlayerPrefs.SetString("userData", JsonUtility.ToJson(menuManager.player));
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
		reference.Child("userList").Child(menuManager.player.userId).SetRawJsonValueAsync(PlayerPrefs.GetString("userData"));
    }

    private void OnDestroy() {
        if (rewardBasedVideo != null) {
            // Called when an ad request has successfully loaded.
            rewardBasedVideo.OnAdLoaded -= HandleRewardBasedVideoLoaded;
            // Called when an ad request failed to load.
            rewardBasedVideo.OnAdFailedToLoad -= HandleRewardBasedVideoFailedToLoad;
            // Called when an ad is shown.
            rewardBasedVideo.OnAdOpening -= HandleRewardBasedVideoOpened;
            // Called when the ad starts to play.
            rewardBasedVideo.OnAdStarted -= HandleRewardBasedVideoStarted;
            // Called when the user should be rewarded for watching a video.
            rewardBasedVideo.OnAdRewarded -= HandleRewardBasedVideoRewarded;
            // Called when the ad is closed.
            rewardBasedVideo.OnAdClosed -= HandleRewardBasedVideoClosed;
            // Called when the ad click caused the user to leave the application.
            rewardBasedVideo.OnAdLeavingApplication -= HandleRewardBasedVideoLeftApplication;
        }
    }

    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args) {
        MonoBehaviour.print("HandleRewardBasedVideoLoaded event received");
    }

    public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args) {
        MonoBehaviour.print(
            "HandleRewardBasedVideoFailedToLoad event received with message: "
                             + args.Message);

        this.RequestRewardBasedVideo();
    }

    public void HandleRewardBasedVideoOpened(object sender, EventArgs args) {
        MonoBehaviour.print("HandleRewardBasedVideoOpened event received");
    }

    public void HandleRewardBasedVideoStarted(object sender, EventArgs args) {
        MonoBehaviour.print("HandleRewardBasedVideoStarted event received");
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args) {
        MonoBehaviour.print("HandleRewardBasedVideoClosed event received");
        this.RequestRewardBasedVideo();
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args) {
        MonoBehaviour.print(
            "HandleRewardBasedVideoRewarded event received for "
                + args.Amount.ToString() + " " + args.Type);

        if(!menuManager)
        {
            menuManager = Camera.main.GetComponent<MenuManager>();
        }

        if(adType == "coin") 
        {
            menuManager.player.totalCoin += 50;
            menuManager.userDataObject.transform.Find("CoinButton").GetChild(0).GetComponent<Text>().text = menuManager.player.totalCoin.ToString();
        
        } 
        else if(adType == "joker")
        {
            menuManager.player.knowQuestionSkillCount += 1;
            menuManager.userDataObject.transform.Find("JokerButton").GetChild(0).GetComponent<Text>().text = "Joker " + menuManager.player.knowQuestionSkillCount.ToString();
            menuManager.knowQuestionCountText.text = menuManager.player.knowQuestionSkillCount.ToString();
        
        }
        else if (adType == "disable")
        {
            menuManager.player.fiftyFiftySkillCount += 1;
            menuManager.userDataObject.transform.Find("DisableButton").GetChild(0).GetComponent<Text>().text = "Şık Eleme " + menuManager.player.fiftyFiftySkillCount.ToString();
		    menuManager.disableTwoCountText.text = menuManager.player.fiftyFiftySkillCount.ToString();
        
        }
        else if (adType == "earn2x")
        {
            Camera.main.GetComponent<GameManager>().DoubleTheCoin();
        }
        SendUserData();
    }

    public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args) {
        MonoBehaviour.print("HandleRewardBasedVideoLeftApplication event received");
    }
}