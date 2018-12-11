using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	[Header("UI Elements")]
	public Text questionText;
	public Text[] optionTexts;
	public GameObject bidButtonsParent;
	
	[Header ("In Game Variables")]
	int turnCount = 0;
	int gameState = 0; // 0 = answers come, 1 = bidding, 2 = answering
	Question currentQuestion;
	float bidTimer;

	[Header ("User Variables")]
	int playingPlayerId;
	int playerScore, opponentScore = 0;
	int playerBid, opponentBid, playerBidTime, opponentBidTime;

	void Start () {
		// I may choose 5 questions at the start?
		GetNextQuesiton();
		ShowChoices();
	}

	void Update()
	{
		if (gameState == 1)
		{
			bidTimer += Time.deltaTime;
		}
	}

	void GetNextQuesiton()
	{
		// question must be chosen so that players haven't seen them

		// demo
		Question tempQ = new Question();
		tempQ.questionText = "Cevabı ne bu sorunun?";
		tempQ.option1 = "Bu değil";
		tempQ.option2 = "Bu doğru";
		tempQ.option3 = "Bu da değil";
		tempQ.option4 = "Bu hiç değil";
		tempQ.rightAnswerIndex = 2;

		currentQuestion = tempQ;
		// question must be added to players' seen question list TODO
	}

	void ShowChoices()
	{
		optionTexts[0].text = currentQuestion.option1;
		optionTexts[1].text = currentQuestion.option2;
		optionTexts[2].text = currentQuestion.option3;
		optionTexts[3].text = currentQuestion.option4;
	}

	void IncreaseGameState()
	{
		if (gameState == 0) // switch to bidding state
		{
			bidButtonsParent.SetActive(true);
		}
		if (gameState == 1)

		bidTimer = Time.TimeSinceLevelLoad();
		gameState++;
	}

	public void MakeBid(int amount)
	{
		playerBid = amount;
		playerBidTime = Time.TimeSinceLevelLoad() - bidTimer;
		// if both bid's made, go to the next gameState
	}

	public void MakeBidBot(int amount)
	{
		opponentBid = amount;
		opponentBidTime = Time.TimeSinceLevelLoad() - bidTimer;
	}
}