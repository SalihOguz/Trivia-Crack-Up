using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class MenuManager : MonoBehaviour {
	// DatabaseReference reference;
	public GameObject uiLayer;
	public GameObject userDataObject;
	public GameObject userDataObject2;
	public GameObject userDataObject3;
	public GameObject opponentDataObject;
	public User player;

	void Start()
	{
		player = JsonUtility.FromJson<User>(PlayerPrefs.GetString("userData"));
		
		Sprite avatar = Resources.Load<Avatars>("Data/Avatars").avatarSprites[player.avatarId];
		userDataObject.transform.GetChild(0).GetComponent<Image>().sprite = avatar;
		userDataObject.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = player.username;
		userDataObject.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = player.totalCoin.ToString();
		// TODO put pass or other powers

		userDataObject2.transform.GetChild(0).GetComponent<Image>().sprite = avatar;
		userDataObject2.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = player.username;

		userDataObject3.transform.GetChild(0).GetComponent<Image>().sprite = avatar;
		userDataObject3.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = player.username;

		UserLogin();
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
		uiLayer.GetComponent<Animator>().SetInteger("layerCount", layerCount);
	}

	public void ChangeShopLayer(int shopLayer)
	{
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
		yield return new WaitForSeconds(UnityEngine.Random.Range(4f, 6f));
		ChangeLayerTo(5);
		yield return new WaitForSeconds(3f);
		ChangeLayerTo(6);
		yield return new WaitForSeconds(1);
		GoToScene("Game");
	}

	public void ShowAd()
	{

	}

	public void PurchasePass()
	{

	}

	public void PurchaseCoin()
	{

	}
}