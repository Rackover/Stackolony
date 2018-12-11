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
        missionManager.StartMission(myBlockLink.gridCoordinates, "EmitEnergy", -1, power);
    }
    public override void Enable()
    {
        base.Enable();
        systemRef.UpdateSystem();
    }

    public override void Disable()
    {
        base.Disable();
        systemRef.UpdateSystem();
    }

    public override void BeforeMovingBlock()
    {
        base.BeforeMovingBlock();
    }

    public override void AfterMovingBlock()
    {
        base.AfterMovingBlock();
        if (isEnabled)
        {
            missionManager.StartMission(myBlockLink.gridCoordinates, "EmitEnergy", -1, power);
        }
    }

    public override void OnBlockUpdate()
    {
        if (isEnabled)
        Invoke("AfterMovingBlock", 0f);
    }

    public override void OnDestroy()
    {
        systemRef.AllGenerators.Remove(this);
        base.OnDestroy();
    }
}
