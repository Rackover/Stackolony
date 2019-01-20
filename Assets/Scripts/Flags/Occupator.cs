using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Occupator : Flag 
{
    public int slots;
    public int range;
    public Population[] acceptedPopulation;
    public List<PopulationManager.Citizen> affectedCitizen = new List<PopulationManager.Citizen>();

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

    public void GenerateOccupations()
    {
        if (isEnabled)
        {
            GameManager.instance.missionManager.StartMission(block.gridCoordinates, "EmitOccupators", range, 0, typeof(House));
        }
    }
}
