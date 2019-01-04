﻿using System.Collections;
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
	DataToCarry dtc;

	void Awake()
	{
		//PlayerPrefs.DeleteAll();

		UpdateGoogle();
		FirebaseStart();
		if (GameObject.Find("DataToCarry"))
		{
			dtc = GameObject.Find("DataToCarry").GetComponent<DataToCarry>();
			GetQuestions();
			GetFakeUsers();
		}
		else
		{
			Debug.LogError("DataToCarry gameObject couldn't be found");
		}

		if (PlayerPrefs.HasKey("userData"))
		{
			SceneManager.LoadScene("MainMenu");
		}
		else
		{
			ChangeLayerTo(1);
		}
	}
	void FirebaseStart()
	{
		// Set up the Editor before calling into the realtime database.
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://trivia-challanger.firebaseio.com/");
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

	public void GetFakeUsers()
	{
		FirebaseDatabase.DefaultInstance.GetReference("fakeUserList").GetValueAsync().ContinueWith(task => {
			if (task.IsFaulted) {
				// Handle the error...
				Debug.LogError("Error in FirebaseStart");
			}
			else if (task.IsCompleted) {
				DataSnapshot snapshot = task.Result;				

				foreach (DataSnapshot i in snapshot.Children)
				{
					dtc.ful.fakeUserList.Add(JsonUtility.FromJson<UserLite>(i.GetRawJsonValue()));
				}
			}
		});
	}

	void GetQuestions()
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
					dtc.ql.questionList.Add(JsonUtility.FromJson<Question>(i.GetRawJsonValue()));
				}
			}
		});
	}

	bool UsernameAvailable(string username)
	{
			FirebaseDatabase.DefaultInstance.GetReference("userList").GetValueAsync().ContinueWith(task => {
			if (task.IsFaulted) {
				// Handle the error...
				Debug.LogError("Error in FirebaseStart");
			}
			else if (task.IsCompleted) {
				DataSnapshot snapshot = task.Result;				

				foreach (DataSnapshot i in snapshot.Children)
				{
					if(userName == JsonUtility.FromJson<User>(i.GetRawJsonValue()).username)
					{
						return false;
					}
				}
			}
		});

		foreach (UserLite fakeUser in dtc.ful.fakeUserList)
		{
			if (userName == fakeUser.userName)
			{
				return false;
			}
		}
		return true;
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
		
		if (!UsernameAvailable(nameField.text))
		{
			infoText.text = "Bu kullanıcı adı zaten kullanılıyor";
			infoText.gameObject.SetActive(true);
		}
		else
		{
			userName = nameField.text;
			ChangeLayerTo(1);		
		}
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