using UnityEngine;

public class MusicManager : MonoBehaviour 
{
	AudioSource source;

	void Awake()
	{
		source = gameObject.GetComponent<AudioSource>();
		if(source == null) source = gameObject.AddComponent<AudioSource>();
	}

	public void ChangeVolume(float v)
	{
		source.volume = v;
	}

	public void Play(AudioClip clip = null)
	{
		if(clip != null)
		{
			source.clip = clip;
		}

		source.Play();
	}

	public void Pause()
	{
		source.Pause();
	}
}