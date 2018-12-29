using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class User {
	public int userId;
    public string username;
    public float score;
	public List<int> seenQuestionIds;
    public int totalCoin;
    public int avatarId;
    public bool isMale; // true male, false female
    public float[] rightAnswersInDifficulties; // right answer count in every question difficulty
    public float[] wrongAnswersInDifficulties; // wrng answer count in every question difficulty
    public int wonGameCount;
    public int playedGameCount;

    public User(int userId, string username, bool isMale, int totalCoin = 0) {
        this.userId = userId;
		this.username = username;
        this.score = 0;
		this.seenQuestionIds = new List<int>{0,1,4,3,5,8,6}; //new List<int>(); TODO reverse the change
        this.totalCoin = totalCoin;
        this.avatarId = 0;
        this.isMale = isMale;
        this.rightAnswersInDifficulties = new float[] {0,0,0,0}; // TODO may change according to the difficulty levels
        this.wrongAnswersInDifficulties = new float[] {0,0,0,0}; // TODO may change according to the difficulty levels
        this.wonGameCount = 0;
        this.playedGameCount = 0;
    }
}