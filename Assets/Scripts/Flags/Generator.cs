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

        GameManager.instance.systemManager.AllGenerators.Add(this);
    }
    public override void Enable()
    {
        base.Enable();
        GameManager.instance.systemManager.UpdateElectricitySystem();
    }

    public override void Disable()
    {
        base.Disable();
        GameManager.instance.systemManager.UpdateElectricitySystem();
    }

    public override void BeforeMovingBlock()
    {
        base.BeforeMovingBlock();
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
            GameManager.instance.missionManager.StartMission(myBlockLink.gridCoordinates, "EmitEnergy", -1, power);
        }
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemManager.AllGenerators.Remove(this);
        base.OnDestroy();
    }
}
