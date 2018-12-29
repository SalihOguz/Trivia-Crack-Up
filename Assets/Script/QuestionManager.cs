using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class QuestionManager : MonoBehaviour {
	QuestionList ql;
	User player1;
	QuestionList[] difficultyList; // questions from every dificulty
	public int difficultyLevelCount = 4; // how many question difficulties are there. If this is changed from 4, User.cs needs to change too
	List<int> sortedSeen;
	int userDifficultyLevel; // level of user, one below, one above
	public float userLevelTreshold = 0.8f; // percentage of questions that has to be answered right to go to the next level
	public float chanceOfAskingLowerLevel = 0.15f;
	public float chanceOfAskingHigherLevel = 0.15f;

	void Start()
	{
		TextAsset textAssset = Resources.Load<TextAsset>("Data/Questions");
		ql = JsonUtility.FromJson<QuestionList>(textAssset.text);
		player1 = GetComponent<GameManager>().player1;

		FindUserLevel();
		FillQuestionLists();
	}

	void FindUserLevel()
	{
		for (int i = difficultyLevelCount - 1; i > -1; i--)
		{
			if (player1.rightAnswersInDifficulties[i] > 0 && player1.wrongAnswersInDifficulties[i] > 0 && 
			player1.rightAnswersInDifficulties[i] / player1.wrongAnswersInDifficulties[i] > userLevelTreshold &&
			player1.rightAnswersInDifficulties[i] + player1.wrongAnswersInDifficulties[i] > 10)
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

	void FillQuestionLists() //  TODO I may call this again if we run out of questions
	{
		CheckQuestionCount();
		RemoveSeenQuestions();

		// initalize the lists to fill
		difficultyList = new QuestionList[difficultyLevelCount];
		for (int i = 0; i < difficultyLevelCount; i++)
		{
			difficultyList[i] = new QuestionList();
		}

		ChooseQuestionsFromDifficulty(userDifficultyLevel);
	}

	void CheckQuestionCount() // checs if there are enough unseen questions
	{
		if (ql.questionList.Count - player1.seenQuestionIds.Count < 10) // there are only ten questions left that is unseen by the player
		{
			player1.seenQuestionIds.RemoveRange(0, 50); // TODO I delete first 50 seen questions, there may be a better solution, I have to also delete according to user level
		}
	}

	void RemoveSeenQuestions()
	{
		// TODO we may need to test question list integrity. It may be uncomplete. If there is a index missing in row numbers, it will cause problems
		sortedSeen = new List<int>(player1.seenQuestionIds);
		sortedSeen.Sort();
		sortedSeen.Reverse();
		foreach (int i in sortedSeen)
		{
			ql.questionList.RemoveAt(i);
		}
	}

	void ChooseQuestionsFromDifficulty(int difficulty)
	{
		Shuffle(ql.questionList); // TODO then maybe sort according to difficulty

		int iterCount = 0;
		foreach (Question i in ql.questionList)
		{
			iterCount++;
			if (i.difficulty == difficulty || i.difficulty == difficulty - 1 || i.difficulty == difficulty + 1)
			{
				difficultyList[i.difficulty].questionList.Add(i);
			}
			else
			{
				continue;
			}

			// check if we are done
			if (difficultyList[i.difficulty].questionList.Count > 5 && 
				difficultyList[Mathf.Clamp(i.difficulty - 1, 0, difficultyLevelCount-1)].questionList.Count > 5 &&
				difficultyList[Mathf.Clamp(i.difficulty + 1, 0, difficultyLevelCount-1)].questionList.Count > 5)
			{
				print("finished after looking at " + iterCount + " questions");
				break;
			}
		}
	}

	public Question ChooseQuestion()
	{
		int rnd = UnityEngine.Random.Range(0, 100);
		if (rnd < chanceOfAskingLowerLevel * 100 && userDifficultyLevel > 0)
		{
			if (difficultyList[userDifficultyLevel - 1].questionList.Count == 0)
			{
				FillQuestionLists();
			}
			int index = UnityEngine.Random.Range(0, difficultyList[userDifficultyLevel - 1].questionList.Count);
			Question toReturn = difficultyList[userDifficultyLevel - 1].questionList[index];
			difficultyList[userDifficultyLevel - 1].questionList.RemoveAt(index);
			return toReturn;
		}
		else if (rnd < (chanceOfAskingLowerLevel + chanceOfAskingHigherLevel) * 100 && userDifficultyLevel < difficultyLevelCount - 1)
		{
			if (difficultyList[userDifficultyLevel + 1].questionList.Count == 0)
			{
				FillQuestionLists();
			}
			int index = UnityEngine.Random.Range(0, difficultyList[userDifficultyLevel + 1].questionList.Count);
			Question toReturn = difficultyList[userDifficultyLevel + 1].questionList[index];
			difficultyList[userDifficultyLevel + 1].questionList.RemoveAt(index);
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