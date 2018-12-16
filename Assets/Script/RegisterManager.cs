using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RegisterManager : MonoBehaviour {
	public GameObject uiLayer;

	public void GoToScene()
    {
        StartCoroutine(GoToMenu());
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

}
