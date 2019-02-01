using UnityEngine;

public class OnRiot : BlockState 
{
    public override void Start()
    {
        block.effects.Activate(GameManager.instance.library.onRiotParticle);

        foreach(Flag f in block.activeFlags)
        {
            f.Disable();
        }
    }

    public override void Remove()
    {
        block.effects.Desactivate(GameManager.instance.library.onRiotParticle);

        foreach(Flag f in block.activeFlags)
        {
            f.Enable();
        }
    }
}
