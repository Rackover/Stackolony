using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : MonoBehaviour 
{
	public VoiceManager voicePlayer;
	public MusicManager musicPlayer;

	[System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip[] clips;
    }

	void Start()
	{
		musicPlayer.Play(GameManager.instance.library.mainMusic);
	}

	void Update()
    {
		ChangeVolume(GameManager.instance.player.options.GetFloat(Options.Option.sfxVolume));
		musicPlayer.ChangeVolume(GameManager.instance.player.options.GetFloat(Options.Option.musicVolume));
		voicePlayer.ChangeVolume(GameManager.instance.player.options.GetFloat(Options.Option.voiceVolume));
    }

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

    public void Play(string soundName, float volume = 1f)
    {
        source.clip = FindClipByName(soundName);
        source.PlayOneShot(FindClipByName(soundName), volume);
    }

    //Renvoit un clip en fonction de son nom
    public AudioClip FindClipByName(string name)
    {
        foreach (Sound clip in GameManager.instance.library.soundBank.sounds)
        {
            if (clip.name == name)
            {
                if (clip.clips.Length == 0)
                {
                    Debug.LogWarning("ERROR : No clip found");
                }
                int randomClipIndex = Random.Range(0, clip.clips.Length);
                return clip.clips[randomClipIndex];
            }
        }
        return null;
    }
}
