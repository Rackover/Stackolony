using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInterfaces : MonoBehaviour {

    TemporalityInterface temporalityInterface;
    BulletinDisplay bulletinDisplay;
    MoodsDisplay moodsDisplay;
    List<Coroutine> routines = new List<Coroutine>();

    private void Awake()
    {
        temporalityInterface = GetComponentInChildren<TemporalityInterface>();
        bulletinDisplay = GetComponentInChildren<BulletinDisplay>();
        moodsDisplay = GetComponentInChildren<MoodsDisplay>();
    }

    public void StartGameInterfaces()
    {
        routines.Add(StartCoroutine(temporalityInterface.UpdateInterface()));
        routines.Add(StartCoroutine(moodsDisplay.UpdateInterface()));
        routines.Add(StartCoroutine(bulletinDisplay.RefreshBulletin()));
    }

    public void StopGameInterfaces()
    {
        foreach(Coroutine routine in routines) {
            StopCoroutine(routine);
        }
    }
}
