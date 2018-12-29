using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class BotManager : MonoBehaviour {

	GameManager gameManager;
    public float botMinBidTime = 0.3f;
    public float botMaxBidTime = 3.5f;   
    public float botMinAnswerTime = 2f;
    public float botMaxAnswerTime = 8f;
    public float baseRightAnswerPercentage = 0.625f;
    public float difficultyEffectOnRightAnswer = 0.08f; // decreases the chance of answering right by this amount for every difficulty level
    public float choiceCountMultiplier = 0.125f; // the increases the chance of answering right the less choices there are
	void Start()
	{
		gameManager = Camera.main.GetComponent<GameManager>();	
	}

	public void Bid()
	{
		StartCoroutine(DelayedBid(UnityEngine.Random.Range(botMinBidTime, botMaxBidTime)));
	}

	IEnumerator DelayedBid(float delay)
	{
		yield return new WaitForSeconds(delay);
		int choice = UnityEngine.Random.Range(0,3); // TODO not random, if winning 2 - 0 or easiest two level question put higer. if losing 2 - 0 or hardest two level question put less. 
        //Not certain, there is a chance to play randomly
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

        float rnd = UnityEngine.Random.Range(0f, 100.0f);
        float rightChance = baseRightAnswerPercentage - (difficultyEffectOnRightAnswer * gameManager.currentQuestion.difficulty);
        rightChance += (gameManager.optionTexts.Length - availableWrongChoices.Count + 1) * choiceCountMultiplier; // if there are less choices, chance is higher by choiceCountMultiplier
        if (rnd <  rightChance * 100)
        {
            gameManager.ChooseAnswerBot(gameManager.currentQuestion.rightAnswerIndex);
        }
        else
        {
            gameManager.ChooseAnswerBot(availableWrongChoices[UnityEngine.Random.Range(0, availableWrongChoices.Count)]);
        }
    }
}