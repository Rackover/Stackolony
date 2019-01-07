using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Occupator : Flag 
{

    public int slots;
    public List<PopulationManager.Citizen> affectedCitizen = new List<PopulationManager.Citizen>();
    public int range;
    public Population[] acceptedPopulation;


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

    public override void AfterMovingBlock()
    {
        base.AfterMovingBlock();
     //   Invoke("OnBlockUpdate", 0f);
    }

    public override void OnBlockUpdate()
    {
        base.OnBlockUpdate();
        GameManager.instance.missionManager.StartMission(myBlockLink.gridCoordinates, "EmitOccupators", range,0, typeof(House));
    }
}
