using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using TMPro;
using DG.Tweening;
using I2.Loc;

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
    public GameObject tutorialHandQuestion;
    public GameObject emojiBg;
    public GameObject emojiButton;
    public GameObject showEmojiObject;
    public GameObject botEmojiButton;
    public GameObject botShowEmojiObject;
    public GameObject doublecoinButton;
    public GameObject tutorialTextObjects;
    public GameObject tutorialBidHand;
    public GameObject disconnectedScreen;

    [Header ("In Game Variables")]
    [HideInInspector]
    public Question currentQuestion;
    QuestionData currentQuestionData;
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
    SoundManager otherSoundManager;
    int startingPlayerId; // who wins the bid
    int totalBidPlayer = 0;
    int totalBidOpponent = 0;
    int startingCoin;
    DatabaseReference reference;
    int tutorialCount = 0;

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
	public float choiceApperTime = 0.75f;
    public int[] bidAmounts = { 15, 35, 70, 100};
    public float questionAnsweringTime = 15f;
    public int totalTurnCount = 7;
    public float maxDisconnectedSec = 3;

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
        if (GameObject.FindGameObjectWithTag("otherSound"))
        {
            otherSoundManager = GameObject.FindGameObjectWithTag("otherSound").GetComponent<SoundManager>();
        }
        

        PlayMusic(2);

        Init();
        StartCoroutine(AnimationDelay());

        accumulatedMoneyText.transform.parent.GetChild(4).GetChild(0).position = playerMoneyText.transform.GetChild(0).position;
        accumulatedMoneyText.transform.parent.GetChild(4).GetChild(1).position = opponentMoneyText.transform.GetChild(0).position;
        endScreen.transform.GetChild(2).GetChild(4).GetChild(0).position = playerMoneyText.transform.GetChild(0).position;
        endScreen.transform.GetChild(2).GetChild(4).GetChild(1).position = opponentMoneyText.transform.GetChild(0).position;


    }

    IEnumerator AnimationDelay()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(DelayedStart());
        
        #if ! UNITY_EDITOR
        StartCoroutine(CheckConnection());
        #endif
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
        playerIndicator.transform.parent.GetChild(1).GetChild(0).GetComponent<Image>().sprite = av.avatarSprites[player1.avatarId];
        opponentIndicator.transform.parent.GetChild(1).GetChild(0).GetComponent<Image>().sprite = av.avatarSprites[player2.avatarId];

        playerNameText.text = player1.username;
        opponentNameText.text = player2.username;

        playerMoneyText.text = player1.totalCoin.ToString();
        opponentMoneyText.text = player2.totalCoin.ToString();

        knowQuestionAmountText.text = player1.knowQuestionSkillCount.ToString();
        fiftyFiftyAmountText.text = player1.fiftyFiftySkillCount.ToString();

        endScreen.transform.Find("PlayerText").GetComponent<Text>().text = player1.username;
        endScreen.transform.Find("OpponentText").GetComponent<Text>().text = player2.username;

        startingCoin = player1.totalCoin;
    }

    public void UpdatePlayerAvatar()
    {
        Avatars av = Resources.Load<Avatars>("Data/Avatars");
        playerIndicator.transform.parent.GetChild(1).GetChild(0).GetComponent<Image>().sprite = av.avatarSprites[player1.avatarId];
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
        else if (reference == null)
        {
            print("reference was null");
            #if UNITY_EDITOR
			    FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://triviachallanger.firebaseio.com/");
		    #endif
            reference = FirebaseDatabase.DefaultInstance.RootReference;
        }
        yield return new WaitForSeconds(1f);
        StartCoroutine(CheckConnection());
	}

    User MakeFakeUser()
    {
        TextAsset textAssset = Resources.Load<TextAsset>("Data/FakeUserList/"+LocalizationManager.CurrentLanguageCode);
		FakeUserList ful = JsonUtility.FromJson<FakeUserList>(textAssset.text);
        UserLite fake = ful.fakeUserList[UnityEngine.Random.Range(0, ful.fakeUserList.Count)];

        User p2 = new User("1", fake.userName, fake.isMale, fake.totalCoin, fake.score);
        return p2;
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.2f);
        DisplayTutorial();
        ProceedGame();
    }

    void DisplayTutorial()
    {
        if (PlayerPrefs.GetInt("IsTutorialCompeted") == 0)
        {
            Time.timeScale = 0f;
            tutorialPanel.transform.GetChild(0).gameObject.SetActive(true);
            tutorialPanel.transform.GetChild(1).GetChild(tutorialCount).gameObject.SetActive(true);
            tutorialTextObjects.transform.GetChild(tutorialCount).gameObject.SetActive(true);
            tutorialPanel.transform.GetChild(0).GetComponent<SpriteRenderer>().DOColor(new Color(0,0,0,0.5f),0.3f).SetUpdate(true);

            if (tutorialCount == 4)
            {
                PlayerPrefs.SetInt("IsTutorialCompeted", 1);
            }
        }
    }

    public void TutorialDone()
    {
        Time.timeScale = 1f;
        tutorialPanel.transform.GetChild(0).gameObject.SetActive(false);
        tutorialPanel.transform.GetChild(1).GetChild(tutorialCount).gameObject.SetActive(false);
        tutorialTextObjects.transform.GetChild(tutorialCount).gameObject.SetActive(false);
        tutorialPanel.transform.GetChild(0).GetComponent<SpriteRenderer>().DOColor(new Color(0,0,0,0),0.3f).SetUpdate(true);

        if (tutorialCount == 2) // Bid
        {
            tutorialBidHand.SetActive(true);
            tutorialBidHand.transform.DOScale(Vector3.one * 0.65f, 0.5f).SetLoops(-1,LoopType.Yoyo);
        }

        tutorialCount++;

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
                StopMusic();
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
        currentQuestionData = new QuestionData();
        currentQuestion = GetComponent<QuestionManager>().ChooseQuestion();
        questionText.text = currentQuestion.questionText;
        player1.seenQuestionIds[LocalizationManager.CurrentLanguageCode].Add(currentQuestion.questionId);
    }

	IEnumerator ShowChoices()
	{
        infoText.text = I2.Loc.ScriptLocalization.Get("Choices coming");

		optionTexts[0].text = currentQuestion.choice1;
		yield return new WaitForSeconds(choiceApperTime);
		optionTexts[1].text = currentQuestion.choice2;
		yield return new WaitForSeconds(choiceApperTime);
		optionTexts[2].text = currentQuestion.choice3;
		yield return new WaitForSeconds(choiceApperTime);
		optionTexts[3].text = currentQuestion.choice4;

        if(playerScore + opponentScore == 0)
        {
            DisplayTutorial();
        }
		ProceedGame();
	}

	IEnumerator ShowBiddinButtons()
	{
		yield return new WaitForSeconds(choiceApperTime);
        PlayMusic(1);

        infoText.text = I2.Loc.ScriptLocalization.Get("Bid Now");
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

        yield return new WaitForSeconds(0.3f);
        if(playerScore + opponentScore == 0)
        {
            DisplayTutorial();
        }
	}

	void EvaluateBidding()
	{
        gameState++;

        if (Random.Range(0,1) == 0 || PlayerPrefs.GetInt("IsTutorialCompeted") == 0) // if both of them didn't choose or in tutorial, we choose randomly
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
        startingPlayerId = playingPlayerId;

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

        currentQuestionData.bidGivingTimes.Add(playerBidTime);

        StartCoroutine(EndBidState());
	}

    IEnumerator EndBidState()
    {
        PlayOtherSound(1);

        // show bids
        bidIndicatorsParent.SetActive(true);

        // arrange info text
        if (playingPlayerId == 0)
        {
            infoText.text = I2.Loc.ScriptLocalization.Get("Question is coming 1") + " <b><color=#FFEA00>" + player1.username + "</color></b> " + I2.Loc.ScriptLocalization.Get("Question is coming 2");
        }
        else
        {
             infoText.text = I2.Loc.ScriptLocalization.Get("Question is coming 1") + " <b><color=#FFEA00>" + player2.username + "</color></b> " + I2.Loc.ScriptLocalization.Get("Question is coming 2");           
        }

        yield return new WaitForSeconds(1f);

        SendCoinsToChest();

        yield return new WaitForSeconds(4f);

        player2.totalCoin -= opponentBid;
        player1.totalCoin -= playerBid;
        totalMoneyAccumulated += playerBid + opponentBid;

        playerMoneyText.text = player1.totalCoin.ToString();
        opponentMoneyText.text = player2.totalCoin.ToString();
        accumulatedMoneyText.text = totalMoneyAccumulated.ToString() + " " + I2.Loc.ScriptLocalization.Get("COIN");

        playerMoneyText.gameObject.SetActive(false);
        opponentMoneyText.gameObject.SetActive(false);
        playerNameText.gameObject.SetActive(true);
        opponentNameText.gameObject.SetActive(true);

        accumulatedMoneyText.transform.parent.GetChild(4).gameObject.SetActive(false);

        //PlaySound(1);

        // if (PlayerPrefs.GetInt("IsTutorialCompeted") == 0)
        // {
        //     tutorialPanel.transform.GetChild(1).GetComponent<SpriteRenderer>().DOColor(new Color(0,0,0,0f),0.5f).OnComplete(delegate()
        //     {
        //         tutorialPanel.SetActive(false);
        //     });
        // }

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

        float flowDelay = 0.5f/Mathf.Max(playerBid/30, opponentBid/30); //Mathf.Max(Mathf.Clamp(playerBid/10, 3, 10), Mathf.Clamp(opponentBid/10, 3, 10));

        accumulatedMoneyText.transform.parent.GetChild(1).DOShakeRotation(Mathf.Max(playerBid/30, opponentBid/30) * flowDelay, new Vector3(0,0,20f),90,90).SetDelay(1f).OnComplete(delegate(){
            tot += (playerBid) % 30;
            tot += (opponentBid) % 30;
            accumulatedMoneyText.text = tot + " " + I2.Loc.ScriptLocalization.Get("COIN");
        });
        accumulatedMoneyText.transform.parent.GetChild(1).DOShakeScale(Mathf.Max(playerBid/30, opponentBid/30) * flowDelay, new Vector3(0.3f, 0.3f, 0f),50,50).SetDelay(1f);

        for(int i = 0; i < Mathf.Max(playerBid/30, opponentBid/30); i++) // Mathf.Max(Mathf.Clamp(playerBid/10, 3, 10), Mathf.Clamp(opponentBid/10, 3, 10))
        {
            if (i < playerBid/30)
            {
                playerCoinsParent.transform.GetChild(i).DOMove(accumulatedMoneyText.transform.parent.GetChild(0).position, 1f).SetEase(Ease.InCubic).SetDelay(flowDelay*i).
                OnComplete(delegate() { 
                    tot += 30;
                    accumulatedMoneyText.text = tot + " " + I2.Loc.ScriptLocalization.Get("COIN");
                 }).OnStart(delegate() {
                    p1 -= 30;
                    playerMoneyText.text = p1.ToString();
                 });
                playerCoinsParent.transform.GetChild(i).DOScale(Vector3.one * 0.8f, 0.5f).SetEase(Ease.InCubic).SetDelay(flowDelay*i);
                playerCoinsParent.transform.GetChild(i).DOScale(Vector3.one * 0.4f, 0.5f).SetEase(Ease.InCubic).SetDelay(flowDelay*i + 0.5f);
            }
            if (i < opponentBid/30)
            {
                opponentCoinsParent.transform.GetChild(i).DOMove(accumulatedMoneyText.transform.parent.GetChild(0).position, 1f).SetEase(Ease.InCubic).SetDelay(flowDelay*i).OnComplete(delegate() { 
                    tot += 30;
                    accumulatedMoneyText.text = tot + " " + I2.Loc.ScriptLocalization.Get("COIN");
                 }).OnStart(delegate() {
                    p2 -= 30;
                    opponentMoneyText.text = p2.ToString();
                 });;
                //opponentCoinsParent.transform.GetChild(i).DOScale(Vector3.one * 0.8f, 0.5f).SetEase(Ease.Linear).SetDelay(flowDelay*i);
                //opponentCoinsParent.transform.GetChild(i).DOScale(Vector3.one * 0.4f, 0.5f).SetEase(Ease.Linear).SetDelay(flowDelay*i + 0.5f);
            }
        }
    }

    void SendCoinsToWinner(int playerId)
    {
        PlaySound(2);

        // reset previous positions
        foreach (Transform i in accumulatedMoneyText.transform.parent.GetChild(0))
        {
            i.localPosition = Vector3.zero;
            i.gameObject.SetActive(true);
        }

         // coin animation
        int p1;
        int tot = totalMoneyAccumulated;
        GameObject target;
        Text winnerText;
        GameObject indicator;

        accumulatedMoneyText.transform.parent.GetChild(4).gameObject.SetActive(true);

        if (playerId == 0)
        {
            target =  playerCoinsParent;
            winnerText = playerMoneyText;
            p1 = player1.totalCoin;
            player1.totalCoin += totalMoneyAccumulated;
            indicator = playerIndicator;
        }
        else
        {
            target = opponentCoinsParent;
            winnerText = opponentMoneyText;
            p1 = player2.totalCoin;
            player2.totalCoin += totalMoneyAccumulated;
            indicator = opponentIndicator;
        }

        playerMoneyText.gameObject.SetActive(true);
        opponentMoneyText.gameObject.SetActive(true);
        playerNameText.gameObject.SetActive(false);
        opponentNameText.gameObject.SetActive(false);

        int count = totalMoneyAccumulated/30; //Mathf.Clamp(totalMoneyAccumulated/10, 3, 10);

        float flowDelay = 0.5f/count;

        indicator.transform.DOShakeRotation(count * flowDelay, new Vector3(0,0,10f),50,50).SetDelay(1f);
        indicator.transform.DOShakeScale(count * flowDelay, new Vector3(0.3f, 0.3f, 0f),50,50).SetDelay(1f).OnComplete(delegate(){
            p1 += totalMoneyAccumulated % 30;
            accumulatedMoneyText.text = "0 " + I2.Loc.ScriptLocalization.Get("COIN");
            winnerText.text = p1.ToString();
        });
        for(int i = 0; i < count; i++)
        {
            accumulatedMoneyText.transform.parent.GetChild(0).GetChild(i).DOMove(target.transform.position, 1f).SetEase(Ease.InCubic).SetDelay(flowDelay * i)
            .OnComplete(delegate() { 
                p1 += 30;
                winnerText.text = p1.ToString();
            }).OnStart(delegate() {
                tot -= 30;
                accumulatedMoneyText.text = tot + " " + I2.Loc.ScriptLocalization.Get("COIN");
            });
            //accumulatedMoneyText.transform.parent.GetChild(0).GetChild(i).DOScale(Vector3.one * 0.6f, 0.5f).SetEase(Ease.Linear).SetDelay(flowDelay*i);
            //accumulatedMoneyText.transform.parent.GetChild(0).GetChild(i).DOScale(Vector3.one * 0.4f, 0.5f).SetEase(Ease.Linear).SetDelay(flowDelay*i + 0.5f);
        }
    }

    void ApplyCutToLoser(int winnerId)
    {
        if (startingPlayerId == 0 && winnerId == 1)
        {
            player1.totalCoin -= playerBid;
            playerMoneyText.text = player1.totalCoin.ToString();
            playerMoneyText.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = (-playerBid).ToString();
            playerMoneyText.transform.GetChild(3).gameObject.SetActive(true);
            playerMoneyText.transform.DOShakeRotation(1.5f, new Vector3(0,0,10f),10,10).OnComplete(delegate(){
                playerMoneyText.transform.GetChild(3).gameObject.SetActive(false);
            });
        }
        else if (startingPlayerId == 1 && winnerId == 0)
        {
            player2.totalCoin -= opponentBid;
            opponentMoneyText.text = player2.totalCoin.ToString();
            opponentMoneyText.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = (-opponentBid).ToString();
            opponentMoneyText.transform.GetChild(3).gameObject.SetActive(true);     
            opponentMoneyText.transform.DOShakeRotation(1.5f, new Vector3(0,0,10f),10,10).OnComplete(delegate(){
                opponentMoneyText.transform.GetChild(3).gameObject.SetActive(false);
            });   
        }
    }

	public void MakeBid(int index)
	{
        if (playerBid == 0) // didn't make a bid before
        {
            PlaySound(3);

            playerBid = bidAmounts[index];
            playerBidTime = Time.timeSinceLevelLoad - bidStartTime;
            totalBidPlayer += playerBid;

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

            tutorialBidHand.SetActive(false);
            currentQuestionData.chosenBidIndexCounts[index]++;
        }
	}

	public void MakeBidBot(int index)
	{        
		opponentBid = bidAmounts[index];
		opponentBidTime = Time.timeSinceLevelLoad - bidStartTime;
        totalBidOpponent += opponentBid;

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
        PlayMusic(2);
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
            infoText.text = "<b><color=#FFEA00>" + player1.username + "</color></b>\n" + I2.Loc.ScriptLocalization.Get("answering");
            knowQuestionButton.GetComponent<Button>().enabled = true;
            fiftyFiftyButton.GetComponent<Button>().enabled = true;
        }
        else
        {
            infoText.text = "<b><color=#FFEA00>" + player2.username + "</color></b>\n"+ I2.Loc.ScriptLocalization.Get("answering");
        }

        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].transform.parent.GetComponent<Button>().interactable = true;
        }
        questionCountdown.GetComponent<Animator>().enabled = true;

        if (playerScore + opponentScore == 2)
        {
            // tutorialHandQuestion.transform.position = new Vector3(1.5f, optionTexts[currentQuestion.rightAnswerIndex].transform.parent.position.y, 0);
            // tutorialPanel.transform.GetChild(2).position = new Vector3(0, optionTexts[currentQuestion.rightAnswerIndex].transform.parent.position.y, 0); //new Vector3(2.53f, optionTexts[currentQuestion.rightAnswerIndex].transform.parent.position.y, 0);

            // tutorialPanel.SetActive(true);
            // tutorialPanel.transform.GetChild(1).GetComponent<SpriteRenderer>().DOColor(new Color(0,0,0,0.45f),0.5f);
            // tutorialHandQuestion.SetActive(true);
            // tutorialHandQuestion.transform.DOScale(Vector3.one * 0.65f, 0.5f).SetLoops(-1,LoopType.Yoyo);

            DisplayTutorial();
        }
    }

    void ChangeActivePlayer()
    {
        if (playingPlayerId == 0)
        {
            playingPlayerId = 1;
            playerIndicator.GetComponent<Image>().sprite = nameBGSprites[0];
            opponentIndicator.GetComponent<Image>().sprite = nameBGSprites[1];
            
            infoText.transform.DOShakeRotation(1f,new Vector3(0,0,10f),20,50);
            infoText.text = "<b><color=#FFEA00>" + player2.username + "</color></b> " + I2.Loc.ScriptLocalization.Get("answering");
            botManager.Choose();
        }
        else
        {
            playingPlayerId = 0;
            playerIndicator.GetComponent<Image>().sprite = nameBGSprites[1];
            opponentIndicator.GetComponent<Image>().sprite = nameBGSprites[0];

            infoText.transform.DOShakeRotation(1f,new Vector3(0,0,10f),20,50);
            infoText.text = "<b><color=#FFEA00>" + player1.username + "</color></b> " + I2.Loc.ScriptLocalization.Get("answering");

            knowQuestionButton.enabled = true;
            fiftyFiftyButton.enabled = true;
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
        knowQuestionButton.enabled = false;
        fiftyFiftyButton.enabled = false;

        if (playingPlayerId == 0 && gameState == 2)
        {
            fiftyFiftyButton.interactable = false;
            if (currentQuestion.rightAnswerIndex == index) // Answered right!
            {
                PlaySound(8);
                Vector3 pos = optionTexts[index].transform.position;
                animStar.transform.position = new Vector3(pos.x, pos.y, 0);

                IncreaseScore();
                GoToTheNextTurn();
                optionTexts[index].transform.parent.GetComponent<Image>().sprite = choiceSprites[2];
                player1.rightAnswersInDifficulties[LocalizationManager.CurrentLanguageCode][currentQuestion.difficulty]++;

                if (!botShowEmojiObject.activeSelf)
                {
                    botManager.SendEmojiLose();
                }

                currentQuestionData.answeredRightCount++;
            }
            else // Answered Wrong!
            {
                PlaySound(9);
                optionTexts[index].transform.parent.GetComponent<Button>().enabled = false;
                optionTexts[index].transform.parent.GetComponent<Image>().sprite = choiceSprites[1];
                player1.wrongAnswersInDifficulties[LocalizationManager.CurrentLanguageCode][currentQuestion.difficulty]++;

                if (GetAvaliableChoiceCount() > 1)
                {
                  ChangeActivePlayer();                
                }
                else
                {
                    StartCoroutine(CleanScreen(true));
                }
                
                if (!botShowEmojiObject.activeSelf)
                {
                    botManager.SendEmojiWin();
                }

                currentQuestionData.answeredWrongCount++;
            }

            // if (PlayerPrefs.GetInt("IsTutorialCompeted") == 0)
            // {
            //     tutorialPanel.transform.GetChild(1).GetComponent<SpriteRenderer>().DOColor(new Color(0,0,0,0f),0.5f).OnComplete(delegate()
            //     {
            //         tutorialPanel.SetActive(false);
            //     });
            //     tutorialHandQuestion.SetActive(false);
            //     PlayerPrefs.SetInt("IsTutorialCompeted", 1); // tutorial completed
            // }
            currentQuestionData.chosenChoiceCounts[index]++;
            if (GetAvaliableChoiceCount() >= 3 || fiftyFiftyUsed)
            {
                currentQuestionData.firstAnsweringTimes.Add(questionTimer);
            }
            else
            {
                currentQuestionData.secondAnsweringTimes.Add(questionTimer);
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
               StartCoroutine(CleanScreen(true));
            }

            if (!botShowEmojiObject.activeSelf)
            {
                botManager.SendEmojiLose();
            }
        }
    }

	public void KnowTheQuestion()
	{
        if (!knowQuestionUsed && player1.knowQuestionSkillCount > 0 &&  gameState == 2 && playingPlayerId == 0)
        {
            PlayOtherSound(3);

    		ChooseAnswer(currentQuestion.rightAnswerIndex);
            knowQuestionUsed = true;
            knowQuestionButton.interactable = false;
            player1.knowQuestionSkillCount--;
            knowQuestionAmountText.text = player1.knowQuestionSkillCount.ToString();

            infoText.transform.Find("joker").gameObject.SetActive(true);
            infoText.text = "";

            currentQuestionData.knowQuestionUsedTime++;

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
            PlayOtherSound(4);

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

            currentQuestionData.disableTwoUsedTime++;

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
            infoText.text = "<b><color=#FFEA00>" + player2.username + "</color></b> " + I2.Loc.ScriptLocalization.Get("answering");
        }
        else
        {
            infoText.text = "<b><color=#FFEA00>" + player1.username + "</color></b> " + I2.Loc.ScriptLocalization.Get("answering");
        }
    }

    void IncreaseScore()
    {
        Transform destination;

        int winnerId = 0;
        if (playingPlayerId == 0)
        {
            playerScore++;
            destination = playerScoreStarsParent.transform.GetChild(playerScore-1);
            winnerId = 0;
        }
        else
        {
            opponentScore++;
            destination = opponentScoreStarsParent.transform.GetChild(opponentScore-1);
            winnerId = 1;
        }

        // Star animation
        PlaySound(4);

        animStar.transform.localScale = Vector3.zero;
        animStar.SetActive(true);
        animStar.transform.DOJump(destination.position, 4, 1, 2f).SetEase(Ease.InCubic).OnComplete(delegate(){
            destination.GetComponent<Image>().sprite = fullStarSprite; 
            animStar.SetActive(false);
            SendCoinsToWinner(winnerId);
            ApplyCutToLoser(winnerId);
            destination.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
            PlayOtherSound(2);
            });;
        // animStar.transform.DORotate(new Vector3(0,0,1080), 2f, RotateMode.WorldAxisAdd).SetEase(Ease.InCubic).OnComplete(delegate(){
        //     destination.GetComponent<Image>().sprite = fullStarSprite; 
        //     animStar.SetActive(false);
        //     SendCoinsToWinner(winnerId);
        //     ApplyCutToLoser(winnerId);
        //     });
        animStar.transform.DOScale(new Vector3(0.6f,0.6f,0), 1f).SetEase(Ease.OutSine).OnComplete(delegate(){
            animStar.transform.DOScale(new Vector3(0.159f,0.159f,0), 1f).SetEase(Ease.InQuart);
        });
        
    }

    void GoToTheNextTurn()
    {
        SendQuestionData();
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
        yield return new WaitForSeconds(5.6f);

        foreach (Transform i in playerCoinsParent.transform)
        {
            i.gameObject.SetActive(false);
        }

        foreach (Transform i in opponentCoinsParent.transform)
        {
            i.gameObject.SetActive(false);
        }

        foreach (Transform i in accumulatedMoneyText.transform.parent.GetChild(0))
        {
            i.gameObject.SetActive(false);
        }
        accumulatedMoneyText.transform.parent.GetChild(4).gameObject.SetActive(false);

        totalMoneyAccumulated = 0;
        accumulatedMoneyText.text = 0 + " " + I2.Loc.ScriptLocalization.Get("COIN");
        playerMoneyText.text = player1.totalCoin.ToString();
        opponentMoneyText.text = player2.totalCoin.ToString();

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

        if(playerScore > opponentScore)
        {
            StartEndGameCoinFlow();

            endScreen.transform.Find("Win").gameObject.SetActive(true);
            player1.totalCoin += totalBidPlayer;
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
            player2.totalCoin += totalBidOpponent;
            opponentMoneyText.text = player2.totalCoin.ToString();

            player1.score += 1;
            playerScoreStarsParent.SetActive(false);
            PlaySound(5);

            if (!botShowEmojiObject.activeSelf)
            {
                botManager.SendEmojiWin();
            }
        }

        GetComponent<LevelManager>().CheckLevelUp();
        endScreen.transform.Find("CoinText").GetComponent<TextMeshProUGUI>().text = (player1.totalCoin - startingCoin) + " " + I2.Loc.ScriptLocalization.Get("COIN");
        retryButton.GetComponent<Image>().sprite = retryButtonSprites[0];
        retryButton.transform.GetChild(0).GetComponent<Text>().text = I2.Loc.ScriptLocalization.Get("play again");
        endGameInfoText.text = "";
        retryButton.GetComponent<Button>().interactable = true;
        botManager.WantAgain();
        
        PlayMusic(0);

        SendUserData();
    }

    void StartEndGameCoinFlow()
    {
        StartCoroutine(PlayDelayedOtherSound(0, 2.1f));

        foreach (Transform i in chestCoinsParent.transform)
        {
            i.transform.localPosition = Vector3.zero;
            i.gameObject.SetActive(true);
        }

        if(playerScore > opponentScore)
        {
            int p1 = player1.totalCoin;
            int count = Mathf.Clamp(Mathf.FloorToInt((totalBidPlayer)/50), 1, chestCoinsParent.transform.childCount);
            float flowDelay = 2f/count;
            chestCoinsParent.transform.parent.DOShakeRotation(1 + 0.1f * count, new Vector3(0,0,10f),50,50).SetDelay(2f);
            playerIndicator.transform.DOShakeRotation(count * flowDelay, new Vector3(0,0,10f),50,50).SetDelay(3f);
            playerIndicator.transform.DOShakeScale(count * flowDelay, new Vector3(0.3f, 0.3f, 0f),50,50).SetDelay(3f).OnComplete(delegate(){
                if(PlayerPrefs.GetInt("IsTutorialCompeted") == 0)
                {
                    DisplayTutorial();
                }
            });
            
            for (int i = 0; i <  count; i++)
            {
                int no = i;
                chestCoinsParent.transform.GetChild(i).DOMove(playerCoinsParent.transform.position, 1f).SetEase(Ease.InCubic).SetDelay(2f + flowDelay*i)
                .OnComplete(delegate(){
                    p1 += 50;
                    if (p1 == player1.totalCoin - (totalBidPlayer) % 50)
                    {
                        p1 += (totalBidPlayer) % 50;
                    }
                    playerMoneyText.text = p1.ToString();
                    chestCoinsParent.transform.GetChild(no).gameObject.SetActive(false);
                });
                chestCoinsParent.transform.GetChild(i).DOScale(Vector3.one * 0.6f, 0.5f).SetEase(Ease.Linear).SetDelay(2f + flowDelay*i);
                chestCoinsParent.transform.GetChild(i).DOScale(Vector3.one * 0.2148f, 0.5f).SetEase(Ease.Linear).SetDelay(2f + flowDelay*i + 0.5f);
            }
        }
    }

    public void DoubleTheCoin()
    {
        doublecoinButton.SetActive(false);
        StartEndGameCoinFlow();
        player1.totalCoin += totalBidPlayer;
        endScreen.transform.Find("CoinText").GetComponent<TextMeshProUGUI>().text = (player1.totalCoin - startingCoin).ToString() + " " + I2.Loc.ScriptLocalization.Get("COIN");
        
        SendUserData();
    } 

    public void SendUserData()
    {
        if (ConnectionManager.isOnline)
        {
            PlayerPrefs.SetString("userData", JsonUtility.ToJson(player1));
            if(reference == null)
            {
                reference = FirebaseDatabase.DefaultInstance.RootReference;
            }
            reference.Child("userList").Child(player1.userId).SetRawJsonValueAsync(JsonUtility.ToJson(player1));
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
        #if ! UNITY_EDITOR
            FirebaseDatabase.DefaultInstance.GetReference("questionDataList/"+LocalizationManager.CurrentLanguageCode+"/"+currentQuestion.questionId).GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted) {
                    // Handle the error...
                    Debug.LogError("Error in FirebaseStart");
                }
                else if (task.IsCompleted) {
                    QuestionData realTimeQuestionData = JsonUtility.FromJson<QuestionData>(task.Result.GetRawJsonValue());

                    realTimeQuestionData.answeredRightCount += currentQuestionData.answeredRightCount;
                    realTimeQuestionData.answeredWrongCount += currentQuestionData.answeredWrongCount;
                    realTimeQuestionData.knowQuestionUsedTime += currentQuestionData.knowQuestionUsedTime;
                    realTimeQuestionData.disableTwoUsedTime += currentQuestionData.disableTwoUsedTime;
                    
                    for (int i = 0; i <  realTimeQuestionData.chosenChoiceCounts.Length; i++)
                    {
                        realTimeQuestionData.chosenChoiceCounts[i] += currentQuestionData.chosenChoiceCounts[i];
                    }
                    
                    for (int i = 0; i <  realTimeQuestionData.chosenBidIndexCounts.Length; i++)
                    {
                        realTimeQuestionData.chosenBidIndexCounts[i] += currentQuestionData.chosenBidIndexCounts[i];
                    }

                    for (int i = 0; i <  currentQuestionData.bidGivingTimes.Count; i++)
                    {
                        realTimeQuestionData.bidGivingTimes.Add(currentQuestionData.bidGivingTimes[i]);
                    }

                    for (int i = 0; i <  currentQuestionData.firstAnsweringTimes.Count; i++)
                    {
                        realTimeQuestionData.firstAnsweringTimes.Add(currentQuestionData.firstAnsweringTimes[i]);
                    }

                    for (int i = 0; i <  currentQuestionData.secondAnsweringTimes.Count; i++)
                    {
                        realTimeQuestionData.secondAnsweringTimes.Add(currentQuestionData.secondAnsweringTimes[i]);
                    }

                    if(reference == null)
                    {
                        reference = FirebaseDatabase.DefaultInstance.RootReference;
                    }
                    reference.Child("questionDataList").Child(LocalizationManager.CurrentLanguageCode).Child(currentQuestion.questionId.ToString()).SetRawJsonValueAsync(JsonUtility.ToJson(realTimeQuestionData));
                }
            });
        #endif
    }

    IEnumerator CleanScreen(bool keepMoney = false)
    {
        gameState = 3; // to stop the timer
        infoText.text = "";
        if (keepMoney)
        {
            yield return new WaitForSeconds(1f);
        }
        else
        {
            yield return new WaitForSeconds(5.5f);
            totalMoneyAccumulated = 0;
            accumulatedMoneyText.text = 0 + " " + I2.Loc.ScriptLocalization.Get("COIN");
        }
        
        playerMoneyText.text = player1.totalCoin.ToString();
        opponentMoneyText.text = player2.totalCoin.ToString();

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

        foreach (Transform i in accumulatedMoneyText.transform.parent.GetChild(0))
        {
            i.gameObject.SetActive(false);
        }

        questionText.gameObject.SetActive(false);
        questionLockObject.SetActive(true);
        bidIndicatorsParent.SetActive(false);

        playerMoneyText.gameObject.SetActive(false);
        opponentMoneyText.gameObject.SetActive(false);
        playerNameText.gameObject.SetActive(true);
        opponentNameText.gameObject.SetActive(true);

        accumulatedMoneyText.transform.parent.GetChild(4).gameObject.SetActive(false);

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
            else if (endGameInfoText.text != I2.Loc.ScriptLocalization.Get("run away"))
            {
                retryButton.GetComponent<Image>().sprite = retryButtonSprites[1];
                endGameInfoText.text = I2.Loc.ScriptLocalization.Get("wants again");
                retryButton.transform.GetChild(0).GetComponent<Text>().text = I2.Loc.ScriptLocalization.Get("waiting");
                retryButton.GetComponent<Button>().interactable = false;
            }
            else
            {
                if (GameObject.Find("DataToCarry"))
                {
                    GameObject.Find("DataToCarry").GetComponent<DataToCarry>().mainMenuAnimLayerIndex = 4;
                }
                StartCoroutine(DelayedGoToMenu());
            }
        }
        else
        {
            endGameInfoText.text = I2.Loc.ScriptLocalization.Get("Not enough coins");
        }
    }

    public void GoToMenuAndSearchPlayer()
    {
        Time.timeScale = 1;
        if (GameObject.Find("DataToCarry"))
        {
            GameObject.Find("DataToCarry").GetComponent<DataToCarry>().mainMenuAnimLayerIndex = 4;
        }
        GoToMenu();
    }

    public void BotWantsAgain()
    {
        if (player1.totalCoin >= 30*5)
        {
            retryButton.GetComponent<Image>().sprite = retryButtonSprites[0];
            endGameInfoText.text = I2.Loc.ScriptLocalization.Get("wants again");
            retryButton.transform.GetChild(0).GetComponent<Text>().text = I2.Loc.ScriptLocalization.Get("play again");
             retryButton.GetComponent<Button>().interactable = true;
        }
    }

    public void BotRunAway()
    {
        if (player1.totalCoin >= 30*5)
        {
            retryButton.GetComponent<Image>().sprite = retryButtonSprites[0];
            endGameInfoText.text = I2.Loc.ScriptLocalization.Get("run away");
            retryButton.transform.GetChild(0).GetComponent<Text>().text = I2.Loc.ScriptLocalization.Get("new game");
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
        endGameInfoText.text = I2.Loc.ScriptLocalization.Get("request accepted");
        retryButton.transform.GetChild(0).GetComponent<Text>().text = I2.Loc.ScriptLocalization.Get("starting");
        yield return new WaitForSeconds(1.5f);

        Restart();
    }

    public void Restart()
    {
        SceneManager.LoadScene("Game");
    } 

    public void GoToMenu()
    {
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

    IEnumerator PlayDelayedOtherSound(int id, float time)
    {
        yield return new WaitForSeconds(time);
        PlayOtherSound(id);
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

    void PlayOtherSound(int id)
    {
        if (otherSoundManager)
        {
            otherSoundManager.GetComponent<SoundManager>().PlaySound(id);
        }
        else if (GameObject.FindGameObjectWithTag("otherSound"))
        {
            otherSoundManager = GameObject.FindGameObjectWithTag("otherSound").GetComponent<SoundManager>();
            otherSoundManager.GetComponent<SoundManager>().PlaySound(id);
        }
    }

    void StopMusic()
    {
        if(musicManager)
        {
            musicManager.GetComponent<MusicManager>().StopMusic();
        }
        else if (GameObject.FindGameObjectWithTag("music"))
        {
            musicManager = GameObject.FindGameObjectWithTag("music").GetComponent<MusicManager>();
            musicManager.GetComponent<MusicManager>().StopMusic();
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

    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            Time.timeScale = 0f;
            disconnectedScreen.SetActive(true);
        }
    }
}                            