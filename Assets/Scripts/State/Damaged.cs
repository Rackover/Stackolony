using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damaged : BlockState 
{
    public override void Start()
    {
        block.effects.Activate(GameManager.instance.library.damagedParticle);

        foreach(Flag f in block.activeFlags)
        {
            f.Disable();
        }
    }

    public override void Remove()
    {
        block.effects.Desactivate(GameManager.instance.library.damagedParticle);

        foreach(Flag f in block.activeFlags)
        {
            f.Enable();
        }
    }
}
