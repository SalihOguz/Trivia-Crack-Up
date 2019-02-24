using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataToCarry : Singleton<DataToCarry> {
	public User player2;
	public QuestionList ql;
	public FakeUserList ful;
	public int mainMenuAnimLayerIndex = -1;

	void Start()
	{
		ql = new QuestionList();
		ql.questionList = new List<Question>();
	}

	public void ChooseOpponent()
	{
		if (PlayerPrefs.GetInt("IsTutorialCompeted") == 0)
		{
			player2 = CallTutor();
		}
		else
		{
			player2 = MakeFakeUser();
		}

		if (Camera.main.GetComponent<MenuManager>())
		{
			Camera.main.GetComponent<MenuManager>().opponentDataObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite = Resources.Load<Avatars>("Data/Avatars").avatarSprites[player2.avatarId];
			Camera.main.GetComponent<MenuManager>().opponentDataObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Text>().text = player2.username;
		}
		else
		{
			print("whaaat");
		}
	}

	User MakeFakeUser()
    {
        UserLite fake = ful.fakeUserList[UnityEngine.Random.Range(0, ful.fakeUserList.Count)];
        User p2 = new User("1", fake.userName, fake.isMale, fake.totalCoin, fake.score);
        return p2;
    }

	User CallTutor()
	{
		User p2 = new User("1", I2.Loc.ScriptLocalization.Get("tutor"), true, 100000, 300);
        return p2;
	}
}
