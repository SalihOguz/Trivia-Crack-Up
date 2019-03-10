using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
public class FirebaseDataControl : MonoBehaviour {

	DatabaseReference reference;
	public VersionNumbers vn; 


	void Start()
	{
		// Set up the Editor before calling into the realtime database.
		#if UNITY_EDITOR
				FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://triviachallanger.firebaseio.com/");
		#endif

		// FirebaseDatabase.DefaultInstance.GetReference("versionCodes").GetValueAsync().ContinueWith(task => {
		// 	if (task.IsFaulted) {
		// 		// Handle the error...
		// 		Debug.LogError("Error in FirebaseStart");
		// 	}
		// 	else if (task.IsCompleted) {
		// 		DataSnapshot snapshot = task.Result;				

		// 		foreach (DataSnapshot i in snapshot.Children)
		// 		{
		// 			//dtc.ful.fakeUserList.Add(JsonUtility.FromJson<UserLite>(i.GetRawJsonValue()));
		// 		}
		// 	}
		// });
		vn.gameVersionNo = Application.version;
	}

	public void SendVersionData()
	{
		if(reference == null)
		{
			reference = FirebaseDatabase.DefaultInstance.RootReference;
		}
		reference.Child("versionCodes").SetRawJsonValueAsync(JsonUtility.ToJson(vn));
	}


}
