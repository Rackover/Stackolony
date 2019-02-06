using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damaged : StateBehavior 
{
    [Header("Damaged")]
    public bool isBeingRepaired = false;
    public int microcycleCount = 0;
    public int lastFireId = 0;

    public override void Start()
    {
        disabler = true;
        base.Start();
        block.RemoveState(State.Unpowered);
        block.effects.Activate(GameManager.instance.library.damagedParticle);
        block.visuals.NewVisual(GameManager.instance.library.ruinPrefab);
        block.Disable();
    }

    public override void OnGridUpdate()
    {
        base.OnGridUpdate();
        CancelRepair();
    }

    public override void OnNewMicrocycle()
    {
        base.OnNewMicrocycle();
        if( microcycleCount++ > 1 ) Remove();
    }

    public void StartRepair()
    {
        if(!isBeingRepaired)
        {
            microcycleCount = 0;
            isBeingRepaired = true;
            if(block != null) { block.effects.Activate( GameManager.instance.library.repairParticle); }
        }
    }

    public void CancelRepair()
    {
        microcycleCount = 0;
        isBeingRepaired = false;
        if(block != null) { block.effects.Desactivate(GameManager.instance.library.repairParticle); }
    }

    public override void Remove()
    {
        block.visuals.NewVisual(block.scheme.model);
        block.effects.Desactivate(GameManager.instance.library.damagedParticle);
        block.Enable();

        base.Remove();
    }
}
