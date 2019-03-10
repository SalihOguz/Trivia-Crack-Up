using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocalQuestionDatabase {
	public StringQuestionListDictionary questionsInLanguages;

	public LocalQuestionDatabase()
	{
		this.questionsInLanguages = new StringQuestionListDictionary() {{"en", new QuestionList()}, {"tr", new QuestionList()}}; // TODO languages hardcoded here
	}
}

[System.Serializable]
public class StringQuestionListDictionary : SerializableDictionary<string, QuestionList> {}
