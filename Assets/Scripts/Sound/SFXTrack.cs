using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXTrack : MonoBehaviour {
    public AudioSource mySource;
    public AudioClip mySound;
    public float pitch;
    public float volume;
    public bool isSpatialized;
    public bool isFree;
    public bool isLooping;
    public GameObject linkedGameObject;
    public Vector3 playPosition;
    public string soundName;

    private void Start()
    {
        isFree = false;
    }

    public void PlaySound()
    {
        isFree = false;
        if (mySource == null)
        {
            mySource = GetComponent<AudioSource>();
        }
        mySource.pitch = pitch;
        mySource.volume = volume;

        if (isSpatialized)
        {
            mySource.spatialize = true;
            mySource.spatialBlend = 1;
        }
        else
        {
            mySource.spatialize = false;
            mySource.spatialBlend = 0;
        }

        mySource.loop = isLooping;
        mySource.clip = mySound;
        mySource.Play();
    }

    public void ChangeVolume(float newVolume)
    {
        mySource = GetComponent<AudioSource>();
        mySource.volume = newVolume;
    }

    private void Update()
    {
        if (mySource != null)
        {
            if (isSpatialized)
            {
                if (playPosition != Vector3.zero)
                {
                    transform.position = playPosition;
                } else if (linkedGameObject != null) 
                {
                    transform.position = linkedGameObject.transform.position;
                }
            }
            if (mySource.isPlaying == false && isLooping == false)
            {
                isFree = true;
            }
        }
    }
}
