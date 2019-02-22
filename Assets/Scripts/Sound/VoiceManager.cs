using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class VoiceManager : MonoBehaviour
{
	AudioSource source;
    float fade = 1f;

    public float fadeOutSpeed = 30f;

    void Awake()
	{
		// REFERENCE TO THE AUDIOSOURCE
		source = gameObject.GetComponent<AudioSource>();
		if(source == null) source = gameObject.AddComponent<AudioSource>();
		source.loop = true;
	}


    public void ChangeVolume(float v)
	{
		source.volume = Mathf.Min(v, fade);
	}

	public void Play(Population pop, float duration = 5f, float pitchVariation = 0.1f)
	{
        fade = 1f;
        source.clip = pop.voice;
		source.time = Random.Range(0f, source.clip.length);
		source.pitch = Random.Range(1- pitchVariation, 1+ pitchVariation);
		source.Play();
		StartCoroutine(Stop(duration));
	}

    public void ContinuePlaying(Population pop, float duration = 5f, float pitchVariation = 0.1f)
    {
        if (source.isPlaying) {
            return;
        }
        else {
            Play(pop, duration, pitchVariation);
        }
    }

    public void Stop()
    {
        StartCoroutine(Stop(0f));
    }

	public void Pause()
	{
		source.Pause();
	}

	IEnumerator Stop(float time)
	{
		yield return new WaitForSeconds(time);
        while (source.volume > 0) {
            fade -= fadeOutSpeed * Time.deltaTime;
            yield return null;
        }
		Pause();
	}
}
