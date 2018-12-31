using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class BotManager : MonoBehaviour {

	GameManager gameManager;
    QuestionManager questionManager;
    public float botMinBidTime = 0.3f;
    public float botMaxBidTime = 3.5f;   
    public float botMinAnswerTime = 2f;
    public float botMaxAnswerTime = 8f;
    public float baseRightAnswerPercentage = 0.625f;
    public float difficultyEffectOnRightAnswer = 0.08f; // decreases the chance of answering right by this amount for every difficulty level
    public float choiceCountMultiplier = 0.125f; // the increases the chance of answering right the less choices there are
    public float randomBidChance = 0.2f;
    public float bidDelayAmount = 1.5f; // if question is hard or if AI has scored 2 points, it gives bid in longer time

	void Start()
	{
		gameManager = Camera.main.GetComponent<GameManager>();
        questionManager = Camera.main.GetComponent<QuestionManager>();
	}

	public void Bid()
	{
        // AI is winning or question is so hard, so, we give player a higher chance to win
        if (gameManager.opponentScore == 2 ||gameManager.currentQuestion.difficulty >= questionManager.difficultyLevelCount - 2 ) 
        {
            StartCoroutine(DelayedBid(UnityEngine.Random.Range(botMinBidTime, botMaxBidTime + bidDelayAmount)));
        }
        else
        {
        	StartCoroutine(DelayedBid(UnityEngine.Random.Range(botMinBidTime, botMaxBidTime)));
        }
	}

	IEnumerator DelayedBid(float delay)
	{
		yield return new WaitForSeconds(delay);
		int choice;
        int rnd = UnityEngine.Random.Range(0, 100);
        if(rnd < randomBidChance)
        {
            choice = UnityEngine.Random.Range(0,3);
        }
        else
        {
            if (gameManager.playerScore == 0 && gameManager.opponentScore == 2)
            {
                choice = UnityEngine.Random.Range(1,3);
            }
            else if (gameManager.playerScore == 2 && gameManager.opponentScore == 0)
            {
                choice = UnityEngine.Random.Range(0,2);
            }
            else if (gameManager.currentQuestion.difficulty == 0) // easeist level question
            {
                choice = UnityEngine.Random.Range(1,3);
            }
            else if (gameManager.currentQuestion.difficulty >= questionManager.difficultyLevelCount - 2) // hardest two level question
            {
                choice = UnityEngine.Random.Range(0,2);
            }
            else
            {
                choice = UnityEngine.Random.Range(0,3);
            }
        }
		gameManager.MakeBidBot(choice);
    }

    public void Choose()
    {
        StartCoroutine(DelayedChoose(UnityEngine.Random.Range(botMinAnswerTime, botMaxAnswerTime)));
    }

    IEnumerator DelayedChoose(float delay)
    {
        yield return new WaitForSeconds(delay);
        List<int> availableWrongChoices = new List<int>();
        for(int i = 0; i < gameManager.optionTexts.Length; i++)
        {
            if (gameManager.optionTexts[i].transform.parent.GetComponent<Button>().enabled && i != gameManager.currentQuestion.rightAnswerIndex)
            {
                availableWrongChoices.Add(i);
            }
        }

        float rnd = UnityEngine.Random.Range(0f, 1000.0f);
        float rightChance = baseRightAnswerPercentage - (difficultyEffectOnRightAnswer * gameManager.currentQuestion.difficulty); // if question is hard, chance is lower by difficulty * difficultyEffectOnRightAnswer
        rightChance += (gameManager.optionTexts.Length - availableWrongChoices.Count + 1) * choiceCountMultiplier; // if there are less number of choices available, chance is higher by choiceCountMultiplier
        if (rnd <  rightChance * 1000)
        {
            gameManager.ChooseAnswerBot(gameManager.currentQuestion.rightAnswerIndex);
        }
        else
        {
            gameManager.ChooseAnswerBot(availableWrongChoices[UnityEngine.Random.Range(0, availableWrongChoices.Count)]);
        }
    }
}