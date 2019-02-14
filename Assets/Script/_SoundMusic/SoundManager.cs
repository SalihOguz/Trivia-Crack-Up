using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
	public AudioClip[] soundClips;
	AudioSource audioSource;

	void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		if (PlayerPrefs.GetInt("isSoundOn") == 1)
		{
			audioSource.volume = 0.5f;
		}
		else
		{
			audioSource.volume = 0;
		}
	}

	public void PlaySound(int id)
	{
		audioSource.Stop();
		audioSource.clip = soundClips[id];
		audioSource.Play();
	}

	public void TurnSound(int state) // 1 = on, 0 = off
	{
		PlayerPrefs.SetInt("isSoundOn", state);
		if (state == 1)
		{
			audioSource.volume = 0.5f;
		}
		else
		{
			audioSource.volume = state;
		}
	}
}