using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInterfaces : MonoBehaviour {

    TemporalityInterface temporalityInterface;
    BulletinDisplay bulletinDisplay;
    List<Coroutine> routines = new List<Coroutine>();

    private void Awake()
    {
        temporalityInterface = GetComponentInChildren<TemporalityInterface>();
        bulletinDisplay = GetComponentInChildren<BulletinDisplay>();
    }

    public void StartGameInterfaces()
    {
        routines.Add(StartCoroutine(temporalityInterface.UpdateInterface()));
        routines.Add(StartCoroutine(bulletinDisplay.RefreshBulletin()));
    }

    public void StopGameInterfaces()
    {
        foreach(Coroutine routine in routines) {
            StopCoroutine(routine);
        }
    }
}
