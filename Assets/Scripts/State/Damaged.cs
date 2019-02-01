using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damaged : StateBehavior 
{
    public override void Start()
    {
        base.Start();

        block.effects.Activate(GameManager.instance.library.damagedParticle);

        foreach(Flag f in block.activeFlags)
        {
            f.Disable();
        }
    }

    public override void Remove()
    {
        base.Remove();

        block.effects.Desactivate(GameManager.instance.library.damagedParticle);

        foreach(Flag f in block.activeFlags)
        {
            f.Enable();
        }

        Destroy(this);
    }
}
