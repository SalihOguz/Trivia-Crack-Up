using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocalFakeUsersDatabase  {

	public StringFakeUserListDictionary fakeusersInLanguages;

	public LocalFakeUsersDatabase()
	{
		this.fakeusersInLanguages = new StringFakeUserListDictionary() {{"en", new FakeUserList()}, {"tr", new FakeUserList()}}; // TODO languages hardcoded here
	}
}

[System.Serializable]
public class StringFakeUserListDictionary : SerializableDictionary<string, FakeUserList> {}