using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicManager : MonoBehaviour {

    private GameObject interfaceObj;
    private CursorManagement cursor;
    private GameObject cursorObj;

    public CameraShake shaker;
    public bool areCinematicsEnabled = false;
    bool isInCinematic = false;

    public void GetReferences()
    {
        interfaceObj = FindObjectOfType<Interface>().gameObject;
        cursorObj = GameManager.instance.cursorDisplay.gameObject;
        cursor = FindObjectOfType<CursorManagement>();
        shaker = GetComponent<CameraShake>();

        Camera.main.targetDisplay = 0;
        shaker.SetCamera(Camera.main);
    }

    public void SetCinematicMode(bool state)
    {
        interfaceObj.GetComponent<Canvas>().enabled = (!state);
        cursorObj.SetActive(!state);
        cursor.enabled = !state;
        cursor.myProjector.GetComponent<Projector>().enabled = !state;
        isInCinematic = state;
        if (state) GameManager.instance.Pause(); else GameManager.instance.UnPause();
    }
     
    public bool IsInCinematic()
    {
        return isInCinematic;
    }
}
