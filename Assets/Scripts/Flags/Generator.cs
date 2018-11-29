using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Flag {
    
    public int power = 2;

    public override void Enable()
    {
        base.Enable();
        missionManager.StartMission(myBlockLink.gridCoordinates, "EmitEnergy", -1, power);
        Debug.Log("Emitting energy");
    }

    public override void Disable()
    {
        base.Disable();
        missionManager.StartMission(myBlockLink.gridCoordinates, "RemoveEnergy", -1, power);
    }

    public override void BeforeMovingBlock()
    {
        base.BeforeMovingBlock();
        Disable();
    }

    public override void AfterMovingBlock()
    {
        base.AfterMovingBlock();
        Enable();
    }

    public override void OnBlockDestroy()
    {
        base.OnBlockDestroy();
        Disable();
    }
}
