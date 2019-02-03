﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NuisanceGenerator : Flag, Flag.IFlag {

    public int range;
    public int amount;

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemManager.AllNuisanceGenerators.Remove(this);
        base.OnDestroy();
    }

    public override void Enable()
    {
        base.Enable();
        GameManager.instance.systemManager.AllNuisanceGenerators.Add(this);
    }

    public override void Disable()
    {
        base.Disable();
        GameManager.instance.systemManager.AllNuisanceGenerators.Remove(this);
        StartCoroutine(GameManager.instance.systemManager.RecalculateNuisance());
    }

    public void GenerateNuisance()
    {
        if (isEnabled)
        {
            GameManager.instance.missionManager.StartMission(block.gridCoordinates, "EmitNuisance", range);
        }
    }

    public System.Type GetFlagType()
    {
        return GetType();
    }
}
