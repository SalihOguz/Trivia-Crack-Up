using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using TMPro;

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
    public Sprite[] bidSprites;
    public GameObject endScreen;
    public Text accumulatedMoneyText;
    public Button knowQuestionButton;
    public Button fiftyFiftyButton;
    public Text knowQuestionAmountText;
    public Text fiftyFiftyAmountText;
    public Text endGameInfoText;
    public Sprite[] retryButtonSprites;
    public GameObject retryButton;

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
    public bool userWantsAgain = false;

	[Header ("User Variables")]
	int playingPlayerId;
	public int playerScore, opponentScore = 0;
	int playerBid, opponentBid = 0;
	float playerBidTime, opponentBidTime;
    bool knowQuestionUsed = false;
    bool fiftyFiftyUsed = false;

	[Header ("Gameplay Variables")]
	public float totalBiddingTime = 10f;
	public float choiceApperTime = 0.4f;
    public int[] bidAmounts = { 15, 35, 70, 100};
    public float questionAnsweringTime = 10f;
    public int totalTurnCount = 5;

    [Header ("Question Data")]
    int[] chosenChoiceCounts = new int[4];
    int[] chosenBidIndexCounts = new int[3];
    float firstAnsweringTime = 0;
    float secondAnsweringTime = 0;

    void Start () {
        Init();
        StartCoroutine(DelayedStart());
    }

    void Init()
    {        
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

        knowQuestionAmountText.text = player1.knowQuestionSkillCount.ToString();
        fiftyFiftyAmountText.text = player1.fiftyFiftySkillCount.ToString();
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
        infoText.text = "Bahsini seç";
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

        for(int i = 0; i < bidAmounts.Length; i++)
        {
            if (opponentBid == bidAmounts[i])
            {
                bidButtonsParent.transform.GetChild(i).GetComponent<Image>().sprite = bidSprites[i];
                break;
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
        accumulatedMoneyText.text = totalMoneyAccumulated.ToString() + " COIN";

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
            bidButtonsParent.transform.GetChild(index).GetComponent<Image>().sprite = bidSprites[index];

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
            infoText.text = "<b><color=#FFEA00>" + player1.username + "</color></b>\ncevaplıyor";
        }
        else
        {
            infoText.text = "<b><color=#FFEA00>" + player2.username + "</color></b>\ncevaplıyor";
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
            fiftyFiftyButton.interactable = false;
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
        fiftyFiftyButton.interactable = false;
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

	public void KnowTheQuestion()
	{
        if (!knowQuestionUsed && player1.knowQuestionSkillCount > 0)
        {
    		ChooseAnswer(currentQuestion.rightAnswerIndex);
            knowQuestionUsed = true;
            knowQuestionButton.interactable = false;
            player1.knowQuestionSkillCount--;
            knowQuestionAmountText.text = player1.knowQuestionSkillCount.ToString();
        }
	}

    public void KnowTheQuestionBot()
    {
        ChooseAnswerBot(currentQuestion.rightAnswerIndex);
        // knowQuestionButton.interactable = false; // needs to be done for bot? TODO
    }

    public void DiasbleTwoChoices()
    {
        if(!fiftyFiftyUsed && GetAvaliableChoiceCount() == 4 && player1.fiftyFiftySkillCount > 0)
        {
            List<Button> enabledbuttons = new List<Button>();
            for(int i = 0; i < optionTexts.Length; i++)
            {
                if (optionTexts[i].transform.parent.gameObject.GetComponent<Button>().enabled && i != currentQuestion.rightAnswerIndex)
                {
                    enabledbuttons.Add(optionTexts[i].transform.parent.gameObject.GetComponent<Button>());
                }
            }
            int rnd = Random.Range(0,enabledbuttons.Count);
            enabledbuttons[rnd].enabled = false;
            enabledbuttons[rnd].gameObject.gameObject.GetComponent<Image>().sprite = choiceSprites[1];
            enabledbuttons.RemoveAt(rnd);

            rnd = Random.Range(0,enabledbuttons.Count);
            enabledbuttons[rnd].enabled = false;
            enabledbuttons[rnd].gameObject.gameObject.GetComponent<Image>().sprite = choiceSprites[1];

            fiftyFiftyButton.interactable = false;
            fiftyFiftyUsed = true;

            player1.fiftyFiftySkillCount--;
            fiftyFiftyAmountText.text = player1.fiftyFiftySkillCount.ToString();
        }
    }

    public void DisableTwoChoicesBot()
    {
        List<Button> enabledbuttons = new List<Button>();
        for(int i = 0; i < optionTexts.Length; i++)
        {
            if (optionTexts[i].transform.parent.gameObject.GetComponent<Button>().enabled && i != currentQuestion.rightAnswerIndex)
            {
                enabledbuttons.Add(optionTexts[i].transform.parent.gameObject.GetComponent<Button>());
            }
        }
        int rnd = Random.Range(0,enabledbuttons.Count);
        enabledbuttons[rnd].enabled = false;
        enabledbuttons[rnd].gameObject.gameObject.GetComponent<Image>().sprite = choiceSprites[1];
        enabledbuttons.RemoveAt(rnd);

        rnd = Random.Range(0,enabledbuttons.Count);
        enabledbuttons[rnd].enabled = false;
        enabledbuttons[rnd].gameObject.gameObject.GetComponent<Image>().sprite = choiceSprites[1];

        // fiftyFiftyButton.interactable = false;  // needs to be done for bot? TODO
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
        player1.playedGameCount++;
        accumulatedMoneyText.transform.parent.gameObject.SetActive(false);
        knowQuestionButton.transform.parent.gameObject.SetActive(false);
        fiftyFiftyButton.transform.parent.gameObject.SetActive(false);

        if(playerScore > opponentScore)
        {
            endScreen.transform.Find("Win").gameObject.SetActive(true);
            player1.totalCoin += totalMoneyAccumulated;
            player1.score += 5;
            player1.wonGameCount++;
            opponentScoreStarsParent.SetActive(false);
        }
        else
        {
            endScreen.transform.Find("Lose").gameObject.SetActive(true);
            player2.totalCoin += totalMoneyAccumulated;
            player1.score += 1;
            playerScoreStarsParent.SetActive(false);
        }
        endScreen.transform.Find("CoinText").GetComponent<TextMeshProUGUI>().text = totalMoneyAccumulated.ToString() + " " + "COIN";
        retryButton.GetComponent<Image>().sprite = retryButtonSprites[0];
        retryButton.transform.GetChild(0).GetComponent<Text>().text = "tekrar oyna";
        endGameInfoText.text = "";
        retryButton.GetComponent<Button>().interactable = true;
        botManager.WantAgain();

        SendUserData();
    }

    void SendUserData()
    {
        PlayerPrefs.SetString("userData", JsonUtility.ToJson(player1));
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
		reference.Child("userList").Child(player1.userId).SetRawJsonValueAsync(PlayerPrefs.GetString("userData"));
    }

    void SendQuestionData()
    {
        // save question data to firebase

        // aşağıdaki fonksiyondan örnek al
        // private void WriteNewScore(string userId, int score) {
        //     // Create new entry at /user-scores/$userid/$scoreid and at
        //     // /leaderboard/$scoreid simultaneously
        //     string key = mDatabase.Child("scores").Push().Key;
        //     LeaderBoardEntry entry = new LeaderBoardEntry(userId, score);
        //     Dictionary<string, Object> entryValues = entry.ToDictionary();

        //     Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
        //     childUpdates["/scores/" + key] = entryValues;
        //     childUpdates["/user-scores/" + userId + "/" + key] = entryValues;

        //     mDatabase.UpdateChildrenAsync(childUpdates);
        // }



        // DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
		// reference.Child("questionData").Child(currentQuestion.questionId.ToString()).Child .SetRawJsonValueAsync(PlayerPrefs.GetString("userData"));
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

        for(int i = 0; i < bidAmounts.Length; i++)
        {
            bidButtonsParent.transform.GetChild(i).GetComponent<Image>().sprite = bidSprites[bidSprites.Length-1];
        }

        questionText.gameObject.SetActive(false);
        questionLockObject.SetActive(true);
        bidIndicatorsParent.SetActive(false);

        playerIndicator.GetComponent<Image>().sprite = nameBGSprites[0];
        opponentIndicator.GetComponent<Image>().sprite = nameBGSprites[0];

        fiftyFiftyUsed = false;
        botManager.isFiftyFiftyUsed = false;
        fiftyFiftyButton.interactable = true;
        
        bidTimer = 0;
        questionTimer = 0;

        playerBid = 0;
        opponentBid = 0;

        gameState = 0;
        ProceedGame();
    }

    public void PlayAgain()
    {
        if (player1.totalCoin >= 30*5)
        {
            userWantsAgain = true;

            if (botManager.botWantsAgain)
            {
                StartCoroutine(DelayRestart());
            }
            else if (endGameInfoText.text != "Rakip kaçtı!")
            {
                retryButton.GetComponent<Image>().sprite = retryButtonSprites[1];
                endGameInfoText.text = "Rakibe istek gönderildi";
                retryButton.transform.GetChild(0).GetComponent<Text>().text = "bekleniyor";
                retryButton.GetComponent<Button>().interactable = false;
            }
            else
            {
                GoToMenu(); // TODO load player choose
            }
        }
        else
        {
            endGameInfoText.text = "Yetersiz bakiye! Marketten daha fazla coin alabilirsiniz";
        }
    }

    public void BotWantsAgain()
    {
        if (player1.totalCoin >= 30*5)
        {
            retryButton.GetComponent<Image>().sprite = retryButtonSprites[0];
            endGameInfoText.text = "Rakip tekrar oynamak istiyor!";
            retryButton.transform.GetChild(0).GetComponent<Text>().text = "tekrar oyna";
             retryButton.GetComponent<Button>().interactable = true;
        }
    }

    public void BotRunAway()
    {
        if (player1.totalCoin >= 30*5)
        {
            retryButton.GetComponent<Image>().sprite = retryButtonSprites[0];
            endGameInfoText.text = "Rakip kaçtı!";
            retryButton.transform.GetChild(0).GetComponent<Text>().text = "yeni oyun";
            retryButton.GetComponent<Button>().interactable = true;
            // retryButton.GetComponent<Button>().onClick.RemoveAllListeners();
            // retryButton.GetComponent<Button>().onClick.AddListener(delegate{GoToMenu();});
        }
    }

    public IEnumerator DelayRestart()
    {
        retryButton.GetComponent<Image>().sprite = retryButtonSprites[0];
        retryButton.GetComponent<Button>().interactable = false;
        endGameInfoText.text = "İstek kabul edildi";
        retryButton.transform.GetChild(0).GetComponent<Text>().text = "başlıyor";
        yield return new WaitForSeconds(1f);
        Restart();
    }

    public void Restart()
    {
        SceneManager.LoadScene("Game");
    } 

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }    

    public int GetAvaliableChoiceCount()
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