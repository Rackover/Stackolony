using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicManager : MonoBehaviour {

    private GameObject interfaceObj;
    private CursorManagement cursor;
    private GameObject cursorObj;
    private GameObject storageBayObj;

    public bool areCinematicsEnabled = false;

    public void GetReferences()
    {
        interfaceObj = FindObjectOfType<Interface>().gameObject;
        cursorObj = GameManager.instance.cursorDisplay.gameObject;
        cursor = FindObjectOfType<CursorManagement>();
    }

    public void SetCinematicMode(bool state)
    {
        interfaceObj.GetComponent<Canvas>().enabled = (!state);
        cursorObj.SetActive(!state);
        cursor.enabled = !state;
        cursor.myProjector.GetComponent<Projector>().enabled = !state;
    }

    public void SwitchToCamera(Camera cam)
    {
        Camera.main.targetDisplay = 1;
        cam.targetDisplay = 0;
        GetComponent<CameraShake>().SetCamera(cam);
    }

    public void SwitchToMainCamera()
    {
        foreach(Camera cam in FindObjectsOfType<Camera>()) {
            cam.targetDisplay = 1;
        }
        GetComponent<CameraShake>().SetCamera(Camera.main);
        Camera.main.targetDisplay = 0;
    }

}
