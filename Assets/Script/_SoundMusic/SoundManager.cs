using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
	public AudioClip[] soundClips;
	AudioSource audioSource;

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		if (PlayerPrefs.GetInt("isSoundOn") == 1)
		{
			audioSource.volume = 1;
		}
		else
		{
			audioSource.volume = 0;
		}
	}

	public void PlaySound(int id)
	{
		audioSource.Stop();
		//audioSource.clip = soundClips[id]; // TODO uncomment when sound come
		audioSource.Play();
	}

	public void TurnSound(int state) // 1 = on, 0 = off
	{
		PlayerPrefs.SetInt("isSoundOn", state);
		if (state == 0)
		{
			audioSource.volume = 0;
		}
		else
		{
			audioSource.volume = 1;
		}
	}
}