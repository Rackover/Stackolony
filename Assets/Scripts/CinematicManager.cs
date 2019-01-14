using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicManager : MonoBehaviour {

    private GameObject interfaceObj;
    private CursorManagement cursor;
    private GameObject cursorObj;
    private GameObject storageBayObj;

    public void GetReferences()
    {
        interfaceObj = FindObjectOfType<Interface>().gameObject;
        cursorObj = GameManager.instance.cursorDisplay.gameObject;
        cursor = FindObjectOfType<CursorManagement>();
        storageBayObj = GameManager.instance.storageBay.gameObject;
    }

    public void SetCinematicMode(bool state)
    {
        interfaceObj.SetActive(!state);
        cursorObj.SetActive(!state);
        cursor.enabled = !state;
        storageBayObj.SetActive(!state);
        cursor.myProjector.GetComponent<Projector>().enabled = !state;
    }
}
