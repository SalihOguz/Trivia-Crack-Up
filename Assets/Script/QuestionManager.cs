using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using I2.Loc;

public class QuestionManager : MonoBehaviour {
	QuestionList ql;
	User player1;
	QuestionList[] difficultyList; // questions from every dificulty
	public int difficultyLevelCount = 4; // how many question difficulties are there. If this is changed from 4, User.cs needs to change too
	List<int> sortedSeen;
	int userDifficultyLevel; // level of user, one below, one above
	public float userLevelTreshold = 0.85f; // percentage of questions that has to be answered right to go to the next level
	public int userLevelTresholdQuestionCount = 25; // number of question to answer before going to the next level
	public float chanceOfAskingLowerLevel = 0.2f;
	public float chanceOfAskingHigherLevel = 0.2f;
	int questionCount = 0;

	void Start()
	{
		if (LocalizationManager.CurrentLanguageCode == "en") // TODO hardcoded in language
		{
			difficultyLevelCount = 3;
			userLevelTresholdQuestionCount = 100;
			userLevelTreshold = 0.85f;
		}

		player1 = GetComponent<GameManager>().player1;
		GetQuestionsFromJson();
		FindUserLevel();
		FillQuestionLists();
	}

	void GetQuestionsFromJson()
	{
		if (GameObject.Find("DataToCarry"))
		{
			QuestionList temp = new QuestionList();
			temp.questionList = new List<Question>(GameObject.Find("DataToCarry").GetComponent<DataToCarry>().ql.questionList);
			ql = temp;
		}
		else
		{
			// TextAsset textAssset = Resources.Load<TextAsset>("Data/Questions/");
			// ql = JsonUtility.FromJson<QuestionList>(textAssset.text);
			Debug.Log("You need to come from the register scene for getting online questions.");	
		}
		questionCount = ql.questionList.Count;

	}

	void FindUserLevel()
	{
		for (int i = difficultyLevelCount - 1; i > -1; i--)
		{
			if (player1.rightAnswersInDifficulties[LocalizationManager.CurrentLanguageCode][i] / (player1.wrongAnswersInDifficulties[LocalizationManager.CurrentLanguageCode][i] + 0.001) > userLevelTreshold &&
			player1.rightAnswersInDifficulties[LocalizationManager.CurrentLanguageCode][i] + player1.wrongAnswersInDifficulties[LocalizationManager.CurrentLanguageCode][i] > userLevelTresholdQuestionCount)
			{
				userDifficultyLevel = Mathf.Clamp(i + 1, 0, difficultyLevelCount - 1);
				break;
			}
			if (i == 0)
			{
				userDifficultyLevel = 0;
			}
		}
	}

	void FillQuestionLists()
	{
		RemoveSeenQuestions();

		// initalize the lists to fill
		difficultyList = new QuestionList[difficultyLevelCount];
		for (int i = 0; i < difficultyLevelCount; i++)
		{
			difficultyList[i] = new QuestionList();
		}

		ChooseQuestionsFromDifficulty();
	}

	void RemoveSeenQuestions()
	{
		if (ql.questionList.Count < questionCount)
		{
			GetQuestionsFromJson();
		}
		// TODO we may need to test question list integrity. It may be uncomplete. If there is a index missing in row numbers, it will cause problems
		sortedSeen = new List<int>(player1.seenQuestionIds[LocalizationManager.CurrentLanguageCode]);
		sortedSeen.Sort();
		sortedSeen.Reverse();
		foreach (int i in sortedSeen)
		{
			ql.questionList.RemoveAt(i);
		}
		CheckQuestionCount();
	}

	void CheckQuestionCount() // checs if there are enough unseen questions
	{
		// check if there are 5 questions available from every difficulty needed
		int[] count = new int[difficultyLevelCount];

		foreach (Question i in ql.questionList)
		{
			if (i.difficulty == userDifficultyLevel || i.difficulty == userDifficultyLevel - 1 || i.difficulty == userDifficultyLevel + 1)
			{
				count[i.difficulty] ++;
			}
			if(count[userDifficultyLevel] > 5 && 
				count[Mathf.Clamp(userDifficultyLevel - 1, 0, difficultyLevelCount-1)] > 5 &&
					count[Mathf.Clamp(userDifficultyLevel + 1, 0, difficultyLevelCount-1)] > 5)
			{
				return;
			}
		}

		// if there are not enough questions, delete question ids from seenQuestionIds until there are enough
		GetQuestionsFromJson();
		List<int> temp = new List<int>(player1.seenQuestionIds[LocalizationManager.CurrentLanguageCode]);
		foreach (int i in temp)
		{
			int diff = ql.questionList[i].difficulty;
			if (diff  == userDifficultyLevel || diff == userDifficultyLevel - 1 || diff == userDifficultyLevel + 1)
			{
				count[diff]++;
				player1.seenQuestionIds[LocalizationManager.CurrentLanguageCode].Remove(i);
			}

			if(count[userDifficultyLevel] > 5 && 
				count[Mathf.Clamp(userDifficultyLevel - 1, 0, difficultyLevelCount-1)] > 5 &&
					count[Mathf.Clamp(userDifficultyLevel + 1, 0, difficultyLevelCount-1)] > 5)
			{
				break;
			}
		}

		RemoveSeenQuestions();
	}

	void ChooseQuestionsFromDifficulty()
	{
		Shuffle(ql.questionList); // TODO then maybe sort according to difficulty

		int iterCount = 0;
		foreach (Question i in ql.questionList)
		{
			iterCount++;
			if (i.difficulty == userDifficultyLevel || i.difficulty == userDifficultyLevel - 1 || i.difficulty == userDifficultyLevel + 1)
			{
				difficultyList[i.difficulty].questionList.Add(i);
			}
			else
			{
				continue;
			}

			// check if we are done
			if (difficultyList[userDifficultyLevel].questionList.Count > 5 && 
				difficultyList[Mathf.Clamp(userDifficultyLevel - 1, 0, difficultyLevelCount-1)].questionList.Count > 5 &&
				difficultyList[Mathf.Clamp(userDifficultyLevel + 1, 0, difficultyLevelCount-1)].questionList.Count > 5)
			{
				print("finished after looking at " + iterCount + " questions");
				break;
			}
		}
	}

	public Question ChooseQuestion()
	{
		int rnd = UnityEngine.Random.Range(0, 100);
		if (rnd < chanceOfAskingLowerLevel * 100)
		{
			if (difficultyList[Mathf.Clamp(userDifficultyLevel - 1, 0, difficultyLevelCount-1)].questionList.Count == 0)
			{
				FillQuestionLists();
			}
			int index = UnityEngine.Random.Range(0, difficultyList[Mathf.Clamp(userDifficultyLevel - 1, 0, difficultyLevelCount-1)].questionList.Count);
			Question toReturn = difficultyList[Mathf.Clamp(userDifficultyLevel - 1, 0, difficultyLevelCount-1)].questionList[index];
			difficultyList[Mathf.Clamp(userDifficultyLevel - 1, 0, difficultyLevelCount-1)].questionList.RemoveAt(index);
			return toReturn;
		}
		else if (rnd < (chanceOfAskingLowerLevel + chanceOfAskingHigherLevel) * 100)
		{
			if (difficultyList[Mathf.Clamp(userDifficultyLevel + 1, 0, difficultyLevelCount-1)].questionList.Count == 0)
			{
				FillQuestionLists();
			}
			int index = UnityEngine.Random.Range(0, difficultyList[Mathf.Clamp(userDifficultyLevel + 1, 0, difficultyLevelCount-1)].questionList.Count);
			Question toReturn = difficultyList[Mathf.Clamp(userDifficultyLevel + 1, 0, difficultyLevelCount-1)].questionList[index];
			difficultyList[Mathf.Clamp(userDifficultyLevel + 1, 0, difficultyLevelCount-1)].questionList.RemoveAt(index);
			return toReturn;	
		}
		else
		{
			if (difficultyList[userDifficultyLevel].questionList.Count == 0)
			{
				FillQuestionLists();
			}
			int index = UnityEngine.Random.Range(0, difficultyList[userDifficultyLevel].questionList.Count);
			Question toReturn = difficultyList[userDifficultyLevel].questionList[index];
			difficultyList[userDifficultyLevel].questionList.RemoveAt(index);
			return toReturn;
		}
	}

	void Shuffle(List<Question> list)
	{
		for(var i=0; i < list.Count; i++)
		{
			int j = UnityEngine.Random.Range(i, list.Count);
			Question temp = list[i];
			list[i] = list[j];
			list[j] = temp;
		}
	}
}