using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Flag {
    
    public int power;

    public override void Awake()
    {
        base.Awake();
        //Generate ou récupère l'objet "SystemReferences";
        systemRef.AllGenerators.Add(this);
        Enable();
    }
    public override void Enable()
    {
        base.Enable();
        missionManager.StartMission(myBlockLink.gridCoordinates, "EmitEnergy", -1, power);
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

    public override void OnBlockUpdate()
    {
        Invoke("BeforeMovingBlock", 0);
        Invoke("AfterMovingBlock", 0.1f);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        systemRef.AllGenerators.Remove(this);
        Disable();
    }
}
