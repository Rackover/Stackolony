using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Flag, Flag.IFlag
{
    
    public int power;

    public override void Enable()
    {
        base.Enable();
        GameManager.instance.systemManager.AllGenerators.Add(this);
    }

    public override void Disable()
    {
        base.Disable();
        GameManager.instance.systemManager.AllGenerators.Remove(this);
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
        Disable();
        base.OnDestroy();
    }

    public System.Type GetFlagType()
    {
        return GetType();
    }

    public string GetFlagDatas()
    {
        return "Generator_" + power;
    }
}
