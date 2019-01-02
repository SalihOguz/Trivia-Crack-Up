using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RegisterManager : MonoBehaviour {
	public GameObject uiLayer;
	public InputField nameField;
	public Text infoText;
	string userName = "";
	bool isMale = false;

	void Awake()
	{
		//PlayerPrefs.DeleteAll();
		if (PlayerPrefs.HasKey("userData"))
		{
			SceneManager.LoadScene("MainMenu");
		}
	}

	public void ChangeLayerTo(int layerCount)
	{
		uiLayer.GetComponent<Animator>().SetInteger("layerCount", layerCount);
	}

	IEnumerator GoToMenu()
	{
		yield return new WaitForSeconds(1.2f);
		SceneManager.LoadScene("MainMenu");
	}

	public void GetName()
	{
		infoText.gameObject.SetActive(false);
		if (nameField.text.Length < 3)
		{
			infoText.text = "Kullanıcı adınız 3 karakterden kısa olamaz";
			infoText.gameObject.SetActive(true);
			return;
		}
		// TODO check if username is taken online
		userName = nameField.text;
		ChangeLayerTo(1);
		print(userName);
	}

	public void GetGender(bool isMale)
	{
		this.isMale = isMale;
		if (isMale)
		{
			uiLayer.transform.GetChild(1).GetChild(0).GetChild(0).gameObject.SetActive(false);
			uiLayer.transform.GetChild(1).GetChild(1).GetChild(0).gameObject.SetActive(true);
		}
		else
		{
			uiLayer.transform.GetChild(1).GetChild(0).GetChild(0).gameObject.SetActive(true);
			uiLayer.transform.GetChild(1).GetChild(1).GetChild(0).gameObject.SetActive(false);			
		}
	}

	public void CreateUser()
	{
		User newUser = new User("0", userName, isMale);
		PlayerPrefs.SetString("userData", JsonUtility.ToJson(newUser));
		ChangeLayerTo(2);
		StartCoroutine(GoToMenu());
	}
}
