using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Monetization;
using Firebase.Database;

public class UnityAdsManager  : Singleton<UnityAdsManager> {

	string gameId;
    public bool testMode = true;
	MenuManager menuManager;
	public AdType adType;

    public void DelayedStart () {
#if UNITY_IOS
		gameId = "3098996";
#else // android or editor
		gameId = "3098997";
#endif
		if (Monetization.isSupported) {
			Monetization.Initialize (gameId, testMode);
		}
		else
		{
			Debug.LogError("WTF");
		}
    }

    public void ShowAd (AdType adType = AdType.video) {
		Debug.Log("show ad " + adType.ToString());
		this.adType = adType;
		if (adType == AdType.video)
		{
			StartCoroutine (ShowAdWhenReady (PlacemantType.video));
		}
		else
		{
			StartCoroutine (ShowAdWhenReady (PlacemantType.rewardedVideo));
		}
    }

    private IEnumerator ShowAdWhenReady (PlacemantType type) {
        while (!Monetization.IsReady (type.ToString())) {
            yield return new WaitForEndOfFrame();
        }

        ShowAdPlacementContent ad = null;
        ad = Monetization.GetPlacementContent (type.ToString()) as ShowAdPlacementContent;

        if(ad != null) {
			if(type == PlacemantType.video)
			{
				// ShowAdCallbacks options = new ShowAdCallbacks ();
				// options.finishCallback = HandleShowResult;
  	        	ad.Show ();
			}
			else if (type == PlacemantType.rewardedVideo)
			{
				ShowAdCallbacks options = new ShowAdCallbacks ();
				options.finishCallback = HandleShowResult;
				ad.Show (options);
			}
        }
    }


	void HandleShowResult (ShowResult result) {
        if (result == ShowResult.Finished) {
            Reward();
        } else if (result == ShowResult.Skipped) {
            Debug.LogWarning ("The player skipped the video - DO NOT REWARD!");
        } else if (result == ShowResult.Failed) {
            Debug.LogError ("Video failed to show");
        }
    }

	void Reward()
	{
		if(!menuManager && Camera.main.GetComponent<MenuManager>())
        {
            menuManager = Camera.main.GetComponent<MenuManager>();
        }

		Debug.Log("Watched " + adType.ToString());
        if(adType == AdType.coin) 
        {
            menuManager.player.totalCoin += 50;
            menuManager.userDataObject.transform.Find("CoinButton").GetChild(0).GetComponent<Text>().text = menuManager.player.totalCoin.ToString();
        
        } 
        else if(adType == AdType.joker)
        {
            menuManager.player.knowQuestionSkillCount += 1;
            menuManager.userDataObject.transform.Find("JokerButton").GetChild(0).GetComponent<Text>().text = I2.Loc.ScriptLocalization.Get("Joker") + " " + menuManager.player.knowQuestionSkillCount.ToString();
            menuManager.knowQuestionCountText.text = menuManager.player.knowQuestionSkillCount.ToString();
        
        }
        else if (adType == AdType.disable)
        {
            menuManager.player.fiftyFiftySkillCount += 1;
            menuManager.userDataObject.transform.Find("DisableButton").GetChild(0).GetComponent<Text>().text = I2.Loc.ScriptLocalization.Get("Wipe") + " " + menuManager.player.fiftyFiftySkillCount.ToString();
		    menuManager.disableTwoCountText.text = menuManager.player.fiftyFiftySkillCount.ToString();
        
        }
        else if (adType == AdType.earn2x)
        {
            Camera.main.GetComponent<GameManager>().DoubleTheCoin();
        }

        if(menuManager || Camera.main.GetComponent<MenuManager>())
        {
            if (!menuManager)
            {
                menuManager = Camera.main.GetComponent<MenuManager>();
            }
            SendUserData();
        }
	}

	void SendUserData()
    {
        PlayerPrefs.SetString("userData", JsonUtility.ToJson(menuManager.player));
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("userList").Child(menuManager.player.userId).SetRawJsonValueAsync(PlayerPrefs.GetString("userData"));

    }
}

public enum PlacemantType
{
	video,
	rewardedVideo
}

public enum AdType{
	coin,
	joker,
	disable,
	earn2x,
	video
}
