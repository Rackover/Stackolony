using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireRiskGenerator : Flag, Flag.IFlag {

    public int amountInPercent;

    public override void Enable()
    {
        base.Enable();
        GameManager.instance.systemManager.AllFireRiskGenerators.Add(this);
    }

    public override void Disable()
    {
        base.Disable();
        GameManager.instance.systemManager.AllFireRiskGenerators.Remove(this);
        GameManager.instance.systemManager.StartCoroutine(GameManager.instance.systemManager.RecalculateFireRisks());
    }

    public void GenerateFireRisks()
    {
        if (isEnabled)
        {
            GameManager.instance.missionManager.StartMission(block.gridCoordinates, "GenerateFireRisks", 1, -1);
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
        return "FireRiskGenerator_" + amountInPercent;
    }
}
