using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damaged : StateBehavior 
{
    public override void Start()
    {
        disabler = true;
        base.Start();

        block.effects.Activate(GameManager.instance.library.damagedParticle);
        block.DisableFlags();
        StartCoroutine(GameManager.instance.systemManager.OnGridUpdate());
    }

    public override void Remove()
    {
        block.effects.Desactivate(GameManager.instance.library.damagedParticle);
        block.EnableFlags();
        StartCoroutine(GameManager.instance.systemManager.OnGridUpdate());

        base.Remove();
    }
}
