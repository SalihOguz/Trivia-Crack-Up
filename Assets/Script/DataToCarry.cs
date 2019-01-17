using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class DataToCarry : Singleton<DataToCarry> {
	public User player2;
	public QuestionList ql;
	public FakeUserList ful;

	void Start()
	{
		ql = new QuestionList();
		ql.questionList = new List<Question>();
		AdmobStart();
	}

	public void ChooseOpponent()
	{
		player2 = MakeFakeUser();
		if (Camera.main.GetComponent<MenuManager>())
		{
			Camera.main.GetComponent<MenuManager>().opponentDataObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = Resources.Load<Avatars>("Data/Avatars").avatarSprites[player2.avatarId];
			Camera.main.GetComponent<MenuManager>().opponentDataObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Text>().text = player2.username;
		}
		else
		{
			print("whaaat");
		}
	}

	User MakeFakeUser()
    {
        // TextAsset textAssset = Resources.Load<TextAsset>("Data/FakeUserList");
		// FakeUserList ful = JsonUtility.FromJson<FakeUserList>(textAssset.text);
        UserLite fake = ful.fakeUserList[UnityEngine.Random.Range(0, ful.fakeUserList.Count)];
        User p2 = new User("1", fake.userName, fake.isMale, fake.totalCoin + 30 * (Random.Range(-65,65)));
        return p2;
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
}
