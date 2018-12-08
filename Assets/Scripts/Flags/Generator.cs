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
    }

    public override void BeforeMovingBlock()
    {
        base.BeforeMovingBlock();
    }

    public override void AfterMovingBlock()
    {
        base.AfterMovingBlock();
        Enable();
    }

    public override void OnBlockUpdate()
    {
        Invoke("AfterMovingBlock", 0.1f);
    }

    public override void OnDestroy()
    {
        systemRef.AllGenerators.Remove(this);
        base.OnDestroy();
    }
}
