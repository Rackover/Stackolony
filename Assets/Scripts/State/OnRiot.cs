using UnityEngine;

public class OnRiot : StateBehavior 
{
    public override void Start()
    {
        disabler = true;
        refresher = true;
        base.Start();

        block.effects.Activate(GameManager.instance.library.onRiotParticle);
        block.visuals.animator.SetBool("OnRiot", true);
        block.DisableFlags();
    }

    public override void Remove()
    {
        base.Remove();

        block.effects.Desactivate(GameManager.instance.library.onRiotParticle);
        block.visuals.animator.SetBool("OnRiot", false);
        block.EnableFlags();

        Destroy(this);
    }
}
