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
    public float minSkillUseTime = 1f;
    public float maxSkillUseTime = 4f;
    public float chanceOfUsingKnowQuestion = 0.05f;
    public float chanceOfUsingDisableTwo = 0.05f;
    public bool isFiftyFiftyUsed = false;
    bool knowQuestionUsed = false;
    public float minReplayClickTime = 1f;
    public float maxReplayClickTime = 6f;
    public bool botWantsAgain = false;
    public float revengeAcceptenceRate = 0.7f;
    public float revengeOfferRate = 0.45f;
    public float minRunAwayTime = 10f;
    public float maxRunAwayTime = 15f;

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
        // using skills before answering
        float skillUseTime = 0;
        float rnd = UnityEngine.Random.Range(0, 100);
        if (rnd < chanceOfUsingKnowQuestion * 100 && gameManager.GetAvaliableChoiceCount() == 4 && !isFiftyFiftyUsed && gameManager.currentQuestion.difficulty >= 2) // use fiftyFifty skill
        {
            isFiftyFiftyUsed = true;
            skillUseTime += UnityEngine.Random.Range(minSkillUseTime, maxSkillUseTime);
            StartCoroutine(DelayedDisableTwoChoices(skillUseTime));
            skillUseTime++;  // to leave more time to answer after skill used to make it look like thinking
        }
        else if (rnd < (chanceOfUsingKnowQuestion + chanceOfUsingDisableTwo) * 100 && !knowQuestionUsed && gameManager.currentQuestion.difficulty >= 3) // use know question skill
        {
            knowQuestionUsed = true;
            skillUseTime += UnityEngine.Random.Range(minSkillUseTime, maxSkillUseTime);
            StartCoroutine(DelayedKnowQuestion(skillUseTime));
            skillUseTime++; // to leave more time to answer after skill used to make it look like thinking
        }

        // answering
        StartCoroutine(DelayedChoose(UnityEngine.Random.Range(botMinAnswerTime + skillUseTime, botMaxAnswerTime)));
    }

    IEnumerator DelayedKnowQuestion(float time)
    {
        yield return new WaitForSeconds(time);
        gameManager.KnowTheQuestionBot();     
    }

    IEnumerator DelayedDisableTwoChoices(float time)
    {
        yield return new WaitForSeconds(time);
        gameManager.DisableTwoChoicesBot();
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

    public void WantAgain()
    {
        StartCoroutine(DelayedWantAgain(UnityEngine.Random.Range(minReplayClickTime, maxReplayClickTime)));
        StartCoroutine(RunAway(UnityEngine.Random.Range(minRunAwayTime, maxRunAwayTime + 1))); // no one would stay more than runAwayTime after the game ended
    }

    public IEnumerator DelayedWantAgain(float time)
    {
        yield return new WaitForSeconds(time); 
        if (gameManager.userWantsAgain)
        {
            float rnd = UnityEngine.Random.Range(0, 100);
            if (rnd < revengeAcceptenceRate * 100)
            {
                botWantsAgain = true;
                StartCoroutine(gameManager.DelayRestart());
            }
            else
            {
                botWantsAgain = false;
                gameManager.BotRunAway();
            }
        }
        else
        {
            float rnd = UnityEngine.Random.Range(0, 100);
            if (rnd < revengeOfferRate * 100)
            {
                botWantsAgain = true;
                gameManager.BotWantsAgain();
            }
            else
            {
                botWantsAgain = false;
                gameManager.BotRunAway();
            }
        }
    }

    public IEnumerator RunAway(float time)
    {
        yield return new WaitForSeconds(time);
        botWantsAgain = false;
        gameManager.BotRunAway();
    }
}