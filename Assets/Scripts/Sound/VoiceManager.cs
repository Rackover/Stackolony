using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class VoiceManager : MonoBehaviour
{
	AudioSource source;

	Dictionary<string, int> populationVoice = new Dictionary<string, int>()
	{
		{"Eari", 0},
		{"Kavga", 1},
		{"Senuth", 2},
		{"Covridian", 3},
		{"Krowser", 4}
	};

	void Awake()
	{
		source = gameObject.GetComponent<AudioSource>();
		if(source == null) source = gameObject.AddComponent<AudioSource>();
		source.loop = true;
	}

	public void ChangeVolume(float v)
	{
		source.volume = v;
	}

	public void Play(string popName, float duration = 5f)
	{
		source.clip = GameManager.instance.library.voices[populationVoice[popName]];
		source.time = Random.Range(0f, source.clip.length);
		source.pitch = Random.Range(0.9f, 1.1f);
		source.Play();
		StartCoroutine(Stop(duration));
	}

	public void Pause()
	{
		source.Pause();
	}

	IEnumerator Stop(float time)
	{
		yield return new WaitForSeconds(time);
		Pause();
	}
}
