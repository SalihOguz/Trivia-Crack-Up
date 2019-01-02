using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectionManager : MonoBehaviour {

	public float pingInterval;
	public static bool isOnline = false;

	void Start() {
		StartCoroutine(Ping());
	}
 
	IEnumerator Ping ()
	{
        WWW www = new WWW ("http://clients3.google.com/generate_204");
		yield return www;
		if (www.error == null) {
			isOnline = true;	
		} else {
			isOnline = false;
		}

		yield return new WaitForSeconds(pingInterval);

        StartCoroutine(Ping());
	}
}
