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

    public User(int userId, string username, int totalCoin = 0) {
        this.userId = userId;
		this.username = username;
        this.score = 0;
		this.seenQuestionIds = new List<int>();
        this.totalCoin = totalCoin;
        this.avatarId = 0;
    }
}