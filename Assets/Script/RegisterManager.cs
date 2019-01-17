using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using DG.Tweening;

public class RegisterManager : MonoBehaviour {
	public GameObject uiLayer;
	public InputField nameField;
	public Text infoText;
	bool isMale = true;
	public GameObject loadingImage;
	DataToCarry dtc;


	void Start()
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
		loadingImage.GetComponent<Image>().DOFillAmount(1, 1).OnComplete(Cont);

	}

	void Cont()
	{
		if (PlayerPrefs.HasKey("userData"))
		{
			SceneManager.LoadScene("MainMenu");
		}
		else
		{
			ChangeLayerTo(0);
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

	public void ChangeLayerTo(int layerCount)
	{
		uiLayer.GetComponent<Animator>().SetInteger("layerCount", layerCount);
	}

	IEnumerator GoToMenu()
	{
		ChangeLayerTo(2);
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

		// check if name exist
		FirebaseDatabase.DefaultInstance.GetReference("userList").GetValueAsync().ContinueWith(task => {
			if (task.IsFaulted) {
				Debug.LogError("Error in FirebaseStart");
				infoText.text = "İnternet bağlantınızı kontrol edin";
				infoText.gameObject.SetActive(true);
			}
			else if (task.IsCompleted) {
				bool isNameAvailable = true;
				DataSnapshot snapshot = task.Result;				
	
				foreach (DataSnapshot i in snapshot.Children)
				{
					if(nameField.text.Equals(JsonUtility.FromJson<User>(i.GetRawJsonValue()).username))
					{
						isNameAvailable = false;
						break;
					}
				}
				if (isNameAvailable)
				{
					foreach (UserLite fakeUser in dtc.ful.fakeUserList)
					{
						if (nameField.text == fakeUser.userName)
						{
							isNameAvailable = false;
							break;
						}
					}
				}

				if (!isNameAvailable)
				{
					infoText.text = "Bu kullanıcı adı zaten kullanılıyor";
					infoText.gameObject.SetActive(true);
				}
				else
				{
					nameField.interactable = false;
					//ChangeLayerTo(2);	
					CreateUser();
				}
			}
		});
	}

	public void GetGender(bool isMale)
	{
		this.isMale = isMale;
		if (isMale)
		{
			uiLayer.transform.GetChild(2).GetChild(0).GetChild(0).gameObject.SetActive(false);
			uiLayer.transform.GetChild(2).GetChild(1).GetChild(0).gameObject.SetActive(true);
		}
		else
		{
			uiLayer.transform.GetChild(2).GetChild(0).GetChild(0).gameObject.SetActive(true);
			uiLayer.transform.GetChild(2).GetChild(1).GetChild(0).gameObject.SetActive(false);			
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
			
			User newUserData = new User(newUser.UserId, nameField.text, isMale);
			PlayerPrefs.SetString("userData", JsonUtility.ToJson(newUserData));

			DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
			reference.Child("userList").Child(newUser.UserId).SetRawJsonValueAsync(PlayerPrefs.GetString("userData"));
		});

		StartCoroutine(GoToMenu());
	}

	public void Restart()
	{
		SceneManager.LoadScene("Register");
	}
}