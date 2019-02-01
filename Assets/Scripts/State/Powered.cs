using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powered : BlockState 
{
    public override void Start()
    {
		base.Start();

        block.effects.Activate(GameManager.instance.library.unpoweredParticle);

        foreach(Flag f in block.activeFlags)
        {
            f.Disable();
        }
    }

    public override void Remove()
    {
        block.effects.Desactivate(GameManager.instance.library.unpoweredParticle);

        foreach(Flag f in block.activeFlags)
        {
            f.Enable();
        }
    }
}
