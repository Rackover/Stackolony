using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NuisanceGenerator : Flag, Flag.IFlag {

    public int range;
    public int amount;

    public override void Awake()
    {
        base.Awake();
        GameManager.instance.systemManager.AllNuisanceGenerators.Add(this);
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemManager.AllNuisanceGenerators.Remove(this);
        base.OnDestroy();
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
