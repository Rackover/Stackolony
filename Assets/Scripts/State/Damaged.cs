using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damaged : StateBehavior 
{
    public override void Start()
    {
        disabler = true;
        refresher = true;
        base.Start();

        block.effects.Activate(GameManager.instance.library.damagedParticle);
        block.DisableFlags();
    }

    public override void Remove()
    {
        base.Remove();

        block.effects.Desactivate(GameManager.instance.library.damagedParticle);
        block.EnableFlags();

        Destroy(this);
    }
}
