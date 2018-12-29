using UnityEngine;

[System.Serializable]
public class Question{
	public int questionId;
	public string questionText;
	public string choice1;
	public string choice2;
	public string choice3;
	public string choice4;
	public int rightAnswerIndex; // starting from 0
	public int difficulty; // 0 = easiest
	public string category;
}