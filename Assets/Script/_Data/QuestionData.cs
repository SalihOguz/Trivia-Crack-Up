using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class QuestionData {
	public int questionId;
	public int answeredRightCount = 0;
	public int answeredWrongCount = 0;
	public int[] chosenChoiceCounts = new int[4];
	public int[] chosenBidIndexCounts = new int[3];
	public List<float> bidGivingTimes = new List<float>();
	public List<float> firstAnsweringTimes = new List<float>();
	public List<float> secondAnsweringTimes = new List<float>();
}