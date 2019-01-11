using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInterfaces : MonoBehaviour {

    TemporalityInterface temporalityInterface;
    List<Coroutine> routines = new List<Coroutine>();

    private void Awake()
    {
        temporalityInterface = GetComponentInChildren<TemporalityInterface>();
    }

    public void StartGameInterfaces()
    {
        routines.Add(StartCoroutine(temporalityInterface.UpdateInterface()));
    }

    public void StopGameInterfaces()
    {
        foreach(Coroutine routine in routines) {
            StopCoroutine(routine);
        }
    }
}
