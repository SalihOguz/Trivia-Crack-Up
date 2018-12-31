using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FakeUserList {
	public List<UserLite> fakeUserList = new List<UserLite>();
}

[System.Serializable]
public class UserLite
{
    public string userName;
    public bool isMale;
    public int totalCoin;
}