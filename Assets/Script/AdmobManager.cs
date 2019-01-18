using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;

public class AdmobManager : MonoBehaviour {
    private RewardBasedVideoAd rewardBasedVideo;
    MenuManager menuManager;

    public void Start()
    {
        menuManager = Camera.main.GetComponent<MenuManager>();
        this.rewardBasedVideo = RewardBasedVideoAd.Instance;
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
    }

    public void RequestRewardBasedVideo(string type)
    {
        string adUnitId = "unexpected_platform";
        if (type == "coin")
        {
            #if UNITY_ANDROID
                adUnitId = "ca-app-pub-3940256099942544/5224354917";
            #elif UNITY_IPHONE
                adUnitId = "ca-app-pub-3940256099942544/1712485313";
            #else
                adUnitId = "unexpected_platform";
            #endif

            // #if UNITY_ANDROID
            //     string adUnitId = "ca-app-pub-7734671340913331/9151687246";
            // #elif UNITY_IPHONE
            //     string adUnitId = "ca-app-pub-7734671340913331/3404590447";
            // #else
            //     string adUnitId = "unexpected_platform";
            // #endif
        }
        else if (type == "joker")
        {
            #if UNITY_ANDROID
                adUnitId = "ca-app-pub-3940256099942544/5224354917";
            #elif UNITY_IPHONE
                adUnitId = "ca-app-pub-3940256099942544/1712485313";
            #else
                adUnitId = "unexpected_platform";
            #endif
        } 
        else if (type == "disable")
        {
            #if UNITY_ANDROID
                adUnitId = "ca-app-pub-3940256099942544/5224354917";
            #elif UNITY_IPHONE
                adUnitId = "ca-app-pub-3940256099942544/1712485313";
            #else
                adUnitId = "unexpected_platform";
            #endif
        }

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        this.rewardBasedVideo.LoadAd(request, adUnitId);

        if (rewardBasedVideo.IsLoaded()) {
            rewardBasedVideo.Show();
        }
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;

        if (type.ToLower() == "coin")
        {
            menuManager.player.totalCoin += (int)amount;
            menuManager.userDataObject.transform.Find("CoinButton").GetChild(0).GetComponent<Text>().text = menuManager.player.totalCoin.ToString();
        }
        else if (type.ToLower() == "joker")
        {
            menuManager.player.knowQuestionSkillCount += (int)amount;
            menuManager.userDataObject.transform.Find("JokerButton").GetChild(0).GetComponent<Text>().text = "Joker " + menuManager.player.knowQuestionSkillCount.ToString();
        }
        else if (type.ToLower() == "disable")
        {
            menuManager.player.fiftyFiftySkillCount += (int)amount;
            menuManager.userDataObject.transform.Find("DisableButton").GetChild(0).GetComponent<Text>().text = "Şık Eleme " + menuManager.player.fiftyFiftySkillCount.ToString();
        }

        SendUserData();

       print("HandleRewardBasedVideoRewarded event received for "
                        + amount.ToString() + " " + type);
    }

    void SendUserData()
    {
        PlayerPrefs.SetString("userData", JsonUtility.ToJson(menuManager.player));
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
		reference.Child("userList").Child(menuManager.player.userId).SetRawJsonValueAsync(PlayerPrefs.GetString("userData"));
    }
}