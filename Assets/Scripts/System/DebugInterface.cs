using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugInterface : MonoBehaviour {

    public GameObject eventDebugWindow;

	public void SpawnEventDebugWindow()
    {
        foreach(EventDebugWindow d in FindObjectsOfType<EventDebugWindow>()) {
            Destroy(d.gameObject);
        }

        Instantiate(eventDebugWindow, transform);
    }
}
