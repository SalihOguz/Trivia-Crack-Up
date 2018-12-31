using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {
	// DatabaseReference reference;
	public GameObject uiLayer;
	public GameObject userDataObject;
	public GameObject userDataObject2;
	public GameObject userDataObject3;
	public GameObject opponentDataObject;
	User player;


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
	}

	// void Start () {
	// 	UpdateGoogle();
	// 	FirebaseStart();
	// }

	// void UpdateGoogle()
	// {
	// 	Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
	// 	var dependencyStatus = task.Result;
	// 	if (dependencyStatus == Firebase.DependencyStatus.Available) {
	// 		// Create and hold a reference to your FirebaseApp, i.e.
	// 		//   app = Firebase.FirebaseApp.DefaultInstance;
	// 		// where app is a Firebase.FirebaseApp property of your application class.

	// 		// Set a flag here indicating that Firebase is ready to use by your
	// 		// application.
	// 	} else {
	// 		UnityEngine.Debug.LogError(System.String.Format(
	// 		"Could not resolve all Firebase dependencies: {0}", dependencyStatus));
	// 		// Firebase Unity SDK is not safe to use here.
	// 	}
	// 	});
	// }

	// void FirebaseStart()
	// {
	// 	FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://try-storage-33aaa.firebaseio.com/");
	// 	reference = FirebaseDatabase.DefaultInstance.RootReference;

	// }

	// public void writeNewUser() {
	// 	User user = new User(int.Parse(inputUserId.text), inputUsername.text);
	// 	string json = JsonUtility.ToJson(user);

	// 	reference.Child("users").Child(inputUserId.text).SetRawJsonValueAsync(json);
	// }

	void ChooseOpponent()
	{
		if(GameObject.Find("UserData"))
		{
			GameObject.Find("UserData").GetComponent<UserData>().ChooseOpponent();
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
		ChooseOpponent();
		ChangeLayerTo(4);
		StartCoroutine(PlayerFound());
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