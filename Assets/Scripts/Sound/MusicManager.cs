using UnityEngine;

public class MusicManager : MonoBehaviour 
{
	public AudioClip music;
	AudioSource source;
	float volume;

	void Awake()
	{
		source = gameObject.GetComponent<AudioSource>();
		if(source == null) source = gameObject.AddComponent<AudioSource>();
	}

	public void ChangeVolume(float v)
	{
		volume = v;
	}

	[ContextMenu("MUSIC")]
	public void Music()
	{
		Play(music);
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