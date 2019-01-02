using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class User {
	public string userId;
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
    // public int knowQuestionCount; // item counts should be added

    public User(string userId, string username, bool isMale, int totalCoin = 2000) {
        this.userId = userId;
		this.username = username;
        this.score = 0;
		this.seenQuestionIds = new List<int>();
        this.totalCoin = totalCoin;
        this.isMale = isMale;
        this.rightAnswersInDifficulties = new float[] {0,0,0,0}; // TODO may change according to the difficulty levels
        this.wrongAnswersInDifficulties = new float[] {0,0,0,0}; // TODO may change according to the difficulty levels
        this.wonGameCount = 0;
        this.playedGameCount = 0;
        if (isMale)
        {
            this.avatarId = 0;
        }
        else
        {
            this.avatarId = 1;
        }
    }
}