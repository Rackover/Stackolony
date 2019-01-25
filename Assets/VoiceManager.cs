using UnityEngine;

public class VoiceManager : MonoBehaviour
{
	AudioSource source;
	public AudioClip voice;
	float volume;

	void Awake()
	{
		source = gameObject.GetComponent<AudioSource>();
		if(source == null) source = gameObject.AddComponent<AudioSource>();
		source.loop = true;
	}

	public void ChangeVolume(float v)
	{
		volume = v;
	}
	[ContextMenu("VOICE")]
	public void Play()
	{
		source.clip = voice;
		source.time = Random.Range(0f, source.clip.length);
		source.pitch = Random.Range(1f, 1.5f);

		source.Play();
	}

	public void Pause()
	{
		source.Pause();
	}
}
