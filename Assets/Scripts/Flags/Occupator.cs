using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Occupator : Flag 
{
    [System.Serializable]
    public class OccupatorSlot
    {
        public PopulationManager.Citizen affectedCitizen;
    }

    public int range;
    public OccupatorSlot[] occupatorSlots;
    public Population[] acceptedPopulation;


    public override void Awake()
    {
        base.Awake();
        GameManager.instance.systemManager.AllOccupators.Add(this);
    }

    public override void Enable()
    {
        base.Enable();
        GameManager.instance.systemManager.UpdateSystem();
    }

    public override void Disable()
    {
        base.Disable();
        GameManager.instance.systemManager.UpdateSystem();
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemManager.AllOccupators.Remove(this);
        base.OnDestroy();
    }

    public override void AfterMovingBlock()
    {
        base.AfterMovingBlock();
        Invoke("OnBlockUpdate", 0f);
    }

    public override void OnBlockUpdate()
    {
        base.OnBlockUpdate();
        GameManager.instance.missionManager.StartMission(myBlockLink.gridCoordinates, "EmitOccupators", range,0, typeof(House));
        Debug.Log("UPDATING BLOCK OCCUPATOR");
    }
}
