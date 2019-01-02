using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public GameObject bidIndicatorsParent;
    public Text infoText;
    public Sprite[] choiceSprites;
    public Sprite[] nameBGSprites;
    public GameObject endScreen;
    public Text accumulatedMoneyText;

    [Header ("In Game Variables")]
    int turnCount = 0;
    int gameState = 0; // 0 = answers come, 1 = bidding, 2 = answering
	public Question currentQuestion;
	float bidTimer;
    float bidStartTime;
	public BotManager botManager;
    public User player1;
    User player2;
    float questionTimer;
    int totalMoneyAccumulated = 0;

	[Header ("User Variables")]
	int playingPlayerId;
	public int playerScore, opponentScore = 0;
	int playerBid, opponentBid = 0;
	float playerBidTime, opponentBidTime;

	[Header ("Gameplay Variables")]
	public float totalBiddingTime = 10f;
	public float choiceApperTime = 0.4f;
    public int[] bidAmounts = { 15, 35, 70, 100};
    public float questionAnsweringTime = 10f;
    public int totalTurnCount = 5;

    void Start () {
        Init();
        StartCoroutine(DelayedStart());
    }

    void Init()
    {
        // get user data and fill avatar etc
        
        player1 = JsonUtility.FromJson<User>(PlayerPrefs.GetString("userData"));
        if (GameObject.Find("DataToCarry"))
        {
            player2 = GameObject.Find("DataToCarry").GetComponent<DataToCarry>().player2;
        }
        else
        {
            player2 = MakeFakeUser();
        }

        // put avatar
        Avatars av = Resources.Load<Avatars>("Data/Avatars");
        playerIndicator.transform.parent.GetChild(1).GetComponent<Image>().sprite = av.avatarSprites[player1.avatarId];
        opponentIndicator.transform.parent.GetChild(1).GetComponent<Image>().sprite = av.avatarSprites[player2.avatarId];

        playerNameText.text = player1.username;
        opponentNameText.text = player2.username;

        playerMoneyText.text = player1.totalCoin.ToString();
        opponentMoneyText.text = player2.totalCoin.ToString();
    }

    User MakeFakeUser()
    {
        TextAsset textAssset = Resources.Load<TextAsset>("Data/FakeUserList");
		FakeUserList ful = JsonUtility.FromJson<FakeUserList>(textAssset.text);
        UserLite fake = ful.fakeUserList[UnityEngine.Random.Range(0, ful.fakeUserList.Count)];

        User p2 = new User("1", fake.userName, fake.isMale, fake.totalCoin + 30 * (Random.Range(-65,65)));
        return p2;
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.2f);
        ProceedGame();
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
                print("lololoooo");
                gameState++;  // to stop the timer
                ChangeActivePlayer();
            }
        }
	}

	void GetNextQuesiton()
	{
        currentQuestion = GetComponent<QuestionManager>().ChooseQuestion();
        questionText.text = currentQuestion.questionText;
        player1.seenQuestionIds.Add(currentQuestion.questionId);
    }

	IEnumerator ShowChoices()
	{
        infoText.text = "Şıklar geliyor";

		optionTexts[0].text = currentQuestion.choice1;
		yield return new WaitForSeconds(choiceApperTime);
		optionTexts[1].text = currentQuestion.choice2;
		yield return new WaitForSeconds(choiceApperTime);
		optionTexts[2].text = currentQuestion.choice3;
		yield return new WaitForSeconds(choiceApperTime);
		optionTexts[3].text = currentQuestion.choice4;
		ProceedGame();
	}

	IEnumerator ShowBiddinButtons()
	{
		yield return new WaitForSeconds(choiceApperTime);
        infoText.text = "Soruyu görmek için\nortaya para koy";
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
                MakeBidBot(bidAmounts[bidAmounts.Length-1]);
            }
        }
        else
        {
            if (opponentBid == 0)
            {
                MakeBidBot(bidAmounts[bidAmounts.Length-1]);
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
            //player1.totalCoin += (playerBid + opponentBid);
            playerIndicator.GetComponent<Image>().sprite = nameBGSprites[1];
        }
        else
        {
            //player2.totalCoin += (playerBid + opponentBid);
            opponentIndicator.GetComponent<Image>().sprite = nameBGSprites[1];
        }

        StartCoroutine(EndBidState());
	}

    IEnumerator EndBidState()
    {
        // show bids
        bidIndicatorsParent.SetActive(true);

        // arrange info text
        if (playingPlayerId == 0)
        {
            infoText.text = "Soru <b><color=#FFEA00>" + player1.username + "</color></b> için geliyor";
        }
        else
        {
             infoText.text = "Soru <b><color=#FFEA00>" + player2.username + "</color></b> için geliyor";           
        }

        // money animation TODO make decrase and flow with animation
        playerMoneyText.gameObject.SetActive(true);
        opponentMoneyText.gameObject.SetActive(true);
        playerNameText.gameObject.SetActive(false);
        opponentNameText.gameObject.SetActive(false);

        player2.totalCoin -= opponentBid;
        player1.totalCoin -= playerBid;
        totalMoneyAccumulated += playerBid + opponentBid;

        playerMoneyText.text = player1.totalCoin.ToString();
        opponentMoneyText.text = player2.totalCoin.ToString();
        accumulatedMoneyText.text = totalMoneyAccumulated.ToString();

        yield return new WaitForSeconds(3f);

        playerMoneyText.gameObject.SetActive(false);
        opponentMoneyText.gameObject.SetActive(false);
        playerNameText.gameObject.SetActive(true);
        opponentNameText.gameObject.SetActive(true);

        ProceedGame();
    }

	public void MakeBid(int index)
	{
        if (playerBid == 0) // didn't make a bid before
        {
            playerBid = bidAmounts[index];
            playerBidTime = Time.timeSinceLevelLoad - bidStartTime;

            // indicators
            bidIndicatorsParent.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = playerBidTime.ToString("0.00");
            Vector3 indicatorPos = bidIndicatorsParent.transform.GetChild(0).transform.position;
            bidIndicatorsParent.transform.GetChild(0).transform.position = new Vector3(indicatorPos.x, bidIndicatorsParent.transform.parent.GetChild(index).position.y, 0);

            if (opponentBid != 0 && gameState == 1)
            {
                EvaluateBidding();
            }
        }
	}

	public void MakeBidBot(int index)
	{
		opponentBid = bidAmounts[index];
		opponentBidTime = Time.timeSinceLevelLoad - bidStartTime;

        // indicators
        bidIndicatorsParent.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = opponentBidTime.ToString("0.00");
        Vector3 indicatorPos = bidIndicatorsParent.transform.GetChild(1).transform.position;
        bidIndicatorsParent.transform.GetChild(1).transform.position = new Vector3(indicatorPos.x, bidIndicatorsParent.transform.parent.GetChild(index).position.y, 0);

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

        if (playingPlayerId == 0)
        {
            infoText.text = "<b><color=#FFEA00>" + player1.username + "</color></b> cevaplıyor";
        }
        else
        {
            infoText.text = "<b><color=#FFEA00>" + player2.username + "</color></b> cevaplıyor";
        }

        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].transform.parent.GetComponent<Button>().interactable = true;
        }
    }

    void ChangeActivePlayer()
    {
        if (playingPlayerId == 0)
        {
            playingPlayerId = 1;
            playerIndicator.GetComponent<Image>().sprite = nameBGSprites[0];
            opponentIndicator.GetComponent<Image>().sprite = nameBGSprites[1];
            
            // we can put a simple bounce animation to infotext TODO
            infoText.text = "<b><color=#FFEA00>" + player2.username + "</color></b> cevaplıyor";
            botManager.Choose();
        }
        else
        {
            playingPlayerId = 0;
            playerIndicator.GetComponent<Image>().sprite = nameBGSprites[1];
            opponentIndicator.GetComponent<Image>().sprite = nameBGSprites[0];

            // we can put a simple bounce animation to infotext TODO
            infoText.text = "<b><color=#FFEA00>" + player1.username + "</color></b> cevaplıyor";
        }

        questionTimer = 0;
        gameState = 2;  // to start the timer
    }

    public void ChooseAnswer(int index)
    {
        if (playingPlayerId == 0 && gameState == 2)
        {
            if (currentQuestion.rightAnswerIndex == index) // Answered right!
            {
                // TODO get and save data for question difficulty
                IncreaseScore();
                GoToTheNextTurn();
                optionTexts[index].transform.parent.GetComponent<Image>().sprite = choiceSprites[2];
                player1.rightAnswersInDifficulties[currentQuestion.difficulty]++;
            }
            else // Answered Wrong!
            {
                // TODO get and save data for question difficulty
                optionTexts[index].transform.parent.GetComponent<Button>().enabled = false;
                optionTexts[index].transform.parent.GetComponent<Image>().sprite = choiceSprites[1];
                player1.wrongAnswersInDifficulties[currentQuestion.difficulty]++;

                if (GetAvaliableChoiceCount() > 1)
                {
                  ChangeActivePlayer();                
                }
                else
                {
                    StartCoroutine(CleanScreen());
                }
            }
        }
    }

    public void ChooseAnswerBot(int index)
    {
        if (currentQuestion.rightAnswerIndex == index) // Answered right!
        {
            IncreaseScore();
            GoToTheNextTurn();
            optionTexts[index].transform.parent.GetComponent<Image>().sprite = choiceSprites[2];
        }
        else
        {
            optionTexts[index].transform.parent.GetComponent<Button>().enabled = false;
            optionTexts[index].transform.parent.GetComponent<Image>().sprite = choiceSprites[1];

            print(GetAvaliableChoiceCount());
            if (GetAvaliableChoiceCount() > 1)
            {
                ChangeActivePlayer();                
            }
            else
            {
               StartCoroutine(CleanScreen());
            }
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
            GameOver();
        }
    }

    void GameOver()
    {
        // make animation TODO
        endScreen.SetActive(true);
        questionLockObject.transform.parent.gameObject.SetActive(false);
        optionTexts[0].transform.parent.parent.gameObject.SetActive(false);
        infoText.gameObject.SetActive(false);

        if(playerScore > opponentScore)
        {
            endScreen.transform.GetChild(0).gameObject.SetActive(true);
            player1.totalCoin += totalMoneyAccumulated;
        }
        else
        {
            endScreen.transform.GetChild(1).gameObject.SetActive(true);
            player2.totalCoin += totalMoneyAccumulated;
        }
        endScreen.transform.GetChild(2).GetComponent<Text>().text = totalMoneyAccumulated.ToString();
        PlayerPrefs.SetString("userData", JsonUtility.ToJson(player1));
    }

    IEnumerator CleanScreen()
    {
        gameState = 3; // to stop the timer
        infoText.text = "";
        yield return new WaitForSeconds(1f);
        for(int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].text = "";
            optionTexts[i].transform.parent.GetComponent<Button>().enabled = true;
            optionTexts[i].transform.parent.GetComponent<Button>().interactable = false;
            optionTexts[i].transform.parent.GetComponent<Image>().sprite = choiceSprites[0];
        }

        questionText.gameObject.SetActive(false);
        questionLockObject.SetActive(true);
        bidIndicatorsParent.SetActive(false);

        playerIndicator.GetComponent<Image>().sprite = nameBGSprites[0];
        opponentIndicator.GetComponent<Image>().sprite = nameBGSprites[0];
        
        bidTimer = 0;
        questionTimer = 0;

        playerBid = 0;
        opponentBid = 0;

        gameState = 0;
        ProceedGame();
    }

    public void Restart()
    {
        SceneManager.LoadScene("Game"); // TODO player must play with the same opponent
    } 

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }    

    int GetAvaliableChoiceCount()
    {
        int count = 0;
        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (optionTexts[i].transform.parent.GetComponent<Button>().enabled)
            {
                count++;
            }
        }
        return count;
    }                                
}                            