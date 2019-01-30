using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour {

    #region Variables
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
    public GameObject noInternetMenu;
    public GameObject questionCountdown;
    public GameObject animStar;
    public GameObject playerCoinsParent;
    public GameObject opponentCoinsParent;
    public GameObject chestCoinsParent;
    public GameObject tutorialPanel;
    public GameObject tutorialHandBid;
    public GameObject tutorialHandQuestion;
    public GameObject emojiBg;
    public GameObject emojiButton;
    public GameObject showEmojiObject;
    public GameObject botEmojiButton;
    public GameObject botShowEmojiObject;

    [Header ("In Game Variables")]
    [HideInInspector]
    public Question currentQuestion;
    int turnCount = 0;
    int gameState = 0; // 0 = answers come, 1 = bidding, 2 = answering
	float bidTimer;
    float bidStartTime;
    [HideInInspector]
	public BotManager botManager;
    [HideInInspector]
    public User player1;
    User player2;
    float questionTimer;
    int totalMoneyAccumulated = 0;
    [HideInInspector]
    public bool userWantsAgain = false;
    float disconnectedSec = 0;
    SoundManager soundManager;
    MusicManager musicManager;

	[Header ("User Variables")]
    [HideInInspector]
    public int playerScore = 0;
    [HideInInspector]
    public int opponentScore = 0;
	int playingPlayerId;
	int playerBid, opponentBid = 0;
	float playerBidTime, opponentBidTime;
    bool knowQuestionUsed = false;
    bool fiftyFiftyUsed = false;

	[Header ("Gameplay Variables")]
	public float totalBiddingTime = 5f;
	public float choiceApperTime = 0.4f;
    public int[] bidAmounts = { 15, 35, 70, 100};
    public float questionAnsweringTime = 10f;
    public int totalTurnCount = 7;
    public float maxDisconnectedSec = 3;

    [Header ("Question Data")]
    int[] chosenChoiceCounts = new int[4];
    int[] chosenBidIndexCounts = new int[3];
    float firstAnsweringTime = 0;
    float secondAnsweringTime = 0;

    #endregion

    void Start () 
    {
        if (GameObject.FindGameObjectWithTag("sound"))
        {
            soundManager = GameObject.FindGameObjectWithTag("sound").GetComponent<SoundManager>();
        }
        if (GameObject.FindGameObjectWithTag("music"))
        {
            musicManager = GameObject.FindGameObjectWithTag("music").GetComponent<MusicManager>();
        }

        PlayMusic(2);

        Init();
        StartCoroutine(AnimationDelay());
    }

    IEnumerator AnimationDelay()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(DelayedStart());
        //StartCoroutine(CheckConnection()); // TODO uncomment
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

    IEnumerator CheckConnection()
	{
		if (!ConnectionManager.isOnline)
		{
			print("notOnline");
			disconnectedSec++;
			if (disconnectedSec >= maxDisconnectedSec)
			{
				noInternetMenu.SetActive(true);
                Time.timeScale = 0f;
			}
		}
        yield return new WaitForSeconds(1f);
        StartCoroutine(CheckConnection());
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
            if (bidTimer >= totalBiddingTime) // bidding time ended // && (playerBid == 0 || opponentBid == 0) 
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
        PlayMusic(1);

        infoText.text = "Teklifini Yap!";
		bidTimer = 0;
        bidStartTime = Time.timeSinceLevelLoad;
        playerBid = 0;
		opponentBid = 0;
        bidTimerText.text = totalBiddingTime.ToString();
        bidButtonsParent.SetActive(true);
		botManager.Bid();

        playerMoneyText.gameObject.SetActive(true);
        opponentMoneyText.gameObject.SetActive(true);
        playerNameText.gameObject.SetActive(false);
        opponentNameText.gameObject.SetActive(false);

        if (PlayerPrefs.GetInt("IsTutorialCompeted") == 0)
        {
            tutorialPanel.SetActive(true);
            tutorialPanel.transform.GetChild(1).GetComponent<SpriteRenderer>().DOColor(new Color(0,0,0,0.45f),0.5f);
            tutorialHandBid.SetActive(true);
            tutorialHandBid.transform.DOScale(Vector3.one * 0.8f, 0.5f).SetLoops(-1,LoopType.Yoyo);
        }
	}

	void EvaluateBidding()
	{
        gameState++;

        if (Random.Range(0,1) == 0 || PlayerPrefs.GetInt("IsTutorialCompeted") == 0) // if both of them didn't choose or in tutorial, we will choose randomly? TODO
        {
            if (playerBid == 0)
            {
                MakeBid(0);
            }
            if (opponentBid == 0)
            {
                MakeBidBot(0);
            }
        }
        else
        {
            if (opponentBid == 0)
            {
                MakeBidBot(0);
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

        SendCoinsToChest();

        yield return new WaitForSeconds(4f);

        player2.totalCoin -= opponentBid;
        player1.totalCoin -= playerBid;
        totalMoneyAccumulated += playerBid + opponentBid;

        playerMoneyText.text = player1.totalCoin.ToString();
        opponentMoneyText.text = player2.totalCoin.ToString();
        accumulatedMoneyText.text = totalMoneyAccumulated.ToString() + " COIN";

        playerMoneyText.gameObject.SetActive(false);
        opponentMoneyText.gameObject.SetActive(false);
        playerNameText.gameObject.SetActive(true);
        opponentNameText.gameObject.SetActive(true);

        PlaySound(1);

        if (PlayerPrefs.GetInt("IsTutorialCompeted") == 0)
        {
            tutorialPanel.transform.GetChild(1).GetComponent<SpriteRenderer>().DOColor(new Color(0,0,0,0f),0.5f).OnComplete(delegate()
            {
                tutorialPanel.SetActive(false);
            });
            tutorialHandBid.SetActive(false);
        }

        ProceedGame();
    }

    void SendCoinsToChest()
    {
        PlaySound(2);

        // reset previous positions
        foreach (Transform i in playerCoinsParent.transform)
        {
            i.localPosition = Vector3.zero;
            i.gameObject.SetActive(true);
        }

        foreach (Transform i in opponentCoinsParent.transform)
        {
            i.localPosition = Vector3.zero;
            i.gameObject.SetActive(true);
        }

        // coin animation
        int p1 = player1.totalCoin;
        int p2 = player2.totalCoin;
        int tot = totalMoneyAccumulated;

        for(int i = 0; i < Mathf.Max(playerBid/10, opponentBid/10); i++)
        {
            if (i < playerBid/10)
            {
                int no = i;
                playerCoinsParent.transform.GetChild(i).DOMove(accumulatedMoneyText.transform.parent.GetChild(0).position, 1f).SetEase(Ease.InCubic).SetDelay(0.15f*i).OnComplete(delegate() { 
                    tot += 10;
                    accumulatedMoneyText.text = tot + " COIN";
                    playerCoinsParent.transform.GetChild(no).gameObject.SetActive(false);
                 }).OnStart(delegate() {
                    p1 -= 10;
                    playerMoneyText.text = p1.ToString();
                 });
            }
            if (i < opponentBid/10)
            {
                int no = i;
                opponentCoinsParent.transform.GetChild(i).DOMove(accumulatedMoneyText.transform.parent.GetChild(0).position, 1f).SetEase(Ease.InCubic).SetDelay(0.15f*i).OnComplete(delegate() { 
                    tot += 10;
                    accumulatedMoneyText.text = tot + " COIN";
                    playerCoinsParent.transform.GetChild(no).gameObject.SetActive(false);
                 }).OnStart(delegate() {
                    p2 -= 10;
                    opponentMoneyText.text = p2.ToString();
                 });;
            }
            if (i < playerBid/10 || i < opponentBid/10)
            {
                accumulatedMoneyText.transform.parent.GetChild(0).DOShakeRotation(0.1f, new Vector3(0,0,20f),90,90).SetDelay(1f + 0.15f*i);
                accumulatedMoneyText.transform.parent.GetChild(0).DOShakeScale(0.1f, new Vector3(0.3f, 0.3f, 0f),50,50).SetDelay(1f + 0.15f*i);
            }
        }
    }

	public void MakeBid(int index)
	{
        if (playerBid == 0) // didn't make a bid before
        {
            PlaySound(0);

            playerBid = bidAmounts[index];
            playerBidTime = Time.timeSinceLevelLoad - bidStartTime;

            // indicators
            bidIndicatorsParent.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = playerBidTime.ToString("0.00");
            Vector3 indicatorPos = bidIndicatorsParent.transform.GetChild(0).transform.position;
            bidIndicatorsParent.transform.GetChild(0).transform.position = new Vector3(indicatorPos.x, bidIndicatorsParent.transform.parent.GetChild(index).position.y, 0);
            bidButtonsParent.transform.GetChild(index).GetComponent<Image>().sprite = bidSprites[index];
            
            bidButtonsParent.GetComponent<Animator>().SetTrigger("clicked");

            // if (opponentBid != 0 && gameState == 1) // commented because we want 5 sec to be completed
            // {
            //     EvaluateBidding();
            // }
            if (PlayerPrefs.GetInt("IsTutorialCompeted") == 0)
            {
                tutorialPanel.transform.GetChild(1).GetComponent<SpriteRenderer>().DOColor(new Color(0,0,0,0f),0.5f).OnComplete(delegate()
                {
                    tutorialPanel.SetActive(false);
                    tutorialPanel.transform.GetChild(0).gameObject.SetActive(false);
                    tutorialPanel.transform.GetChild(2).gameObject.SetActive(true);
                });
                tutorialHandBid.SetActive(false);
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

        // if (playerBid != 0 && gameState == 1) // commented because we want 5 sec to be completed
		// {
		// 	EvaluateBidding();
		// }
	}

    IEnumerator ShowQuestion()
    {
        bidButtonsParent.SetActive(false);
        yield return new WaitForSeconds(choiceApperTime);
        PlayMusic(2);

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
        questionCountdown.GetComponent<Animator>().enabled = true;

        if (PlayerPrefs.GetInt("IsTutorialCompeted") == 0)
        {
            print("aaaa");
            tutorialHandQuestion.transform.position = new Vector3(1.5f, optionTexts[currentQuestion.rightAnswerIndex].transform.parent.position.y, 0);
            tutorialPanel.transform.GetChild(2).position = new Vector3(0, optionTexts[currentQuestion.rightAnswerIndex].transform.parent.position.y, 0); //new Vector3(2.53f, optionTexts[currentQuestion.rightAnswerIndex].transform.parent.position.y, 0);

            tutorialPanel.SetActive(true);
            tutorialPanel.transform.GetChild(1).GetComponent<SpriteRenderer>().DOColor(new Color(0,0,0,0.45f),0.5f);
            tutorialHandQuestion.SetActive(true);
            tutorialHandQuestion.transform.DOScale(Vector3.one * 0.65f, 0.5f).SetLoops(-1,LoopType.Yoyo);
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

        questionCountdown.SetActive(false);
        questionCountdown.SetActive(true);
        questionCountdown.GetComponent<Animator>().enabled = true;
    }

    public void ChooseAnswer(int index)
    {
        questionCountdown.GetComponent<Animator>().enabled = false;

        if (playingPlayerId == 0 && gameState == 2)
        {
            fiftyFiftyButton.interactable = false;
            if (currentQuestion.rightAnswerIndex == index) // Answered right!
            {
                PlaySound(8);
                // TODO get and save data for question difficulty
                Vector3 pos = optionTexts[index].transform.position;
                animStar.transform.position = new Vector3(pos.x, pos.y, 0);

                IncreaseScore();
                GoToTheNextTurn();
                optionTexts[index].transform.parent.GetComponent<Image>().sprite = choiceSprites[2];
                player1.rightAnswersInDifficulties[currentQuestion.difficulty]++;

                if (!botShowEmojiObject.activeSelf)
                {
                    botManager.SendEmojiLose();
                }
            }
            else // Answered Wrong!
            {
                PlaySound(9);
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
                
                if (!botShowEmojiObject.activeSelf)
                {
                    botManager.SendEmojiWin();
                }
            }

            if (PlayerPrefs.GetInt("IsTutorialCompeted") == 0)
            {
                tutorialPanel.transform.GetChild(1).GetComponent<SpriteRenderer>().DOColor(new Color(0,0,0,0f),0.5f).OnComplete(delegate()
                {
                    tutorialPanel.SetActive(false);
                });
                tutorialHandQuestion.SetActive(false);
                PlayerPrefs.SetInt("IsTutorialCompeted", 1); // tutorial completed
            }
        }
    }

    public void ChooseAnswerBot(int index)
    {
        questionCountdown.GetComponent<Animator>().enabled = false;

        fiftyFiftyButton.interactable = false;
        if (currentQuestion.rightAnswerIndex == index) // Answered right!
        {
            PlaySound(8);
            Vector3 pos = optionTexts[index].transform.position;
            animStar.transform.position = new Vector3(pos.x, pos.y, 0);

            IncreaseScore();
            GoToTheNextTurn();
            optionTexts[index].transform.parent.GetComponent<Image>().sprite = choiceSprites[2];
           
            if (!botShowEmojiObject.activeSelf)
            {
                botManager.SendEmojiWin();
            }
        }
        else
        {
            PlaySound(9);
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

            if (!botShowEmojiObject.activeSelf)
            {
                botManager.SendEmojiLose();
            }
        }
    }

	public void KnowTheQuestion()
	{
        if (!knowQuestionUsed && player1.knowQuestionSkillCount > 0 &&  gameState == 2)
        {
            PlaySound(6);

    		ChooseAnswer(currentQuestion.rightAnswerIndex);
            knowQuestionUsed = true;
            knowQuestionButton.interactable = false;
            player1.knowQuestionSkillCount--;
            knowQuestionAmountText.text = player1.knowQuestionSkillCount.ToString();

            infoText.transform.Find("joker").gameObject.SetActive(true);
            infoText.text = "";
            StartCoroutine(KnowEnd());
        }
	}

    public void KnowTheQuestionBot()
    {
        ChooseAnswerBot(currentQuestion.rightAnswerIndex);
        infoText.transform.Find("joker").gameObject.SetActive(true);
        infoText.text = "";
        StartCoroutine(KnowEnd());
    }

    IEnumerator KnowEnd()
    {
        yield return new WaitForSeconds(1.9f);
        infoText.transform.Find("joker").gameObject.SetActive(false);
    }

    public void DiasbleTwoChoices()
    {
        if(!fiftyFiftyUsed && GetAvaliableChoiceCount() == 4 && player1.fiftyFiftySkillCount > 0 && gameState == 2 && playingPlayerId == 0)
        {
            PlaySound(10);

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

            infoText.transform.Find("disable").gameObject.SetActive(true);
            infoText.text = "";
            StartCoroutine(DisableEnd());
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

        infoText.transform.Find("disable").gameObject.SetActive(true);
        infoText.text = "";
        StartCoroutine(DisableEnd());
    }

    IEnumerator DisableEnd()
    {
        yield return new WaitForSeconds(1.9f);
        infoText.transform.Find("disable").gameObject.SetActive(false);
        if (playingPlayerId == 0)
        {
            infoText.text = "<b><color=#FFEA00>" + player2.username + "</color></b> cevaplıyor";
        }
        else
        {
            infoText.text = "<b><color=#FFEA00>" + player1.username + "</color></b> cevaplıyor";
        }
    }

    void IncreaseScore()
    {
        Transform destination;

        if (playingPlayerId == 0)
        {
            playerScore++;
            destination = playerScoreStarsParent.transform.GetChild(playerScore-1);
        }
        else
        {
            opponentScore++;
            destination = opponentScoreStarsParent.transform.GetChild(opponentScore-1);
        }

        // Star animation
        PlaySound(4);

        animStar.transform.localScale = Vector3.zero;
        animStar.SetActive(true);
        animStar.transform.DOJump(destination.position, 4, 1, 2f);
        animStar.transform.DORotate(new Vector3(0,0,1080), 2f, RotateMode.WorldAxisAdd).SetEase(Ease.InCubic).OnComplete(delegate(){
            destination.GetComponent<Image>().sprite = fullStarSprite; 
            animStar.SetActive(false);
            });
        animStar.transform.DOScale(Vector3.one * 0.3f, 0.5f).SetEase(Ease.OutQuint);
        animStar.transform.DOScale(Vector3.one * 0.159f, 1.5f).SetEase(Ease.OutQuint).SetDelay(1.5f); 
    }

    void GoToTheNextTurn()
    {
        if (turnCount < totalTurnCount && playerScore < Mathf.Floor(totalTurnCount/2) + 1 && opponentScore < Mathf.Floor(totalTurnCount/2) + 1)
        {
            turnCount++;
            StartCoroutine(CleanScreen());
        }
        else
        {
            print("Game Over");
            gameState = 4;
            StartCoroutine(GameOver());
        }
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2.1f);

        foreach (Transform i in playerCoinsParent.transform)
        {
            i.gameObject.SetActive(false);
        }

        foreach (Transform i in opponentCoinsParent.transform)
        {
            i.gameObject.SetActive(false);
        }

        endScreen.SetActive(true);
        questionLockObject.transform.parent.gameObject.SetActive(false);
        optionTexts[0].transform.parent.parent.gameObject.SetActive(false);
        infoText.gameObject.SetActive(false);
        player1.playedGameCount++;
        accumulatedMoneyText.transform.parent.gameObject.SetActive(false);
        knowQuestionButton.transform.parent.gameObject.SetActive(false);
        fiftyFiftyButton.transform.parent.gameObject.SetActive(false);

        playerMoneyText.gameObject.SetActive(true);
        opponentMoneyText.gameObject.SetActive(true);
        playerNameText.gameObject.SetActive(false);
        opponentNameText.gameObject.SetActive(false);

        print(player1.totalCoin + " " + totalMoneyAccumulated + " " + player1.totalCoin + totalMoneyAccumulated);

        if(playerScore > opponentScore)
        {
            StartEndGameCoinFlow();

            endScreen.transform.Find("Win").gameObject.SetActive(true);
            player1.totalCoin += totalMoneyAccumulated;
            player1.score += 5;
            player1.wonGameCount++;
            opponentScoreStarsParent.SetActive(false);
            PlaySound(7);

            if (!botShowEmojiObject.activeSelf)
            {
                botManager.SendEmojiLose();
            }
        }
        else
        {
            endScreen.transform.Find("Lose").gameObject.SetActive(true);
            player2.totalCoin += totalMoneyAccumulated;
            player1.score += 1;
            playerScoreStarsParent.SetActive(false);
            PlaySound(5);

            if (!botShowEmojiObject.activeSelf)
            {
                botManager.SendEmojiWin();
            }
        }
        endScreen.transform.Find("CoinText").GetComponent<TextMeshProUGUI>().text = totalMoneyAccumulated.ToString() + " " + "COIN";
        retryButton.GetComponent<Image>().sprite = retryButtonSprites[0];
        retryButton.transform.GetChild(0).GetComponent<Text>().text = "tekrar oyna";
        endGameInfoText.text = "";
        retryButton.GetComponent<Button>().interactable = true;
        botManager.WantAgain();
        
        PlayMusic(0);

        SendUserData();
    }

    void StartEndGameCoinFlow()
    {
        PlayDelayedSound(2, 2.1f);

        int p1 = player1.totalCoin;
        int p2 = player2.totalCoin;

        if(playerScore > opponentScore)
        {
            for (int i = 0; i <  Mathf.Clamp(Mathf.FloorToInt(totalMoneyAccumulated/30), 1, chestCoinsParent.transform.childCount); i++)
            {
                int no = i;
                chestCoinsParent.transform.GetChild(i).DOMove(playerCoinsParent.transform.position, 1f).SetEase(Ease.InCubic).SetDelay(2f + 0.1f*i).OnStart(delegate() {
                    p1 += 30;
                    playerMoneyText.text = p1.ToString();
                 }).OnComplete(delegate(){
                     chestCoinsParent.transform.GetChild(no).gameObject.SetActive(false);
                 });

                playerIndicator.transform.DOShakeRotation(0.1f, new Vector3(0,0,10f),50,50).SetDelay(3f + 0.1f*i);
                playerIndicator.transform.DOShakeScale(0.1f, new Vector3(0.3f, 0.3f, 0f),50,50).SetDelay(3f + 0.1f*i);
            }
            p1 += totalMoneyAccumulated % 30;
            playerMoneyText.text = p1.ToString();
        }
        else
        {
            for (int i = 0; i <  Mathf.Clamp(Mathf.FloorToInt(totalMoneyAccumulated/30), 1, chestCoinsParent.transform.childCount); i++)
            {
                int no = i;
                chestCoinsParent.transform.GetChild(i).DOMove(opponentCoinsParent.transform.position, 1f).SetEase(Ease.InCubic).SetDelay(2f + 0.1f*i).OnStart(delegate() {
                    p2 += 30;
                    opponentMoneyText.text = p2.ToString();
                 }).OnComplete(delegate(){
                     chestCoinsParent.transform.GetChild(no).gameObject.SetActive(false);
                 });

                opponentIndicator.transform.DOShakeRotation(0.1f, new Vector3(0,0,20f),90,90).SetDelay(3f + 1.1f*i);
                opponentIndicator.transform.DOShakeScale(0.1f, new Vector3(0.3f, 0.3f, 0f),50,50).SetDelay(3f + 1.1f*i);
            }
            p2 += totalMoneyAccumulated % 30;
            opponentMoneyText.text = p2.ToString();
        }
    }

    public void DoubleTheCoin()
    {
        player1.totalCoin += totalMoneyAccumulated;
        endScreen.transform.Find("CoinText").GetComponent<TextMeshProUGUI>().text = (totalMoneyAccumulated *2).ToString() + " " + "COIN";
        SendUserData();
    } 

    void SendUserData()
    {
        if (ConnectionManager.isOnline)
        {
            PlayerPrefs.SetString("userData", JsonUtility.ToJson(player1));
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference.Child("userList").Child(player1.userId).SetRawJsonValueAsync(PlayerPrefs.GetString("userData"));
        }
        else
        {
            Debug.LogError("No internet. Data couldn't be sent");
        }
    }

    public void DoubleCoinAdShow()
    {
        if (GameObject.Find("AdManager"))
        {
            GameObject.Find("AdManager").GetComponent<AdmobManager>().ShowAd("earn2x");
        }
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
        yield return new WaitForSeconds(2f);
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
        if (player1.fiftyFiftySkillCount > 0)
        {
            fiftyFiftyButton.interactable = true;
        }
        
        bidTimer = 0;
        questionTimer = 0;

        playerBid = 0;
        opponentBid = 0;

        gameState = 0;
        ProceedGame();
    }

    public void PlayAgain()
    {
        // foreach (Transform i in chestCoinsParent.transform)
        // {
        //     i.gameObject.SetActive(false);
        // }

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
                if (GameObject.Find("DataToCarry"))
                {
                    GameObject.Find("DataToCarry").GetComponent<DataToCarry>().mainMenuAnimLayerIndex = 4;
                }
                StartCoroutine(DelayedGoToMenu()); // TODO load player choose
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
        }
    }

    public IEnumerator DelayRestart()
    {
        endScreen.GetComponent<Animator>().SetTrigger("gameEnded");
        playerScoreStarsParent.transform.parent.GetComponent<Animator>().SetTrigger("gameEnded");
        opponentScoreStarsParent.transform.parent.GetComponent<Animator>().SetTrigger("gameEnded");

        retryButton.GetComponent<Image>().sprite = retryButtonSprites[0];
        retryButton.GetComponent<Button>().interactable = false;
        endGameInfoText.text = "İstek kabul edildi";
        retryButton.transform.GetChild(0).GetComponent<Text>().text = "başlıyor";
        yield return new WaitForSeconds(1.5f);

        Restart();
    }

    public void Restart()
    {
        PlaySound(0);
        SceneManager.LoadScene("Game");
    } 

    public void GoToMenu()
    {
        PlaySound(0);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitButton()
    {
        StartCoroutine(DelayedGoToMenu());
    }

    IEnumerator DelayedGoToMenu()
    {
        endScreen.GetComponent<Animator>().SetTrigger("gameEnded");
        playerScoreStarsParent.transform.parent.GetComponent<Animator>().SetTrigger("gameEnded");
        opponentScoreStarsParent.transform.parent.GetComponent<Animator>().SetTrigger("gameEnded");
        yield return new WaitForSeconds(1.5f);

        GoToMenu();
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

    void PlaySound(int id)
	{
        if(soundManager)
        {
            soundManager.GetComponent<SoundManager>().PlaySound(id);
        }
        else if (GameObject.FindGameObjectWithTag("sound"))
        {
            soundManager = GameObject.FindGameObjectWithTag("sound").GetComponent<SoundManager>();
            soundManager.GetComponent<SoundManager>().PlaySound(id);
        }
	} 

    IEnumerator PlayDelayedSound(int id, float time)
    {
        yield return new WaitForSeconds(time);
        PlaySound(id);
    }

    void PlayMusic(int id)
	{
        if(musicManager)
        {
            musicManager.GetComponent<MusicManager>().PlayMusic(id);
        }
        else if (GameObject.FindGameObjectWithTag("music"))
        {
            musicManager = GameObject.FindGameObjectWithTag("music").GetComponent<MusicManager>();
            musicManager.GetComponent<MusicManager>().PlayMusic(id);
        }
	}

    public void ShowEmojiBG()
    {
        emojiBg.SetActive(true);
        emojiButton.SetActive(false);
    }

    public void SendEmoji(int id) 
    {
        emojiBg.SetActive(false);
        showEmojiObject.GetComponent<Image>().sprite = emojiBg.transform.GetChild(id).GetComponent<Image>().sprite;
        showEmojiObject.SetActive(true);
        showEmojiObject.transform.eulerAngles = new Vector3(0,0,-15f);
        showEmojiObject.transform.DORotate(new Vector3(0,0,15f),0.5f).SetLoops(6, LoopType.Yoyo).SetEase(Ease.Linear).OnComplete(delegate(){
            emojiButton.SetActive(true);
            showEmojiObject.SetActive(false);
        });
        // TODO we can make different animations for every emotion
    }

    public void SendEmojiBot(int id)
    {
        botEmojiButton.SetActive(false);
        botShowEmojiObject.GetComponent<Image>().sprite = emojiBg.transform.GetChild(id).GetComponent<Image>().sprite;
        botShowEmojiObject.SetActive(true);
        botShowEmojiObject.transform.eulerAngles = new Vector3(0,0,-15f);
        botShowEmojiObject.transform.DORotate(new Vector3(0,0,15f),0.5f).SetLoops(6, LoopType.Yoyo).SetEase(Ease.Linear).OnComplete(delegate(){
            botEmojiButton.SetActive(true);
            botShowEmojiObject.SetActive(false);
        });    
    }
}                            