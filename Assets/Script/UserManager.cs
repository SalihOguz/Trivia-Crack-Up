using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine.UI;

public class UserManager : MonoBehaviour {

    public string userId;
    public string userName;
    public int onlineScore;
    public int offlineScore;
    public int totalCoin;
    public int passCount;
    public List<User> userList;

    //public FlowManager flowManager;

    public GameObject gameSelection;
    public GameObject wait;
    public Text passCountText;
    public Text passCountTextIntro;
    public Text passCountTextMain;

    private FirebaseApp fApp;
    private DatabaseReference mDatabaseRef;

    // Use this for initialization
    void Start() {
        // if(Application.platform == RuntimePlatform.OSXPlayer) {
        //     userName = "MY MAC";
        //     userId = "-101";
        //     onlineScore = 5555;
        //     offlineScore = 7777;
        //     totalCoin = 10000;

        //     flowManager.refreshUserNameUI(userName);
        //     flowManager.refreshCoinCountUI(totalCoin);
        //     if(SceneLoader.gameType == -1) {
        //         flowManager.intro.SetActive(false);
        //         flowManager.gameSelection.SetActive(true);
        //         flowManager.menuPanel.SetActive(true);
        //     }
        // } else {
            // Debug.Log("UserManager Started");
            // if (PlayerPrefs.HasKey("USER_NAME")) {
            //     userName = PlayerPrefs.GetString("USER_NAME");
            //     userId = PlayerPrefs.GetString("USER_ID");
            //     onlineScore = PlayerPrefs.GetInt("ONLINE_SCORE");
            //     offlineScore = PlayerPrefs.GetInt("OFFLINE_SCORE");
            //     totalCoin = PlayerPrefs.GetInt("TOTAL_COIN");
            //     Debug.Log("TOTAL: " + totalCoin);

            //     // flowManager.refreshUserNameUI(userName);
            //     // flowManager.refreshCoinCountUI(totalCoin);
            //     // if (SceneLoader.gameType == -1) {
            //     //     flowManager.intro.SetActive(false);
            //     //     flowManager.gameSelection.SetActive(true);
            //     //     flowManager.menuPanel.SetActive(true);
            //     // }
            // } else {
            //     updateUserCoin(300);
            // }
            // //SceneLoader.gameType = -1;
            // getUserScores();
        //}
    }

	public void InitializeDB (string userId)
	{
		fApp = FirebaseApp.DefaultInstance;

        #if UNITY_EDITOR
            fApp.SetEditorDatabaseUrl("https://trivia-challanger.firebaseio.com/");
        #endif

		mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

		this.userId = userId;
		this.enabled = true;
	}

    //TODO
    // public IEnumerator writeNewUser(string userId, string name, string email, int score) {
	// 	User user = new User(name, score);
    // 	string json = JsonUtility.ToJson(user);

    //     var task = mDatabaseRef.Child("Users").Child(userId).SetRawJsonValueAsync(json);

    //     yield return new WaitUntil(() => task.IsCompleted || task.IsFaulted);

    //     int status = -1;
	// 	if (task.IsFaulted) {
	// 		Debug.Log("WRITE ERROR");
	// 	} else if (task.IsCompleted) {
    //         status = 0;
	// 		Debug.Log("WRITE COMPLETED");
	// 	}

	// 	onlineScore = score;
	// 	userName    = name;
	// 	PlayerPrefs.SetString("USER_NAME", userName);
	// 	PlayerPrefs.SetInt("ONLINE_SCORE", onlineScore);

    //     yield return status;
	// }

    //TODO
    // public IEnumerator checkUserExist (InputField name)
	// {
    //     int status = -1; // -1: Error, 0: New User, 1: User Exist

	// 	if (fApp != null) {
	// 		if (ConnectionManager.isOnline) {
	// 			var task = FirebaseDatabase.GetInstance (fApp)
	// 			.GetReference ("Users")
	// 			.GetValueAsync ();

    //             yield return new WaitUntil(() => task.IsCompleted || task.IsFaulted);

    //             if (task.IsFaulted) {
    //                 Debug.Log("READ ERROR");
    //             } else if (task.IsCompleted) {
    //                 DataSnapshot snapshot = task.Result;
    //                 Debug.Log(snapshot.GetRawJsonValue());

    //                 foreach (DataSnapshot snap in snapshot.Children) {
    //                     Debug.Log(snap.Child("user").Value.ToString() + " - " + snap.Key.ToString());
    //                     if (snap.Child("user").Value.ToString().Equals(name.text.ToLower())) {
    //                         Debug.Log("USER EXIST");
    //                         status = 1;
    //                         break;
    //                     }
    //                 }

    //                 if(status == -1){
    //                     Debug.Log("NOT EXIST");
    //                     CoroutineWithData writeUserCR = new CoroutineWithData(this, writeNewUser(userId, name.text.ToLower(), "", 0));
    //                     yield return writeUserCR.coroutine;
    //                     status = (int)writeUserCR.result;

    //                 }
                   
    //             }
	// 		} else {
	// 			//UIController.ERROR_CODE = 102;
	// 			//UIController.refreshUI("ConnectionError", null);
	// 		}
	// 	} else {
	// 		//UIController.ERROR_CODE = 102;
	// 		//UIController.refreshUI("ConnectionError", null);
	// 	}

    //     yield return status;
	// }

    //TODO
	// public void getUserScores ()
	// {
	// 	userList = new List<User>();

	// 	FirebaseDatabase.GetInstance(fApp)
	// 	.GetReference("Users")
	// 	.OrderByChild("score")
	// 	.LimitToLast(10)
	// 	.GetValueAsync().ContinueWith(task => {
	// 		if (task.IsFaulted) {
	// 			Debug.Log("READ ERROR");
	// 		} else if (task.IsCompleted) {
	// 			DataSnapshot snapshot = task.Result;
	// 			Debug.Log(snapshot.GetRawJsonValue());

	// 			foreach(DataSnapshot snap in snapshot.Children) {
	// 				userList.Add(new User(snap.Child("user").Value.ToString(), int.Parse(snap.Child("score").Value.ToString())));
	// 			}

	// 			GetComponent<HighScoreManager>().enabled = true;
	// 		}

	// 	});	
	// }

    //TODO
	// public void updateUserScore(int score) {
	// 	User user = new User(userName, onlineScore + score);
    // 	string json = JsonUtility.ToJson(user);

	// 	mDatabaseRef.Child("Users").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task => {
	// 		if (task.IsFaulted) {
	// 			Debug.Log("WRITE ERROR");
	// 		} else if (task.IsCompleted) {
	// 			PlayerPrefs.SetInt("ONLINE_SCORE", onlineScore + score);
	// 			Debug.Log("WRITE COMPLETED");
	// 		}
	// 	});
	// }

	// public void updateUserCoin(int coin) {
	// 	totalCoin = totalCoin + coin;
	// 	PlayerPrefs.SetInt("TOTAL_COIN", totalCoin);

    //     if (!gameSelection) {
    //         gameSelection = GameObject.Find("GameSelection");
    //         wait = gameSelection.transform.parent.GetChild(4).gameObject;
    //     }

    //     gameSelection.transform.GetChild(2).GetChild(1).GetComponent<Text>().text = totalCoin.ToString();
    //     wait.transform.GetChild(11).GetChild(1).GetComponent<Text>().text = totalCoin.ToString();
	// }


	// public void processOfflineScore ()
	// {
	// 	User offlineUser = new User(userName, offlineScore);
	// 	string json = JsonUtility.ToJson(offlineUser);

	// 	List<User> list = new List<User>();
	// 	mDatabaseRef.Child("OfflineUsers").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task => {
	// 		if (task.IsFaulted) {
	// 			Debug.Log("WRITE ERROR");
	// 		} else if (task.IsCompleted) {
	// 			Debug.Log("WRITE COMPLETED");

	// 			FirebaseDatabase.GetInstance(fApp)
	// 			.GetReference("OfflineUsers")
	// 			.OrderByChild("score")
	// 			.LimitToLast(10)
	// 			.GetValueAsync().ContinueWith(taskOffline => {
	// 				if (taskOffline.IsFaulted) {
	// 					Debug.Log("READ ERROR");
	// 				} else if (taskOffline.IsCompleted) {
	// 					DataSnapshot snapshot = taskOffline.Result;
	// 					Debug.Log(snapshot.GetRawJsonValue());

	// 					foreach(DataSnapshot snap in snapshot.Children) {
	// 						list.Add(new User(snap.Child("user").Value.ToString(), int.Parse(snap.Child("score").Value.ToString())));
	// 					}

	// 					GetComponent<HighScoreManager>().setHighScoreOffline(list);
	// 				}

	// 			});
	// 		}
	// 	});
	// }

	// public void updateOfflineScore (int score)
	// {
	// 	offlineScore = offlineScore + score;
	// 	PlayerPrefs.SetInt("OFFLINE_SCORE", offlineScore);
	// }
}
