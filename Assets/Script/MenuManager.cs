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

		UpdateGoogle();
		FirebaseStart();

		if (GameObject.Find("DataToCarry"))
		{
			GetQuestions(GameObject.Find("DataToCarry").GetComponent<DataToCarry>().ql);
		}
		else
		{
			Debug.LogError("DataToCarry gameObject couldn't be found");
		}

		UserLogin();
	}

	void UpdateGoogle()
	{
		Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
		var dependencyStatus = task.Result;
		if (dependencyStatus == Firebase.DependencyStatus.Available) {
			// Create and hold a reference to your FirebaseApp, i.e.
			//   app = Firebase.FirebaseApp.DefaultInstance;
			// where app is a Firebase.FirebaseApp property of your application class.

			// Set a flag here indicating that Firebase is ready to use by your
			// application.
		} else {
			UnityEngine.Debug.LogError(System.String.Format(
			"Could not resolve all Firebase dependencies: {0}", dependencyStatus));
			// Firebase Unity SDK is not safe to use here.
		}
		});
	}
	

	void FirebaseStart()
	{
		// Set up the Editor before calling into the realtime database.
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://trivia-challanger.firebaseio.com/");

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
		print(PlayerPrefs.GetString("userData"));
	}

	void GetQuestions(QuestionList ql)
	{
			FirebaseDatabase.DefaultInstance.GetReference("questionList").GetValueAsync().ContinueWith(task => {
			if (task.IsFaulted) {
				// Handle the error...
				Debug.LogError("Error in FirebaseStart");
			}
			else if (task.IsCompleted) {
				DataSnapshot snapshot = task.Result;				

				foreach (DataSnapshot i in snapshot.Children)
				{
					ql.questionList.Add(JsonUtility.FromJson<Question>(i.GetRawJsonValue()));
				}
			}
		});
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