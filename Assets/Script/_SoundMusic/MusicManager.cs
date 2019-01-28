using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MusicManager : MonoBehaviour {
	public AudioClip[] musicClips;
	AudioSource audioSource;

	void Awake()
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
		if (PlayerPrefs.GetInt("isMusicOn") == 1)
		{
			if (audioSource.clip)
			{
				DOTween.To(()=> audioSource.volume, x => audioSource.volume = x, 0.2f,0.5f).OnComplete(delegate()
				{
					audioSource.Stop();
					audioSource.clip = musicClips[id];
					audioSource.Play();
					DOTween.To(()=> audioSource.volume, x => audioSource.volume = x, 1, 0.5f);
				});
			}
			else
			{
				audioSource.clip = musicClips[id];
				audioSource.Play();
			}
		}
	}

	public void TurnMusic(int state) // 1 = on, 0 = off
	{
		PlayerPrefs.SetInt("isMusicOn", state);
		audioSource.volume = state;
	}
}