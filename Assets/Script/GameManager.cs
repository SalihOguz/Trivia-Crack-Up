using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	[Header("UI Elements")]
	public Text questionText;
	public Text[] optionTexts;
	public GameObject bidButtonsParent;
    public Text bidTimerText;
    public Text playerNameText;
    public Text opponentNameText;
    public Text playerMoneyText;
    public Text opponentMoneyText;
    public GameObject playerIndicator;
    public GameObject opponentIndicator;
    public Text questionTimerText;
    public GameObject playerScoreStarsParent;
    public GameObject opponentScoreStarsParent;
    public Sprite fullStarSprite;
    public GameObject questionLockObject;

    [Header ("In Game Variables")]
    int turnCount = 0;
    int gameState = 0; // 0 = answers come, 1 = bidding, 2 = answering
	Question currentQuestion;
	float bidTimer;
    float bidStartTime;
	public BotManager botManager;
    User player1;
    User player2;
    float questionTimer;

	[Header ("User Variables")]
	int playingPlayerId;
	int playerScore, opponentScore = 0;
	int playerBid, opponentBid = 0;
	float playerBidTime, opponentBidTime;

	[Header ("Gameplay Variables")]
	public float totalBiddingTime = 5f;
	public float choiceApperTime = 0.3f;
    public int[] bidAmounts = { 15, 35, 70, 100};
    public float questionAnsweringTime = 10f;
    public int totalTurnCount = 5;

    void Start () {
        Init();

        // I may choose 5 questions at the start?
        ProceedGame();
    }

    void Init()
    {
        // get user data and fill avatar etc
        User p1 = new User(0, "MustafaSalih", 1000);
        User p2 = new User(1, "Musab Bey", 2000);
        player1 = p1;
        player2 = p2;

        // put avatar

        playerNameText.text = player1.username;
        opponentNameText.text = player2.username;

        playerMoneyText.text = player1.totalCoin.ToString();
        opponentMoneyText.text = player2.totalCoin.ToString();
    }

	void ProceedGame()
	{
		if (gameState == 0) // chosing question and showing choices
		{
			GetNextQuesiton();
			StartCoroutine(ShowChoices());
            gameState++;
			return;
		}
		else if (gameState == 1) // bidding stage
		{
            // game state increased in evaluate bidding
            StartCoroutine(ShowBiddinButtons());
			return;
		}
        else if (gameState == 2) // show question and wait for answer
        {
            StartCoroutine(ShowQuestion());
        }
	}

	void Update()
	{
        //print(gameState);
		if (gameState == 1) // bidding
		{
			bidTimer += Time.deltaTime;
            bidTimerText.text =  (Mathf.CeilToInt(totalBiddingTime - bidTimer)).ToString();
            if (bidTimer >= totalBiddingTime && (playerBid == 0 || opponentBid == 0)) // bidding time ended
			{
				EvaluateBidding();
			}
		}
        if (gameState == 2) // answering
        {
            questionTimer += Time.deltaTime;
            questionTimerText.text = (Mathf.CeilToInt(questionAnsweringTime - questionTimer)).ToString();
            if (questionTimer >= questionAnsweringTime)
            {
                gameState++;  // to stop the timer
                ChangeActivePlayer();
            }
        }
	}

	void GetNextQuesiton()
	{
		// question must be chosen so that players haven't seen them TODO

		// demo
		Question tempQ = new Question();
		tempQ.questionText = "Cevabı ne bu sorunun?";
		tempQ.option1 = "Bu değil";
		tempQ.option2 = "Bu doğru";
		tempQ.option3 = "Bu da değil";
		tempQ.option4 = "Bu hiç değil";
		tempQ.rightAnswerIndex = 2;

		currentQuestion = tempQ;
        questionText.text = tempQ.questionText;
        // question must be added to players' seen question list TODO
    }

	IEnumerator ShowChoices()
	{
		optionTexts[0].text = currentQuestion.option1;
		yield return new WaitForSeconds(choiceApperTime);
		optionTexts[1].text = currentQuestion.option2;
		yield return new WaitForSeconds(choiceApperTime);
		optionTexts[2].text = currentQuestion.option3;
		yield return new WaitForSeconds(choiceApperTime);
		optionTexts[3].text = currentQuestion.option4;
		ProceedGame();
	}

	IEnumerator ShowBiddinButtons()
	{
		yield return new WaitForSeconds(0.3f);
		//play a sound TODO
		bidTimer = 0;
        bidStartTime = Time.timeSinceLevelLoad;
        playerBid = 0;
		opponentBid = 0;
        bidTimerText.text = totalBiddingTime.ToString();
        bidButtonsParent.SetActive(true);
		botManager.Bid();
	}

	void EvaluateBidding()
	{
        gameState++;
        if (Random.Range(0,1) == 0) // if both of them didn't choose, we will choose randomly? TODO
        {
            if (playerBid == 0)
            {
                MakeBid(0);
            }
            if (opponentBid == 0)
            {
                MakeBidBot(bidAmounts[0]);
            }
        }
        else
        {
            if (opponentBid == 0)
            {
                MakeBidBot(bidAmounts[0]);
            }
            if (playerBid == 0)
            {
                MakeBid(0);
            }
        }

		
		if (playerBid > opponentBid)
		{
			playingPlayerId = 0;
		}
		else if (playerBid < opponentBid)
		{
			playingPlayerId = 1;
		}
		else
		{
			if (playerBidTime <= opponentBidTime)
			{
				playingPlayerId = 0;
			}
			else
			{
				playingPlayerId = 1;
			}
		}

        if (playingPlayerId == 0)
        {
            player1.totalCoin += (playerBid + opponentBid);
            playerIndicator.SetActive(true);
        }
        else
        {
            player2.totalCoin += (playerBid + opponentBid);
            opponentIndicator.SetActive(true);
        }
        ProceedGame();
	}

	public void MakeBid(int index)
	{
		playerBid = bidAmounts[index];
		playerBidTime = Time.timeSinceLevelLoad - bidStartTime;
        bidButtonsParent.transform.GetChild(index).GetComponent<Image>().color = Color.green; // chosen amount must be visible after each player has chosen TODO

        player1.totalCoin -= bidAmounts[index];
        playerMoneyText.text = player1.totalCoin.ToString();

        if (opponentBid != 0 && gameState == 1)
		{
			EvaluateBidding();
		}
	}

	public void MakeBidBot(int amount)
	{
		opponentBid = amount;
		opponentBidTime = Time.timeSinceLevelLoad - bidStartTime;

        player2.totalCoin -= amount;
        opponentMoneyText.text = player2.totalCoin.ToString();

        if (playerBid != 0 && gameState == 1)
		{
			EvaluateBidding();
		}
	}

    IEnumerator ShowQuestion()
    {
        bidButtonsParent.SetActive(false);
        yield return new WaitForSeconds(choiceApperTime);
        questionText.gameObject.SetActive(true);
        questionTimer = 0;

        if (playingPlayerId == 1)
        {
            botManager.Choose();
        }
        questionLockObject.SetActive(false);
    }

    void ChangeActivePlayer()
    {
        if (playingPlayerId == 0)
        {
            playingPlayerId = 1;
            playerIndicator.SetActive(false);
            opponentIndicator.SetActive(true);
            botManager.Choose();
        }
        else
        {
            playingPlayerId = 0;
            playerIndicator.SetActive(true);
            opponentIndicator.SetActive(false);
        }
        questionTimer = 0;
        gameState = 2;  // to start the timer
    }

    public void ChooseAnswer(int index)
    {
        if (playingPlayerId == 0 && gameState == 2)
        {
            if (currentQuestion.rightAnswerIndex == index + 1) // Answered right! - rightAnswerIndex starts from 1
            {
                //optionTexts[index].transform.parent.GetComponent<Image>().color = Color.green; // TODO make a sprite change
                IncreaseScore();
                GoToTheNextTurn();
            }
            else
            {
                ChangeActivePlayer();
                optionTexts[index].transform.parent.GetComponent<Button>().interactable = false;
            }
        }
    }

    public void ChooseAnswerBot(int index)
    {
        if (currentQuestion.rightAnswerIndex == index + 1) // Answered right! - rightAnswerIndex starts from 1
        {
            //optionTexts[index].transform.parent.GetComponent<Image>().color = Color.green; // TODO make a sprite change
            IncreaseScore();
            GoToTheNextTurn();
        }
        else
        {
            ChangeActivePlayer();
            optionTexts[index].transform.parent.GetComponent<Button>().interactable = false;
        }
    }

    void IncreaseScore()
    {
        if (playingPlayerId == 0)
        {
            playerScore++;
            playerScoreStarsParent.transform.GetChild(playerScore-1).GetComponent<Image>().sprite = fullStarSprite;
        }
        else
        {
            opponentScore++;
            opponentScoreStarsParent.transform.GetChild(opponentScore-1).GetComponent<Image>().sprite = fullStarSprite;
        }
    }

    void GoToTheNextTurn()
    {
        if (turnCount < totalTurnCount && playerScore < 3 && opponentScore < 3)
        {
            turnCount++;
            StartCoroutine(CleanScreen());
        }
        else
        {
            print("Game Over");
            gameState = 4;
            // put winner on and finish the game TODO
        }
    }

    IEnumerator CleanScreen()
    {
        gameState = 3; // to stop the timer
        yield return new WaitForSeconds(1f);
        for(int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].text = "";
            optionTexts[i].transform.parent.GetComponent<Button>().interactable = true;
            optionTexts[i].transform.parent.GetComponent<Image>().color = Color.white;

            //bidButtonsParent.transform.GetChild(i).GetComponent<Image>().color = Color.white; // TODO remove this, lengths might change
        }

        questionText.gameObject.SetActive(false);
        questionLockObject.SetActive(true);
        playerIndicator.SetActive(false);
        opponentIndicator.SetActive(false);
        
        bidTimer = 0;
        questionTimer = 0;

        playerBid = 0;
        opponentBid = 0;

        gameState = 0;
        ProceedGame();
    }                                     
}                            