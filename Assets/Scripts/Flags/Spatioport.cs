using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spatioport : Flag {

    public override void Awake()
    {
        base.Awake();

        GameManager.instance.systemManager.AllSpatioports.Add(this);
    }

    public override void AfterMovingBlock()
    {
        base.AfterMovingBlock();
        Invoke("OnBlockUpdate", 0f);
    }

    public override void OnBlockUpdate()
    {
        base.OnBlockUpdate();
        if (isEnabled)
        {
            GameManager.instance.missionManager.StartMission(myBlockLink.gridCoordinates, "EmitSpatioportInfluence", -1, -1);
        }
    }
}
