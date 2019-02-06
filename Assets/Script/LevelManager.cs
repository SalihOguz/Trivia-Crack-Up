using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LevelManager : MonoBehaviour {
	GameManager gameManager;
	public int[] levelUpgradeScores;
	public GameObject levelUpScreen;
	Avatars av;

	void Start()
	{
		gameManager = Camera.main.GetComponent<GameManager>();
		av = Resources.Load<Avatars>("Data/Avatars");
	}

	public void CheckLevelUp()
	{
		if (gameManager.player1.score > av.levelUpgradeScores[gameManager.player1.level + 1]) // level up
		{
			levelUpScreen.SetActive(true);
			gameManager.endScreen.SetActive(false);

			levelUpScreen.transform.Find("AvatarBodyImage").GetComponent<Image>().sprite = av.avatarBodySprites[Mathf.Clamp(gameManager.player1.avatarId + 1, 1, av.avatarBodySprites.Length - 1)];
			levelUpScreen.transform.Find("light Image").DORotate(new Vector3(0,0,360), 4f, RotateMode.WorldAxisAdd).SetLoops(-1).SetEase(Ease.Linear);

			gameManager.player1.level++;
			gameManager.player1.avatarId++;
			gameManager.UpdatePlayerAvatar();
		}
	}
}
