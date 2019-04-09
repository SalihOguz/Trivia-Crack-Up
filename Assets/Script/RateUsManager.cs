using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RateUsManager : MonoBehaviour {
    // private string appStoreUrl = "https://itunes.apple.com/us/app/appname/1442550000?mt=8";
    // private string playStoreUrl = "https://play.google.com/store/apps/details?id=com.digitalwords.wordcube";

    public Button rateButton;
    MenuManager menuManager;

    void Start()
	{
		menuManager = Camera.main.GetComponent<MenuManager>();
        if(PlayerPrefs.GetInt("rated") == 1) {
            rateButton.interactable = false;
        }
    }

    // public void shareGameLinks() {
    //     #if UNITY_IOS
    //         new NativeShare().SetSubject("(Kelime Oyunu - Word Cube) ").SetText(appStoreUrl).Share();
    //     #elif UNITY_ANDROID
    //         new NativeShare().SetSubject("(Kelime Oyunu - Word Cube) ").SetText(playStoreUrl).Share();
    //     #endif
    // }

    public void RateApp()
	{
		try
		{
			#if UNITY_ANDROID
            	Application.OpenURL("market://details?id=com.digitalwords.triviacrackup");
			#elif UNITY_IPHONE
				Application.OpenURL("itms-apps://itunes.apple.com/app/id1457500186");
			#endif

			rateButton.interactable = false;
			menuManager.AddCoins(50);
			PlayerPrefs.SetInt("rated", 1);
		}
		catch
		{
			Debug.LogError("Error in RateApp");
		}
    }
}
