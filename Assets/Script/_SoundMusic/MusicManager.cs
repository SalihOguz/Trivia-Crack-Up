using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {
	public AudioClip[] musicClips;
	AudioSource audioSource;

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		if (PlayerPrefs.GetInt("isMusicOn") == 1)
		{
			audioSource.volume = 1;
			PlayMusic(0);
		}
		else
		{
			audioSource.volume = 0;
		}
	}

	public void PlayMusic(int id)
	{
		audioSource.Stop();
		audioSource.clip = musicClips[id];
		audioSource.Play();
	}

	public void TurnSound(int state) // 1 = on, 0 = off
	{
		PlayerPrefs.SetInt("isMusicOn", state);
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
