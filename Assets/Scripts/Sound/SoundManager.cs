using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : MonoBehaviour 
{
	public SFXManager sfxPlayer;
	public VoiceManager voicePlayer;
	public MusicManager musicPlayer;

	void Start()
	{
		musicPlayer.Play(GameManager.instance.library.mainMusic);
		sfxPlayer.PlaySound("AmbianceSound", GameManager.instance.player.options.GetFloat("bgsVolume"), 1, true);
	}

	void Update()
    {
        foreach(SFXTrack t in sfxPlayer.trackList) 
		{
            t.ChangeVolume(GameManager.instance.player.options.GetFloat("bgsVolume"));
        }
		musicPlayer.ChangeVolume(GameManager.instance.player.options.GetFloat("musicVolume"));
		voicePlayer.ChangeVolume(GameManager.instance.player.options.GetFloat("voiceVolume"));
    }
}
