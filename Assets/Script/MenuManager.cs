using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine.Purchasing;

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

	void Start()
	{
		player = JsonUtility.FromJson<User>(PlayerPrefs.GetString("userData"));
		
		Sprite avatar = Resources.Load<Avatars>("Data/Avatars").avatarSprites[player.avatarId];
		userDataObject.transform.GetChild(0).GetComponent<Image>().sprite = avatar;
		userDataObject.transform.Find("UserNameBG").GetChild(0).GetComponent<Text>().text = player.username;
		userDataObject.transform.Find("CoinButton").GetChild(0).GetComponent<Text>().text = player.totalCoin.ToString();
		userDataObject.transform.Find("JokerButton").GetChild(0).GetComponent<Text>().text = "Joker " + player.knowQuestionSkillCount.ToString();
		userDataObject.transform.Find("DisableButton").GetChild(0).GetComponent<Text>().text = "Şık Eleme " + player.fiftyFiftySkillCount.ToString();
		adButton.transform.GetChild(UnityEngine.Random.Range(0, adButton.transform.childCount)).gameObject.SetActive(true);

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
		if (GameObject.Find("DataToCarry"))
		{
			if (GameObject.Find("DataToCarry").GetComponent<DataToCarry>().mainMenuAnimLayerIndex == 4)
			{
				if (player.totalCoin >= 30 * 5) // TODO this is min bid amount 30 * 5 turns
				{
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
			auth.SignInAnonymouslyAsync();
		}
	}

	void ChooseOpponent()
	{
		if(GameObject.Find("DataToCarry"))
		{
			GameObject.Find("DataToCarry").GetComponent<DataToCarry>().ChooseOpponent();
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
			PlaySound(0);
			uiLayer.GetComponent<Animator>().SetInteger("layerCount", layerCount);
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
		if (player.totalCoin >= 30 * 5) // TODO this is min bid amount 30 * 5 turns
		{
			ChooseOpponent();
			ChangeLayerTo(4);
			StartCoroutine(PlayerFound());
		}
		else
		{
			ChangeLayerTo(3);
		}
	}

	IEnumerator PlayerFound()
	{
		if (GameObject.Find("DataToCarry"))
		{
			GameObject.Find("DataToCarry").GetComponent<DataToCarry>().mainMenuAnimLayerIndex = -1;
		}

		yield return new WaitForSeconds(UnityEngine.Random.Range(4f, 6f));
		ChangeLayerTo(5);
		yield return new WaitForSeconds(3f);
		ChangeLayerTo(8);
		yield return new WaitForSeconds(1);
		GoToScene("Game");
	}

	public void ShowAdEarnCoin()
	{
		if(GameObject.Find("AdManager"))
		{
			PlaySound(0);
			print("coin");
			GameObject.Find("AdManager").GetComponent<AdmobManager>().ShowAd("coin");
		}
	}

	public void ShowAdEarnKnowQuestion()
	{
		if(GameObject.Find("AdManager"))
		{
			PlaySound(0);
			print("joker");
			GameObject.Find("AdManager").GetComponent<AdmobManager>().ShowAd("joker");
		}
	}	

	public void ShowAdEarnDisableTwo()
	{
		if(GameObject.Find("AdManager"))
		{
			PlaySound(0);
			print("disable");
			GameObject.Find("AdManager").GetComponent<AdmobManager>().ShowAd("disable");
		}
	}

	public void PurchasePass()
	{

	}

	public void PurchaseCoin()
	{

	}

	public void AddCoins(int amount)
	{
		player.totalCoin += amount;
		SendUserData();

	}

	public void AddMixedPack(int amount)
	{
		player.knowQuestionSkillCount += amount;
		player.fiftyFiftySkillCount += amount;
		SendUserData();
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
        // foreach(var product in products) {
        //     if(product.definition.id.Equals("com.digitalwords.wordcube.pass5")) {
        //         hintPanel.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
        //     } else if (product.definition.id.Equals("com.digitalwords.wordcube.pass10")) {
        //         hintPanel.transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
        //     } else if (product.definition.id.Equals("com.digitalwords.wordcube.pass20")) {
        //         hintPanel.transform.GetChild(2).GetChild(2).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
        //     } else if (product.definition.id.Equals("com.digitalwords.wordcube.coin1000")) {
        //         coinPanel.transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
        //     } else if (product.definition.id.Equals("com.digitalwords.wordcube.coin5000")) {
        //         coinPanel.transform.GetChild(1).GetChild(3).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
        //     } else if (product.definition.id.Equals("com.digitalwords.wordcube.coin10000")) {
        //         coinPanel.transform.GetChild(2).GetChild(3).GetChild(0).GetComponent<Text>().text = product.metadata.localizedPriceString;
        //     }
        // }
    }

	public void LoadLeaderboard()
	{
		// getting all users evertime leaderboard viewed might not be the best idea TODO
		leaderboardLoadingObject.SetActive(true);
		List<User> userList = new List<User>();
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
				FillLeaderboard(userList);	
			}
		});
	}

	public void FillLeaderboard(List<User> userList)
	{
		QuickSort(userList, 0, userList.Count - 1);

		bool userFound = false;

		for (int i = 0; i < userList.Count; i++)
		{
			if (i == 10)
			{
				break;
			}
			leaderboardObject.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = userList[i].username;
			leaderboardObject.transform.GetChild(i).GetChild(1).GetComponent<Text>().text = userList[i].score.ToString();
			leaderboardObject.transform.GetChild(i).GetChild(2).GetComponent<Text>().text = (i+1).ToString() + ".";
			leaderboardObject.transform.GetChild(i).gameObject.SetActive(true);

			if(userList[i].userId == player.userId)
			{
				userFound = true;
				leaderboardObject.transform.GetChild(i).GetChild(3).gameObject.SetActive(true);
			}
		}
		
		if (!userFound)
		{
			leaderboardObject.transform.GetChild(leaderboardObject.transform.childCount).gameObject.SetActive(true);
			leaderboardObject.transform.GetChild(leaderboardObject.transform.childCount).GetChild(0).GetComponent<Text>().text = player.username;
			leaderboardObject.transform.GetChild(leaderboardObject.transform.childCount).GetChild(1).GetComponent<Text>().text = player.score.ToString();
			leaderboardObject.transform.GetChild(leaderboardObject.transform.childCount).GetChild(2).GetComponent<Text>().text = (FindUserIndex(userList)).ToString() + ".";
		}
		
		leaderboardLoadingObject.SetActive(false);
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
				max = mid - 1;  
			}  
			else  
			{  
				min = mid + 1;  
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
}