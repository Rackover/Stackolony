using UnityEngine;

public class OnFire : StateBehavior 
{
    public override void Start()
    {
        disabler = true;
        refresher = true;
        base.Start();

        // Add fire effect and play fire sound
        block.effects.Activate(GameManager.instance.library.onFireParticle);
        GameManager.instance.soundManager.Play("StartingFire");

        // Disactivate all flags of the block
        block.DisableFlags();
    }

    public override void OnNewCycle()
    {
        base.OnNewCycle();
        
        GameManager.instance.missionManager.StartMission(block.gridCoordinates, "Ignite", 1);

        Remove();
    }


    public override void Remove()
    {
        base.Remove();
        
        // Remove effect and play sounds
        block.effects.Desactivate(GameManager.instance.library.onFireParticle);
        GameManager.instance.soundManager.Play("StoppingFire");

        // Reactivate all flags of the block
        block.EnableFlags();

        Destroy(this);
    }
}
