using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {
	// DatabaseReference reference;
	public GameObject uiLayer;


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