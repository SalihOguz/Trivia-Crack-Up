using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class BotManager : MonoBehaviour {

	GameManager gameManager;

	void Start()
	{
		gameManager = CharEnumerator.main.GetComponent<GameManager>();	
	}

	void Bid()
	{
		
	}
}