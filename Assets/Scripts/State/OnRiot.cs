using UnityEngine;

public class OnRiot : StateBehavior 
{
    public override void Start()
    {
        disabler = true;
        base.Start();

        block.effects.Activate(GameManager.instance.library.onRiotParticle);
        block.visuals.animator.SetBool("OnRiot", true);
        block.DisableFlags();
        StartCoroutine(GameManager.instance.systemManager.OnGridUpdate());
    }

    public override void Remove()
    {
        block.effects.Desactivate(GameManager.instance.library.onRiotParticle);
        block.visuals.animator.SetBool("OnRiot", false);
        block.EnableFlags();
        StartCoroutine(GameManager.instance.systemManager.OnGridUpdate());

        base.Remove();
    }
}
