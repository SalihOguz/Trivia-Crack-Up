using UnityEngine;

[System.Serializable]
public class Question{
	public int questionId;
	public string questionText;
	public string option1;
	public string option2;
	public string option3;
	public string option4;
	public int rightAnswerIndex; // starting from 1
	public int difficulty; // 1 = easiest
	public string category;
}