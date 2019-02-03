using UnityEngine;

public class OnFire : StateBehavior 
{
    [Header("On fire")]
    public bool beingExtinguished = false;

    public override void Start()
    {
        disabler = true;
        base.Start();

        // Add fire effect and play fire sound
        block.effects.Activate(GameManager.instance.library.onFireParticle);
        GameManager.instance.soundManager.Play("StartingFire");

        // Refresh OnGridUpdate
        StartCoroutine(GameManager.instance.systemManager.OnGridUpdate());

        // Disactivate all flags of the block
        block.DisableFlags();
    }

    public override void OnGridUpdate()
    {
        base.OnGridUpdate();
        CancelExtinguish();
    }

    public void StartExtinguish()
    {
        if(!beingExtinguished)
        {
            Debug.Log("Starting Extinguishing");
            beingExtinguished = true;
            if(block != null)
            {
                block.effects.Activate(GameManager.instance.library.extinguishParticle);

            }
        }
    }
    
    public void CancelExtinguish()
    {
        beingExtinguished = false;

        if(block != null)
            block.effects.Desactivate(GameManager.instance.library.extinguishParticle);
    }

    public override void OnNewMicrocycle()
    {
        base.OnNewCycle();
        
        if(!beingExtinguished)
        {
            Spread();
        }
        else 
        {
            Remove();
        }
    }

    public void Spread()
    {
        GameManager.instance.missionManager.StartMission(block.gridCoordinates, "Ignite", 1);
        block.AddState(State.Damaged);
        Remove();
    }

    public override void Remove()
    {
        block.effects.Desactivate(GameManager.instance.library.extinguishParticle);
        block.effects.Desactivate(GameManager.instance.library.onFireParticle);
        GameManager.instance.soundManager.Play("StoppingFire");

        block.EnableFlags();

        base.Remove();
    }
}
