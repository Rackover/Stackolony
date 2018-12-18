using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Flag {
    
    public int power;

    public override void Awake()
    {
        base.Awake();

        //Generate ou récupère l'objet "SystemReferences";
        //missionManager.StartMission(myBlockLink.gridCoordinates, "EmitEnergy", -1, power);

        GameManager.instance.systemReferences.AllGenerators.Add(this);
    }
    public override void Enable()
    {
        base.Enable();
        GameManager.instance.systemReferences.UpdateSystem();
    }

    public override void Disable()
    {
        base.Disable();
        GameManager.instance.systemReferences.UpdateSystem();
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
            GameManager.instance.missionManager.StartMission(myBlockLink.gridCoordinates, "EmitEnergy", -1, power);
        }
    }

    public override void OnBlockUpdate()
    {
        if (isEnabled)
        Invoke("AfterMovingBlock", 0f);
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemReferences.AllGenerators.Remove(this);
        base.OnDestroy();
    }
}
