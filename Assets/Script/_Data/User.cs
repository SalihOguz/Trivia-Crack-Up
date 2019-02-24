using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class User {
	public string userId;
    public string username;
    public float score;
    public int totalCoin;
    public int avatarId;
    public bool isMale; // true male, false female
    public StringIntListDictionary rightAnswersInDifficulties; // right answer count in every question difficulty
    public StringIntListDictionary wrongAnswersInDifficulties; // wrng answer count in every question difficulty
    public int wonGameCount;
    public int playedGameCount;
    public int knowQuestionSkillCount; // item that makes user know the question
    public int fiftyFiftySkillCount; // item that eliminates two choices
    public int level;
    public StringIntListDictionary seenQuestionIds;

    public User(string userId, string username, bool isMale, int totalCoin = 2000, int score = 0) {
        this.userId = userId;
		this.username = username;
        this.score = score;
        this.totalCoin = totalCoin;
        this.isMale = isMale;
        this.wonGameCount = 0;
        this.playedGameCount = 0;
        this.knowQuestionSkillCount = 3;
        this.fiftyFiftySkillCount = 3;
        this.level = GetLevel(score);
        this.avatarId = level;
        this.seenQuestionIds =  new StringIntListDictionary() {{"en", new List<int>()}, {"tr", new List<int>()}}; // TODO languages hardcoded here
        this.rightAnswersInDifficulties = new StringIntListDictionary() {{"en", new List<int>() {0,0,0}}, {"tr",  new List<int>() {0,0,0,0}}}; // TODO may change according to the difficulty levels
        this.wrongAnswersInDifficulties = new StringIntListDictionary() {{"en", new List<int>() {0,0,0}}, {"tr",  new List<int>() {0,0,0,0}}}; // TODO may change according to the difficulty levels
    }

    int GetLevel(int score)
    {
        Avatars av = Resources.Load<Avatars>("Data/Avatars");
        for (int i = 1; i < av.avatarSprites.Length; i++)
        {
            if (score < av.levelUpgradeScores[i])
            {
                return --i;
            }
        }
        return 0;
    }
}

[System.Serializable]
public class intListStorage : SerializableDictionary.Storage<List<int>> {}
[System.Serializable]
public class StringIntListDictionary : SerializableDictionary<string, List<int>, intListStorage> {}
