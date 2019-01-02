using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase;
using Firebase.Unity.Editor;

public class FirebaseManager : MonoBehaviour {

	public string userId;

	//public UserManager userManager;

	Firebase.Auth.FirebaseAuth auth;
	Firebase.Auth.FirebaseUser user;

	// Use this for initialization
	void Start () {
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;

        if (Application.platform == RuntimePlatform.OSXPlayer) {
            //userManager.enabled = true;
        } else {
            InitializeFirebase();
        }
	}

	// Handle initialization of the necessary firebase modules:
	void InitializeFirebase ()
	{
		Debug.Log ("Setting up Firebase Auth");
		auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
		auth.StateChanged += AuthStateChanged;

		if (!PlayerPrefs.HasKey("userData")) {
			Debug.Log ("SignInAnonymously");
			SignInAnonymously ();
		} else {
            userId = JsonUtility.FromJson<User>(PlayerPrefs.GetString("userData")).userId;

			DatabaseReference mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
			//userManager.InitializeDB(userId);
		}
	}

	void SignInAnonymously ()
	{
		auth.SignInAnonymouslyAsync ().ContinueWith (task => {
			if (task.IsCanceled) {
				Debug.LogError ("SignInAnonymouslyAsync was canceled.");
				return;
			}

			if (task.IsFaulted) {
				Debug.LogError ("SignInAnonymouslyAsync encountered an error: " + task.Exception);
				return;
			}

			Firebase.Auth.FirebaseUser newUser = task.Result;
			Debug.Log(newUser.UserId);

			// update user id
			User newUserData = JsonUtility.FromJson<User>(PlayerPrefs.GetString("userData"));
			newUserData.userId = newUser.UserId;
			PlayerPrefs.SetString("userData", JsonUtility.ToJson(newUserData));
		});
	}

	// Track state changes of the auth object.
	void AuthStateChanged (object sender, System.EventArgs eventArgs)
	{
		Debug.Log("CURRENT");

		if (auth.CurrentUser != user) {
			bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
			if (!signedIn && user != null) {
				Debug.Log ("Signed out " + user.UserId);
			}
			user = auth.CurrentUser;
			if (signedIn) {
				Debug.Log ("Signed in " + user.UserId);
				User userData = JsonUtility.FromJson<User>(PlayerPrefs.GetString("userData"));

				if (userData.userId != "0") {
                    userData.userId = user.UserId;
					PlayerPrefs.SetString("userData", JsonUtility.ToJson(userData));
				} else {
					userId = JsonUtility.FromJson<User>(PlayerPrefs.GetString("userData")).userId;
				}

				DatabaseReference mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
				//userManager.InitializeDB(userId);
			}
		}
	}

	void OnDestroy ()
	{
        Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;

		auth.StateChanged -= AuthStateChanged;
		auth = null;
	}

    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    }

}
