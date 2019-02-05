using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spatioport : Flag, Flag.IFlag {

    public override void Awake()
    {
        base.Awake();
    }

    public override void AfterMovingBlock()
    {
        base.AfterMovingBlock();
        Invoke("OnBlockUpdate", 0f);
    }

    public override void Enable()
    {
        base.Enable();
        GameManager.instance.systemManager.AllSpatioports.Add(this);
    }

    public override void Disable()
    {
        base.Disable();
        GameManager.instance.systemManager.AllSpatioports.Remove(this);
        GameManager.instance.systemManager.StartCoroutine(GameManager.instance.systemManager.RecalculateSpatioportInfluence());
    }

    public override void OnBlockUpdate()
    {
        base.OnBlockUpdate();
        if (isEnabled)
        {
            GameManager.instance.missionManager.StartMission(block.gridCoordinates, "EmitSpatioportInfluence", -1, -1);
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
        return "Spatioport";
    }
}
