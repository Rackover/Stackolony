using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class VoiceManager : MonoBehaviour
{
	AudioSource source;

	Dictionary<string, AudioClip> populationVoice = new Dictionary<string, AudioClip>();

	void Awake()
	{
		// REFERENCE TO THE AUDIOSOURCE
		source = gameObject.GetComponent<AudioSource>();
		if(source == null) source = gameObject.AddComponent<AudioSource>();
		source.loop = true;

		// FILLING DICTIONARY
		populationVoice.Add("Eari", GameManager.instance.library.voices[0]);
		populationVoice.Add("Kavga", GameManager.instance.library.voices[1]);
		populationVoice.Add("Senuth", GameManager.instance.library.voices[2]);
		populationVoice.Add("Covridian", GameManager.instance.library.voices[3]);
		populationVoice.Add("Krowser", GameManager.instance.library.voices[4]);
	}

	public void ChangeVolume(float v)
	{
		source.volume = v;
	}

	public void Play(string popName, float duration = 5f)
	{
		source.clip = populationVoice[popName];
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
