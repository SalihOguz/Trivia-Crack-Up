using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class BotManager : MonoBehaviour {

	GameManager gameManager;
	//int[] bidAmounts = {15, 35, 70, 100};

	void Start()
	{
		gameManager = Camera.main.GetComponent<GameManager>();	
	}

	public void Bid()
	{
		StartCoroutine(DelayedBid(UnityEngine.Random.Range(0.3f, 2f)));
	}

	IEnumerator DelayedBid(float delay)
	{
		yield return new WaitForSeconds(delay);
		int choice = UnityEngine.Random.Range(0,3);
		gameManager.MakeBidBot(gameManager.bidAmounts[choice]);
		//gameManager.bidButtonsParent.transform.GetChild(choice).GetComponent<Image>().color = Color.red; // chosen amount must be visible AFTER each player has chosen TODO
    }

    public void Choose()
    {
        StartCoroutine(DelayedChoose(UnityEngine.Random.Range(1f, 4f)));
    }

    IEnumerator DelayedChoose(float delay)
    {
        yield return new WaitForSeconds(delay);
        for(int i = 0; i < gameManager.optionTexts.Length; i++)
        {
            if (gameManager.optionTexts[i].transform.parent.GetComponent<Button>().interactable)
            {
                gameManager.ChooseAnswerBot(i);
                break;
            }
        }
    }
}