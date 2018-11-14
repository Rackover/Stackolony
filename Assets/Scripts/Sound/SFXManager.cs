using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour {
    /// <summary>
    /// FONCTIONNEMENT DU SCRIPT : 
    /// Le script va récupérer une pool d'objets "tracks" et leur assigner un son s'ils ne sont pas en train d'en jouer. Si jamais aucune track n'est disponible, une nouvelle sera generée.
    /// </summary>
    [System.Serializable]
    public class Sound
    {
        public string soundName;
        public AudioClip[] soundClip;
    }
    public Sound[] soundList;

    [System.NonSerialized]
    public List<SFXTrack> trackList = new List<SFXTrack>();
    private GameObject trackParent;

    private void Start()
    {
        //Genere un gameObject enfant qui contiendra toutes les tracks
        trackParent = new GameObject();
        trackParent.name = "TrackList";
        trackParent.transform.position = Vector3.zero;
        trackParent.transform.parent = this.transform;

        PlaySound("Ambiance", 1, 1, true);
    }

    //Recupere une track libre et lui ajoute un son
    public void PlaySound(string soundName, float volume = 1, float pitch = 1, bool looping = false)
    {
        SFXTrack myTrack = trackList[FindFreeTrack()];
        myTrack.mySound = FindClipByName(soundName);
        myTrack.pitch = pitch;
        myTrack.volume = volume;
        myTrack.isLooping = looping;
        myTrack.isFree = false;
        myTrack.isSpatialized = false;
        myTrack.linkedGameObject = null;
        myTrack.playPosition = Vector3.zero;
        myTrack.PlaySound();
    }

    //Recupere une track libre et lui ajoute un son avec des parametres choisi aléatoirement
    public void PlaySoundWithRandomParameters(string soundName, float volumeMin = 1, float volumeMax = 1, float pitchMin = 1, float pitchMax = 1, bool looping = false)
    {
        SFXTrack myTrack = trackList[FindFreeTrack()];
        myTrack.mySound = FindClipByName(soundName);
        myTrack.pitch = Random.Range(pitchMin,pitchMax);
        myTrack.volume = Random.Range(volumeMin, volumeMax);
        myTrack.isLooping = looping;
        myTrack.isFree = false;
        myTrack.isSpatialized = false;
        myTrack.linkedGameObject = null;
        myTrack.playPosition = Vector3.zero;
        myTrack.PlaySound();
    }

    //Recupere une track libre, lui ajoute un son spatialisé et la lie à un objet donné
    public void PlaySoundLinked(string soundName, GameObject linkedObject, float volume = 1, float pitch = 1, bool looping = false)
    {
        SFXTrack myTrack = trackList[FindFreeTrack()];
        myTrack.mySound = FindClipByName(soundName);
        myTrack.pitch = pitch;
        myTrack.volume = volume;
        myTrack.isLooping = looping;
        myTrack.isFree = false;
        myTrack.isSpatialized = true;
        myTrack.linkedGameObject = linkedObject;
        myTrack.playPosition = Vector3.zero;
        myTrack.PlaySound();
    }

    //Recupere une track libre, lui ajoute un son spatialisé et la place à une position donnée
    public void PlaySoundAtPosition(string soundName, Vector3 position, float volume = 1, float pitch = 1, bool looping = false)
    {
        SFXTrack myTrack = trackList[FindFreeTrack()];
        myTrack.mySound = FindClipByName(soundName);
        myTrack.pitch = pitch;
        myTrack.volume = volume;
        myTrack.isLooping = looping;
        myTrack.isFree = false;
        myTrack.isSpatialized = true;
        myTrack.linkedGameObject = null;
        myTrack.playPosition = position;
        myTrack.PlaySound();
    }

    //Renvoit un clip en fonction de son nom
    public AudioClip FindClipByName(string name)
    {
        foreach (Sound clip in soundList)
        {
            if (clip.soundName == name)
            {
                if (clip.soundClip.Length == 0)
                {
                    Debug.LogWarning("ERROR : No clip found");
                }
                int randomClipIndex = Random.Range(0, clip.soundClip.Length - 1);
                return clip.soundClip[randomClipIndex];
            }
        }
        return null;
    }

    //Renvoit l'index d'une track qui ne joue pas de musique
    public int FindFreeTrack()
    {
        int trackIndex = 0;
        for (int i = 0; i < trackList.Count; i++)
        {
            if (trackList[i].isFree)
            {
                trackIndex = i;
                return trackIndex;
            }
        }
        GenerateTrack();
        return trackList.Count-1;
    }

    //Genere une nouvelle track (si aucune n'est disponible)
    public void GenerateTrack()
    {
        GameObject newTrack = new GameObject();
        newTrack.transform.parent = trackParent.transform;
        AudioSource trackSource = newTrack.AddComponent<AudioSource>();
        trackSource.maxDistance = 100;
        trackSource.rolloffMode = AudioRolloffMode.Custom;
        SFXTrack trackComp = newTrack.AddComponent<SFXTrack>();
        trackList.Add(trackComp);
        newTrack.name = "Track " + trackList.Count;
    }
}
