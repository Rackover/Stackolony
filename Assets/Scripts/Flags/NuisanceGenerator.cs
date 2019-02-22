using System.Collections;
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
        Disable();
        base.OnDestroy();
    }

    public override void Enable()
    {
        base.Enable();
        if(!GameManager.instance.systemManager.AllNuisanceGenerators.Contains(this))
            GameManager.instance.systemManager.AllNuisanceGenerators.Add(this);
    }

    public override void Disable()
    {
        base.Disable();
        GameManager.instance.systemManager.AllNuisanceGenerators.Remove(this);
        GameManager.instance.systemManager.StartCoroutine(GameManager.instance.systemManager.RecalculateNuisance());
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

    public string GetFlagDatas()
    {
        return "NuisanceGenerator_" + range + "_" + amount;
    }
}
