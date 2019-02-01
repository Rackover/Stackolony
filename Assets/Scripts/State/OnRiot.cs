using UnityEngine;

public class OnRiot : StateBehavior 
{
    public override void Start()
    {
        base.Start();

        block.effects.Activate(GameManager.instance.library.onRiotParticle);
        block.visuals.animator.SetBool("OnRiot", true);

        foreach(Flag f in block.activeFlags)
        {
            f.Disable();
        }
    }

    public override void Remove()
    {
        base.Remove();

        block.effects.Desactivate(GameManager.instance.library.onRiotParticle);
        block.visuals.animator.SetBool("OnRiot", false);

        foreach(Flag f in block.activeFlags)
        {
            f.Enable();
        }

        Destroy(this);
    }
}
