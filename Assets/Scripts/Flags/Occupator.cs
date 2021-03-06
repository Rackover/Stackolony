﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Occupator : Flag, Flag.IFlag
{
    public int slots;
    public int range;
    public Population[] acceptedPopulation;
    public List<PopulationManager.Citizen> affectedCitizen = new List<PopulationManager.Citizen>();
    public int nuisanceImpact; //Number of slots lost because of nuisance

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemManager.AllOccupators.Remove(this);
        base.OnDestroy();
    }

    override public void UpdateNuisanceImpact()
    {
        slots += nuisanceImpact;
        nuisanceImpact = Mathf.FloorToInt(slots * (block.nuisance * block.scheme.sensibility * 10) / 100);
        slots -= nuisanceImpact;
    }

    public override void Enable()
    {
        base.Enable();
        if(!GameManager.instance.systemManager.AllOccupators.Contains(this))
        {
            GameManager.instance.systemManager.AllOccupators.Add(this);
        }
    }

    public override void Disable()
    {
        base.Disable();
        GameManager.instance.systemManager.AllOccupators.Remove(this);
        StartCoroutine(GameManager.instance.systemManager.RecalculateOccupators());
    }

    public void GenerateOccupations()
    {
        if (isEnabled)
        {
            GameManager.instance.missionManager.StartMission(block.gridCoordinates, "EmitOccupators", range, 0, typeof(House));
        }
    }

    public System.Type GetFlagType()
    {
        return GetType();
    }


    public string GetFlagDatas()
    {
        string profiles = "";
        for (int i = 0; i < acceptedPopulation.Length; i++)
        {
            if (i != 0)
                profiles += "-";
            profiles += acceptedPopulation[i].codeName;
        }
        return "Occupator_" + slots + "_" + range + "_" + profiles;
    }
}
