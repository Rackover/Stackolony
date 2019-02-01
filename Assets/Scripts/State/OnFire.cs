﻿using UnityEngine;

public class OnFire : BlockState 
{
    public override void Start()
    {
        base.Start();

        // Add fire effect and play fire sound
        block.effects.Activate(GameManager.instance.library.onFireParticle);
        GameManager.instance.soundManager.Play("StartingFire");

        // Disactivate all flags of the block
        foreach(Flag f in block.activeFlags)
        {
            f.Disable();
        }
    }

    public override void OnNewCycle()
    {
        base.OnNewCycle();
        GameManager.instance.missionManager.StartMission(block.gridCoordinates, "Ignite", 1);
    }


    public override void Remove()
    {
        base.Remove();
        
        // Remove effect and play sounds
        block.effects.Desactivate(GameManager.instance.library.onFireParticle);
        GameManager.instance.soundManager.Play("StoppingFire");

        // Reactivate all flags of the block
        foreach(Flag f in block.activeFlags)
        {
            f.Enable();
        }

        Destroy(this);
    }
}