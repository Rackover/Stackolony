using System.Collections;
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
        GameManager.instance.systemManager.AllOccupators.Add(this);
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemManager.AllOccupators.Remove(this);
        base.OnDestroy();
    }

    override public void UpdateNuisanceImpact()
    {
        slots += nuisanceImpact;
        nuisanceImpact = Mathf.FloorToInt(slots * (block.nuisance * block.scheme.sensibility * 10)/100);
        slots -= nuisanceImpact;
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
}
