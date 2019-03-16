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
using GoogleMobileAds.Api;
using BugsnagUnity;
using I2.Loc;

public class RegisterManager : MonoBehaviour {
	public GameObject uiLayer;
	public InputField nameField;
	public Text infoText;
	bool isMale = true;
	public GameObject loadingImage;
	DataToCarry dtc;
	int tryConnectionCount = 0;
	public int maxConectionTryCount = 7;
	bool isCheckingName;
	public GameObject NotUpToDateObject;
	bool quesitonDone;
	bool fakeUsersDone;
	public Text versionCodeText;


	void Start()
	{
		versionCodeText.text = "v." + Application.version;
		if (!PlayerPrefs.HasKey("lang"))
		{
			if (Application.systemLanguage == SystemLanguage.Turkish)
			{
				PlayerPrefs.SetString("lang", "tr");
				LocalizationManager.CurrentLanguageCode = "tr";
			}
			else
			{
				PlayerPrefs.SetString("lang", "en");
				LocalizationManager.CurrentLanguageCode = "en";
			}
		}

		//PlayerPrefs.DeleteAll(); //TODO comment these
		// PlayerPrefs.DeleteKey("fakeUsersVersionNo");
		// PlayerPrefs.DeleteKey("questionVersionNo");

		loadingImage.GetComponent<Image>().DOFillAmount(0.8f, 0.4f);
		StartCoroutine(CheckConnection());
	}

	IEnumerator CheckConnection()
	{
		if (ConnectionManager.isOnline)
		{
			DelayedStart();
		}
		else
		{
			tryConnectionCount++;
			if (tryConnectionCount == maxConectionTryCount)
			{
				ChangeLayerTo(3);
			}
			else
			{
				yield return new WaitForSeconds(0.2f);
				StartCoroutine(CheckConnection());
			}
		}
	}

	void DelayedStart()
	{
		//UpdateGoogle();
		FirebaseStart();
		if (GameObject.Find("DataToCarry"))
		{
			dtc = GameObject.Find("DataToCarry").GetComponent<DataToCarry>();
			if (ConnectionManager.isOnline)
			{
				if(GameObject.Find("AdManager"))
				{
					GameObject.Find("AdManager").GetComponent<AdmobManager>().DelayedStart();
				}
				ForceUpdate();
			}
			else
			{
				ChangeLayerTo(3);
			}
		}
		else
		{
			Debug.LogError("DataToCarry gameObject couldn't be found");
		}

	}

	void Cont()
	{
		if (PlayerPrefs.HasKey("userData"))
		{
			SceneManager.LoadScene("MainMenu");
		}
		else // First time opening
		{
			ChangeLayerTo(0);
			PlayerPrefs.SetInt("isMusicOn", 1);
			PlayerPrefs.SetInt("isSoundOn", 1);
		}
	}

	void FirebaseStart()
	{
		// Set up the Editor before calling into the realtime database.
#if UNITY_EDITOR
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://triviachallanger.firebaseio.com/");
#endif
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

	public void GetFakeUsersFromCloud()
	{
		FirebaseDatabase.DefaultInstance.GetReference("fakeUserList").GetValueAsync().ContinueWith(task => {
			if (task.IsFaulted) {
				// Handle the error...
				Debug.LogError("Error in FirebaseStart");
			}
			else if (task.IsCompleted) {
				DataSnapshot snapshot = task.Result;				

				LocalFakeUsersDatabase lfudb = new LocalFakeUsersDatabase();

				foreach (var language in snapshot.Children)
				{
					lfudb.fakeusersInLanguages[language.Key] = new FakeUserList();
					foreach (var fakeUser in language.Children)
					{
						print(JsonUtility.FromJson<UserLite>(fakeUser.GetRawJsonValue()));
						lfudb.fakeusersInLanguages[language.Key].fakeUserList.Add(JsonUtility.FromJson<UserLite>(fakeUser.GetRawJsonValue()));
					}
				}
				PlayerPrefs.SetString("localFakeUsers", JsonUtility.ToJson(lfudb));

				LoadFakeUsers();
				fakeUsersDone = true;
				CompleteBar();
			}
		});
	}

	void GetQuestionsFromCloud()
	{
		FirebaseDatabase.DefaultInstance.GetReference("questionList").GetValueAsync().ContinueWith(task => {
			if (task.IsFaulted) {
				// Handle the error...
				Debug.LogError("Error in FirebaseStart");
			}
			else if (task.IsCompleted) {
				DataSnapshot snapshot = task.Result;				

				LocalQuestionDatabase lqdb = new LocalQuestionDatabase();

				foreach (var language in snapshot.Children)
				{
					lqdb.questionsInLanguages[language.Key] = new QuestionList();
					foreach (var question in language.Children)
					{
						lqdb.questionsInLanguages[language.Key].questionList.Add(JsonUtility.FromJson<Question>(question.GetRawJsonValue()));
					}
				}
				PlayerPrefs.SetString("localQuestions", JsonUtility.ToJson(lqdb));

				LoadQuestions();
				quesitonDone = true;
				CompleteBar();
			}
		});
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

	public void ChangeLayerTo(int layerCount)
	{
		uiLayer.GetComponent<Animator>().SetInteger("layerCount", layerCount);
	}

	IEnumerator GoToMenu()
	{
		ChangeLayerTo(2);
		yield return new WaitForSeconds(0.9f);
		SceneManager.LoadScene("MainMenu");
	}

	public void GetName()
	{
		if(!isCheckingName)
		{
			isCheckingName = true;
			infoText.gameObject.SetActive(false);
			if (nameField.text.Length < 3)
			{
				//infoText.text = "Kullanıcı adınız 3 karakterden kısa olamaz";
				infoText.text = I2.Loc.ScriptLocalization.Get("name less than 3 chars"); 
				infoText.gameObject.SetActive(true);
				isCheckingName = false;
				return;
			}

			// check if name exist
			FirebaseDatabase.DefaultInstance.GetReference("userList").GetValueAsync().ContinueWith(task => {
				if (task.IsFaulted) {
					Debug.LogError("Error in FirebaseStart");
					//infoText.text = "İnternet bağlantınızı kontrol edin";
					infoText.text = I2.Loc.ScriptLocalization.Get("check internet"); 
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
						//infoText.text = "Bu kullanıcı adı zaten kullanılıyor";
						infoText.text = I2.Loc.ScriptLocalization.Get("Name being used"); 
						infoText.gameObject.SetActive(true);
					}
					else
					{
						nameField.interactable = false;
						//ChangeLayerTo(2);	
						CreateUser();
					}
				}
				isCheckingName = false;
			});
		}
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

			StartCoroutine(GoToMenu());
		});	
	}

	public void Restart()
	{
		SceneManager.LoadScene("Register");
	}

	void ForceUpdate()
	{
		bool upToDate = false;
		FirebaseDatabase.DefaultInstance.GetReference("versionCodes").GetValueAsync().ContinueWith(task => {
			if (task.IsFaulted) {
				// Handle the error...
				Debug.LogError("Error in FirebaseStart");
			}
			else if (task.IsCompleted) {
				DataSnapshot snapshot = task.Result;				

				VersionNumbers vn = JsonUtility.FromJson<VersionNumbers>(snapshot.GetRawJsonValue());

				try{
					if (float.Parse(vn.gameVersionNo) <= float.Parse(Application.version))
					{
						print("uptodate");
						upToDate = true;
					}
					else if (vn.gameVersionNo.Equals(Application.version))
					{
						print("uptodate");
						upToDate = true;
					}
				}
				catch
				{
					if (vn.gameVersionNo.Equals(Application.version))
					{
						print("uptodate");
						upToDate = true;
					}
					Debug.LogError("Error in parsing version code to float");
				}

				if (!upToDate)
				{
					print("not uptodate");
					NotUpToDateObject.SetActive(true);
#if UNITY_ANDROID
					Application.OpenURL("market://details?id=com.digitalwords.trivia");
#elif UNITY_IPHONE
					Application.OpenURL("itms-apps://itunes.apple.com/app/id1454414970");
#elif UNITY_EDITOR
					Debug.LogError("Update version code data in Firebase. Go to the scene 'Firebase Data Control' and do it.");
#endif
				}
				else
				{
					try
					{
						if (float.Parse(vn.questionsVersionNo) <= float.Parse(PlayerPrefs.GetString("questionVersionNo")))
						{
							LoadQuestions();
							quesitonDone = true;
						}
						else if (vn.questionsVersionNo.Equals(PlayerPrefs.GetString("questionVersionNo")))
						{
							LoadQuestions();
							quesitonDone = true;
						}
						else
						{
							GetQuestionsFromCloud();
							PlayerPrefs.SetString("questionVersionNo", vn.questionsVersionNo);
						}
					}
					catch
					{
						if (vn.questionsVersionNo.Equals(PlayerPrefs.GetString("questionVersionNo")))
						{
							LoadQuestions();
							quesitonDone = true;
						}
						else
						{
							GetQuestionsFromCloud();
							PlayerPrefs.SetString("questionVersionNo", vn.questionsVersionNo);
						}
					}

					try
					{
						if (float.Parse(vn.fakeUsersVersionNo) <= float.Parse(PlayerPrefs.GetString("fakeUsersVersionNo")))
						{
							LoadFakeUsers();
							fakeUsersDone = true;
						}
						else if (vn.fakeUsersVersionNo.Equals(PlayerPrefs.GetString("fakeUsersVersionNo")))
						{
							LoadFakeUsers();
							fakeUsersDone = true;
						}
						else
						{
							GetFakeUsersFromCloud();
							PlayerPrefs.SetString("fakeUsersVersionNo", vn.fakeUsersVersionNo);
						}
					}
					catch
					{
						if (vn.fakeUsersVersionNo.Equals(PlayerPrefs.GetString("fakeUsersVersionNo")))
						{
							LoadFakeUsers();
							fakeUsersDone = true;
						}
						else
						{
							GetFakeUsersFromCloud();
							PlayerPrefs.SetString("fakeUsersVersionNo", vn.fakeUsersVersionNo);
						}
					}

					CompleteBar();
				}
			}
		});
	}

	void CompleteBar()
	{
		if (quesitonDone && fakeUsersDone)
		{
			loadingImage.GetComponent<Image>().DOFillAmount(1, 0.2f).OnComplete(Cont);
		}
	}

	public void GoToStore()
	{
#if UNITY_ANDROID
					Application.OpenURL("market://details?id=com.digitalwords.trivia");
#elif UNITY_IPHONE
					Application.OpenURL("itms-apps://itunes.apple.com/app/id1454414970");
#endif		
	}
}