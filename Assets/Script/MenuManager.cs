using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine.Purchasing;
using I2.Loc;

public class MenuManager : MonoBehaviour {
	// DatabaseReference reference;
	public GameObject uiLayer;
	public GameObject userDataObject;
	public GameObject opponentDataObject;
	public Text knowQuestionCountText;
	public Text disableTwoCountText;
	public User player;
	public GameObject leaderboardObject;
	public GameObject leaderboardLoadingObject;
	public GameObject adButton;
	public GameObject soundButton;
	public GameObject levelContentParent;
	public Sprite[] levelBgSprites; // 0 past, 1 current, 2 future
	public GameObject packsStoreParent;
	public GameObject coinsStoreParent;
	Avatars av;
	public GameObject tr_flag;
	public GameObject en_flag;
	public GameObject chooseLanguageScreen;
	public GameObject characterNamesParent;
	DataToCarry dtc;
	public Purchaser purchaser;
	
	void Start()
	{
		player = JsonUtility.FromJson<User>(PlayerPrefs.GetString("userData"));
		av = Resources.Load<Avatars>("Data/Avatars");
		dtc = GameObject.Find("DataToCarry").GetComponent<DataToCarry>();

		adButton.transform.GetChild(UnityEngine.Random.Range(0, adButton.transform.childCount - 1)).gameObject.SetActive(true);
		Sprite avatar = av.avatarSprites[player.avatarId];
		userDataObject.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = avatar;
		userDataObject.transform.Find("UserNameBG").GetChild(0).GetComponent<Text>().text = player.username;
		userDataObject.transform.Find("CoinButton").GetChild(0).GetComponent<Text>().text = player.totalCoin.ToString();
		userDataObject.transform.Find("JokerButton").GetChild(0).GetComponent<Text>().text = ScriptLocalization.Get("Joker") + " " + player.knowQuestionSkillCount.ToString();
		userDataObject.transform.Find("DisableButton").GetChild(0).GetComponent<Text>().text = ScriptLocalization.Get("Wipe") + " " + player.fiftyFiftySkillCount.ToString();
		

		knowQuestionCountText.text = player.knowQuestionSkillCount.ToString();
		disableTwoCountText.text = player.fiftyFiftySkillCount.ToString();

		if (ConnectionManager.isOnline)
		{
			UserLogin();
		}
		else
		{
			Debug.LogError("No internet. User couldn't login");
			ChangeLayerTo(7);
		}

		ArrangeSound();
		SetStartScreen();
		ArrangeLevelScreen();

		SetLanguage(PlayerPrefs.GetString("lang"));

		Debug.Log(JsonUtility.ToJson(player));

		if (PlayerPrefs.GetInt("chosenLang") == 0)
		{
			chooseLanguageScreen.SetActive(true);
			PlayerPrefs.SetInt("chosenLang", 1);
		}

		purchaser.Init();
	}

	void ArrangeLevelScreen()
	{
		for (int i = 0; i < levelContentParent.transform.childCount; i++) // -1 because last one is not character but "coming soon"
		{
			if (i < player.level)
			{
				levelContentParent.transform.GetChild(i).GetComponent<Image>().sprite = levelBgSprites[0];
				levelContentParent.transform.GetChild(i).GetChild(2).GetComponent<Image>().sprite = av.avatarSprites[i];
				levelContentParent.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = ScriptLocalization.Get(av.avatarNames[i]);
			}
			else if (i == player.level)
			{
				levelContentParent.transform.GetChild(i).GetComponent<Image>().sprite = levelBgSprites[1];
				levelContentParent.transform.GetChild(i).GetChild(2).GetComponent<Image>().sprite = av.avatarSprites[i];
				levelContentParent.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = ScriptLocalization.Get(av.avatarNames[i]);
			}
			else
			{
				levelContentParent.transform.GetChild(i).GetComponent<Image>().sprite = levelBgSprites[2];
				levelContentParent.transform.GetChild(i).GetChild(2).GetComponent<Image>().sprite = av.avatarUnlockedSprites[i];
				levelContentParent.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = ScriptLocalization.Get(av.avatarNames[i]);
			}

			levelContentParent.transform.GetChild(i).GetChild(1).GetComponent<Text>().text = AddOrdinal(i+1) + " " + ScriptLocalization.Get("LevelSmall");
		}
	}

	void ArrangeSound()
	{
		int id = PlayerPrefs.GetInt("isMusicOn");
		TurnMusic(id);
		TurnSound(id);

		PlayMusic(0);
	}

	void SetStartScreen()
	{
		if (dtc)
		{
			if (dtc.mainMenuAnimLayerIndex == 4)
			{
				if (player.totalCoin >= 30 * 5) // TODO this is min bid amount 30 * 5 turns
				{
					userDataObject.transform.GetChild(0).GetComponent<Button>().enabled = false;
					userDataObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
					ChooseOpponent();
					uiLayer.GetComponent<Animator>().SetInteger("layerCount", 4);
					StartCoroutine(PlayerFound());
				}
				else
				{
					uiLayer.GetComponent<Animator>().SetInteger("layerCount", -1);
				}
			}
			else
			{
				uiLayer.GetComponent<Animator>().SetInteger("layerCount", -1);
			}
		}
	}

	void UserLogin()
	{
		Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

		if (!PlayerPrefs.HasKey("userData")) 
		{
			auth.SignInAnonymouslyAsync().ContinueWith(task => {
				if (task.IsCanceled) {
					Debug.LogError("SignInAnonymouslyAsync was canceled.");
					return;
				}
				if (task.IsFaulted) {
					Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
					return;
				}

				Firebase.Auth.FirebaseUser newUser = task.Result;
				User newUserData = JsonUtility.FromJson<User>(PlayerPrefs.GetString("userData"));
				newUserData.userId = newUser.UserId;
				PlayerPrefs.SetString("userData", JsonUtility.ToJson(newUserData));
			});
		}
		else
		{
#if ! UNITY_EDITOR
			auth.SignInAnonymouslyAsync().ContinueWith(task => {
				if (task.IsCanceled) {
					Debug.LogError("SignInAnonymouslyAsync was canceled.");
					return;
				}
				if (task.IsFaulted) {
					Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
					return;
				}
			});
#endif
		}
	}

	void ChooseOpponent()
	{
		if(dtc)
		{
			dtc.ChooseOpponent();
		}
	}

    public void GoToScene(string name)
    {
        SceneManager.LoadScene(name);
    }

	public void ChangeLayerTo(int layerCount)
	{
		if (ConnectionManager.isOnline)
		{
			if(layerCount != 7 && layerCount != 5 && layerCount != 8)
			{
				PlaySound(0);
			}
			if (layerCount == 6)
			{
				if (player.totalCoin >= 30 * 5) // TODO this is min bid amount 30 * 5 turns
				{
					uiLayer.GetComponent<Animator>().SetInteger("layerCount", layerCount);
					userDataObject.transform.GetChild(0).GetComponent<Button>().enabled = false;
					userDataObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
				}
				else
				{
					ChangeLayerTo(3);
				}
			}
			else
			{
				uiLayer.GetComponent<Animator>().SetInteger("layerCount", layerCount);
			}
		}
		else
		{
			Debug.Log("No internet in menu");
			uiLayer.GetComponent<Animator>().SetInteger("layerCount", 7);
		}
	}

	public void ChangeShopLayer(int shopLayer)
	{
		PlaySound(0);
		uiLayer.GetComponent<Animator>().SetInteger("shopIndex", shopLayer);
	}

	public void FakeSearchPlayer()
	{
		// if (player.totalCoin >= 30 * 5) // TODO this is min bid amount 30 * 5 turns
		// {
			ChooseOpponent();
			ChangeLayerTo(4);
			StartCoroutine(PlayerFound());
		// }
		// else
		// {
		// 	ChangeLayerTo(3);
		// }
	}

	IEnumerator PlayerFound()
	{
		if (dtc)
		{
			dtc.mainMenuAnimLayerIndex = -1;
		}

		characterNamesParent.transform.GetChild(0).GetComponent<Text>().text = ScriptLocalization.Get(av.avatarNames[player.avatarId]);
		characterNamesParent.transform.GetChild(1).GetComponent<Text>().text = ScriptLocalization.Get(av.avatarNames[dtc.player2.avatarId]);

		float rnd = UnityEngine.Random.Range(4f, 8f);
		yield return new WaitForSeconds(rnd - 1.5f);
		PlaySound(11);
		yield return new WaitForSeconds(1.5f);
		ChangeLayerTo(5);
		yield return new WaitForSeconds(1.5f);
		ChangeLayerTo(8);
		yield return new WaitForSeconds(0.3f);
		GoToScene("Game");
	}

	public void ShowAdEarnCoin()
	{
		if(GameObject.Find("AdManager"))
		{
			PlaySound(0);
			print("coin");
			//GameObject.Find("AdManager").GetComponent<AdmobManager>().ShowAd("coin");
			GameObject.Find("AdManager").GetComponent<UnityAdsManager>().ShowAd(AdType.coin);
		}
	}

	public void ShowAdEarnKnowQuestion()
	{
		if(GameObject.Find("AdManager"))
		{
			PlaySound(0);
			print("joker");
			//GameObject.Find("AdManager").GetComponent<AdmobManager>().ShowAd("joker");
			GameObject.Find("AdManager").GetComponent<UnityAdsManager>().ShowAd(AdType.joker);
		}
	}	

	public void ShowAdEarnDisableTwo()
	{
		if(GameObject.Find("AdManager"))
		{
			PlaySound(0);
			print("disable");
			//GameObject.Find("AdManager").GetComponent<AdmobManager>().ShowAd("disable");
			GameObject.Find("AdManager").GetComponent<UnityAdsManager>().ShowAd(AdType.disable);
		}
	}

	public void AddCoins(int amount)
	{
		player.totalCoin += amount;
		SendUserData();

		userDataObject.transform.Find("CoinButton").GetChild(0).GetComponent<Text>().text = player.totalCoin.ToString();
	}

	public void AddMixedPack(int amount)
	{
		player.knowQuestionSkillCount += amount;
		player.fiftyFiftySkillCount += amount;
		SendUserData();

		userDataObject.transform.Find("JokerButton").GetChild(0).GetComponent<Text>().text = ScriptLocalization.Get("Joker") + " " + player.knowQuestionSkillCount.ToString();
		userDataObject.transform.Find("DisableButton").GetChild(0).GetComponent<Text>().text = ScriptLocalization.Get("Wipe") + " " + player.fiftyFiftySkillCount.ToString();
		knowQuestionCountText.text = player.knowQuestionSkillCount.ToString();
		disableTwoCountText.text = player.fiftyFiftySkillCount.ToString();
	}

    void SendUserData()
    {
        if (ConnectionManager.isOnline)
        {
            PlayerPrefs.SetString("userData", JsonUtility.ToJson(player));
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference.Child("userList").Child(player.userId).SetRawJsonValueAsync(PlayerPrefs.GetString("userData"));
        }
        else
        {
            Debug.LogError("No internet. Data couldn't be sent");
        }
    }

	public void updatePriceList(Product[] products) {
        foreach(var product in products) {
            if(product.definition.id.Equals("mixedpack1")) {
                packsStoreParent.transform.GetChild(2).GetChild(5).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
            } else if (product.definition.id.Equals("mixedpack2")) {
                packsStoreParent.transform.GetChild(3).GetChild(5).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
            } else if (product.definition.id.Equals("mixedpack3")) {
                packsStoreParent.transform.GetChild(4).GetChild(5).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
            } else if (product.definition.id.Equals("mixedpack4")) {
                packsStoreParent.transform.GetChild(5).GetChild(5).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
            } 
			
			else if (product.definition.id.Equals("coinpack1")) {
                coinsStoreParent.transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
            } else if (product.definition.id.Equals("coinpack2")) {
                coinsStoreParent.transform.GetChild(1).GetChild(3).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
            } else if (product.definition.id.Equals("coinpack3")) {
                coinsStoreParent.transform.GetChild(2).GetChild(3).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
            }
        }
    }

	public void LoadLeaderboard()
	{
		// getting all users evertime leaderboard viewed might not be the best idea TODO
		leaderboardLoadingObject.SetActive(true);
		List<User> userList = new List<User>();

		// get users
		FirebaseDatabase.DefaultInstance.GetReference("userList").GetValueAsync().ContinueWith(task => {
			if (task.IsFaulted) {
				// Handle the error...
				Debug.LogError("Error in FirebaseStart");
			}
			else if (task.IsCompleted) {
				DataSnapshot snapshot = task.Result;				

				foreach (DataSnapshot i in snapshot.Children)
				{
					userList.Add(JsonUtility.FromJson<User>(i.GetRawJsonValue()));
				}

				// get fake users
				foreach (var fake in dtc.ful.fakeUserList)
				{
					User temp = new User("1", fake.userName, fake.isMale, fake.totalCoin, fake.score);						
					userList.Add(temp);
				}
				FillLeaderboard(userList);

				// FirebaseDatabase.DefaultInstance.GetReference("fakeUserList/"+LocalizationManager.CurrentLanguageCode).GetValueAsync().ContinueWith(fakesTask => {
				// 	if (fakesTask.IsFaulted) {
				// 		// Handle the error...
				// 		Debug.LogError("Error in FirebaseStart");
				// 	}
				// 	else if (fakesTask.IsCompleted) {
				// 		DataSnapshot fakesSnapshot = fakesTask.Result;				

				// 		foreach (DataSnapshot i in fakesSnapshot.Children)
				// 		{
				// 			UserLite fake = JsonUtility.FromJson<UserLite>(i.GetRawJsonValue());
        		// 			User temp = new User("1", fake.userName, fake.isMale, fake.totalCoin, fake.score);						
				// 			userList.Add(temp);
				// 		}
				// 		FillLeaderboard(userList);	
				// 	}
				// });	
			}
		});
	}

	

	public void FillLeaderboard(List<User> userList)
	{	
		QuickSort(userList, 0, userList.Count - 1);

		bool userFound = false;

		for (int i = 0; i < userList.Count; i++)
		{
			if (i == leaderboardObject.transform.childCount - 1)
			{
				break;
			}
			leaderboardObject.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = userList[i].username;
			leaderboardObject.transform.GetChild(i).GetChild(1).GetComponent<Text>().text = userList[i].score.ToString();
			leaderboardObject.transform.GetChild(i).GetChild(2).GetComponent<Text>().text = AddOrdinal(i+1);
			leaderboardObject.transform.GetChild(i).gameObject.SetActive(true);

			if(userList[i].userId == player.userId)
			{
				userFound = true;
				leaderboardObject.transform.GetChild(i).GetChild(3).gameObject.SetActive(true);
			}
		}
		
		if (!userFound)
		{
			leaderboardObject.transform.GetChild(leaderboardObject.transform.childCount-1).gameObject.SetActive(true);
			leaderboardObject.transform.GetChild(leaderboardObject.transform.childCount-1).GetChild(0).GetComponent<Text>().text = player.username;
			leaderboardObject.transform.GetChild(leaderboardObject.transform.childCount-1).GetChild(1).GetComponent<Text>().text = player.score.ToString();
			int index = FindUserIndex(userList);
			leaderboardObject.transform.GetChild(leaderboardObject.transform.childCount-1).GetChild(2).GetComponent<Text>().text = AddOrdinal(index);
		}
		
		leaderboardLoadingObject.SetActive(false);
	}

	public static string AddOrdinal(int num)
	{
		if (LocalizationManager.CurrentLanguageCode == "tr")
		{
			return num + ".";
		}
		else if (LocalizationManager.CurrentLanguageCode == "en")
		{
			if( num <= 0 ) return num.ToString();

			switch(num % 100)
			{
				case 11:
				case 12:
				case 13:
					return num + "th";
			}

			switch(num % 10)
			{
				case 1:
					return num + "st";
				case 2:
					return num + "nd";
				case 3:
					return num + "rd";
				default:
					return num + "th";
			}	
		}
		else
		{
			return "";
		}
	}

	int FindUserIndex(List<User> userList)
	{
		// Binary Search
		int min = 0;
  		int max = userList.Count - 1; 
		while (min <=max)  
		{  
			int mid = (min + max) / 2;  
			if (player.userId == userList[mid].userId)  
			{  
				return ++mid;  
			}  
			else if (player.score < userList[mid].score)  
			{  
				min = mid + 1;  
				
			}  
			else if (player.score > userList[mid].score || (userList[mid].score == player.score && userList[mid].wonGameCount > player.wonGameCount)) 
			{  
				max = mid - 1;  
			}
			else if (userList[mid].score == player.score)
			{
				return ++mid;
			}
		}  
		return 0;
	}

	public static void QuickSort(List<User> userList, int left, int right)
    {
        if(left > right || left <0 || right <0) return; 

        int index = partition(userList, left, right);

        if (index != -1)
        {
            QuickSort(userList, left, index - 1);
            QuickSort(userList, index + 1, right);
        }
		else
		{
			print("sorted");
		}
    }

    private static int partition(List<User> userList, int left, int right)
    {
        if(left > right) return -1; 

        int end = left; 

        User pivot = userList[right];    // choose last one to pivot, easy to code
        for(int i= left; i< right; i++)
        {
            if (userList[i].score > pivot.score || (userList[i].score == pivot.score && userList[i].wonGameCount > pivot.wonGameCount)) // score is higher or score equals but more game won
            {
                swap(userList, i, end);
                end++; 
            }
        }

        swap(userList, end, right);

        return end; 
    }

    private static void swap(List<User> userList, int left, int right)
    {
        User tmp = userList[left];
        userList[left] = userList[right];
        userList[right] = tmp; 
    }

	void PlaySound(int id)
	{
		if (GameObject.FindGameObjectWithTag("sound"))
		{
			GameObject.FindGameObjectWithTag("sound").GetComponent<SoundManager>().PlaySound(id);
		}
	}

	void PlayMusic(int id)
	{
		if (GameObject.FindGameObjectWithTag("music"))
		{
			GameObject.FindGameObjectWithTag("music").GetComponent<MusicManager>().PlayMusic(id);
		}
	}

	public void TurnMusic(int id)
	{
		if (GameObject.FindGameObjectWithTag("music"))
		{
			GameObject.FindGameObjectWithTag("music").GetComponent<MusicManager>().TurnMusic(id);
			soundButton.transform.GetChild(0).gameObject.SetActive(0 == id);
		}
	}

	public void TurnSound(int id)
	{
		if (GameObject.FindGameObjectWithTag("sound"))
		{
			GameObject.FindGameObjectWithTag("sound").GetComponent<SoundManager>().TurnSound(id);
			soundButton.transform.GetChild(0).gameObject.SetActive(0 == id);
		}
	}	

	public void ChangeLanguage()
	{
		if (PlayerPrefs.GetString("lang") == "en")
		{
			SetLanguage("tr");
		}
		else
		{
			SetLanguage("en");
		}
	}

	public void SetLanguage(string language)
	{
		if (language == "tr")
		{
			en_flag.SetActive(false);
			tr_flag.SetActive(true);
			PlayerPrefs.SetString("lang", "tr");
			LocalizationManager.CurrentLanguageCode = "tr";
		}
		else
		{
			en_flag.SetActive(true);
			tr_flag.SetActive(false);
			PlayerPrefs.SetString("lang", "en");
			LocalizationManager.CurrentLanguageCode = "en";		
		}

		userDataObject.transform.Find("JokerButton").GetChild(0).GetComponent<Text>().text = ScriptLocalization.Get("Joker") + " " + player.knowQuestionSkillCount.ToString();
		userDataObject.transform.Find("DisableButton").GetChild(0).GetComponent<Text>().text = ScriptLocalization.Get("Wipe") + " " + player.fiftyFiftySkillCount.ToString();
		ArrangeLevelScreen();
		LoadQuestions();
		LoadFakeUsers();
	}

	void LoadQuestions()
	{
		LocalQuestionDatabase lqdb = JsonUtility.FromJson<LocalQuestionDatabase>(PlayerPrefs.GetString("localQuestions"));
		dtc.ql = lqdb.questionsInLanguages[LocalizationManager.CurrentLanguageCode];
	}

	void LoadFakeUsers()
	{
		LocalFakeUsersDatabase lfudb = JsonUtility.FromJson<LocalFakeUsersDatabase>(PlayerPrefs.GetString("localFakeUsers"));
		dtc.ful = lfudb.fakeusersInLanguages[LocalizationManager.CurrentLanguageCode];
	}
}