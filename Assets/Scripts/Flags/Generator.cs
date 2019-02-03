using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Flag, Flag.IFlag
{
    
    public int power;

    public override void Awake()
    {
        base.Awake();

        //Generate ou récupère l'objet "SystemReferences";
        //missionManager.StartMission(myBlockLink.gridCoordinates, "EmitEnergy", -1, power);

        GameManager.instance.systemManager.AllGenerators.Add(this);
    }

    public override void Enable()
    {
        base.Enable();
        StartCoroutine(GameManager.instance.systemManager.RecalculatePropagation());
    }

    public override void Disable()
    {
        base.Disable();
        StartCoroutine(GameManager.instance.systemManager.RecalculatePropagation());
    }

    public void GenerateEnergy()
    {
        if(isEnabled)
        {
            GameManager.instance.missionManager.StartMission(block.gridCoordinates, "EmitEnergy", -1, power);
        }
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemManager.AllGenerators.Remove(this);
        base.OnDestroy();
    }

    public System.Type GetFlagType()
    {
        return GetType();
    }
}
