using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class RegisterManager : MonoBehaviour {
	public GameObject uiLayer;
	public InputField nameField;
	public Text infoText;
	string userName = "";
	bool isMale = false;

	void Awake()
	{
		//PlayerPrefs.DeleteAll();
		if (PlayerPrefs.HasKey("userData"))
		{
			SceneManager.LoadScene("MainMenu");
		}
	}

	public void ChangeLayerTo(int layerCount)
	{
		uiLayer.GetComponent<Animator>().SetInteger("layerCount", layerCount);
	}

	IEnumerator GoToMenu()
	{
		yield return new WaitForSeconds(1.2f);
		SceneManager.LoadScene("MainMenu");
	}

	public void GetName()
	{
		infoText.gameObject.SetActive(false);
		if (nameField.text.Length < 3)
		{
			infoText.text = "Kullanıcı adınız 3 karakterden kısa olamaz";
			infoText.gameObject.SetActive(true);
			return;
		}
		// TODO check if username is taken online
		userName = nameField.text;
		ChangeLayerTo(1);
		print(userName);
	}

	public void GetGender(bool isMale)
	{
		this.isMale = isMale;
		if (isMale)
		{
			uiLayer.transform.GetChild(1).GetChild(0).GetChild(0).gameObject.SetActive(false);
			uiLayer.transform.GetChild(1).GetChild(1).GetChild(0).gameObject.SetActive(true);
		}
		else
		{
			uiLayer.transform.GetChild(1).GetChild(0).GetChild(0).gameObject.SetActive(true);
			uiLayer.transform.GetChild(1).GetChild(1).GetChild(0).gameObject.SetActive(false);			
		}
	}

	public void CreateUser()
	{
		Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
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
		
		User newUserData = new User(newUser.UserId, userName, isMale);
		PlayerPrefs.SetString("userData", JsonUtility.ToJson(newUserData));

		DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
		reference.Child("userList").Child(newUser.UserId).SetRawJsonValueAsync(PlayerPrefs.GetString("userData"));
		});

		ChangeLayerTo(2);
		StartCoroutine(GoToMenu());
	}
}
