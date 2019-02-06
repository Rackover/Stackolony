﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodProvider: Flag, Flag.IFlag
{
    public int range;
    public float foodTotal;
    public float foodLeft;
    private int nuisanceImpact;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Enable()
    {
        base.Enable();
        GameManager.instance.systemManager.AllFoodProviders.Add(this);
    }

    public override void Disable()
    {
        base.Disable();
        GameManager.instance.systemManager.AllFoodProviders.Remove(this);
        StartCoroutine(GameManager.instance.systemManager.RecalculateFoodConsumption());
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemManager.AllFoodProviders.Remove(this);
        base.OnDestroy();
    }

    public void GenerateFood()
    {
        if (isEnabled)
        {
            GameManager.instance.missionManager.StartMission(block.gridCoordinates, "DistributeFood", range, 0, typeof(House));
        }
    }

    override public void UpdateNuisanceImpact()
    {
        range += nuisanceImpact;
        nuisanceImpact = block.nuisance * block.scheme.sensibility;
        range -= nuisanceImpact;
    }

    public System.Type GetFlagType()
    {
        return GetType();
    }

    public string GetFlagDatas()
    {
        return "FoodProvider_" + range + "_" + foodTotal;
    }
}
