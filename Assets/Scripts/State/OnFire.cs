using UnityEngine;

public class OnFire : StateBehavior 
{
    [Header("On fire")]
    public bool isBeingExtinguished = false;
    public int id;

    public override void Start()
    {
        disabler = true;
        base.Start();

        // Add fire effect and play fire sound
        block.effects.Activate(GameManager.instance.library.onFireParticle);
        GameManager.instance.soundManager.Play("StartingFire");
        block.Disable();
    }

    public override void OnGridUpdate()
    {
        base.OnGridUpdate();
        CancelExtinguish();
    }

    public void StartExtinguish()
    {
        if(!isBeingExtinguished)
        {
            isBeingExtinguished = true;
            if(block != null) block.effects.Activate(GameManager.instance.library.extinguishParticle);
        }
    }
    
    public void CancelExtinguish()
    {
        isBeingExtinguished = false;

        if(block != null) block.effects.Desactivate(GameManager.instance.library.extinguishParticle);
    }

    public override void OnNewMicrocycle()
    {
        base.OnNewMicrocycle();
        if(!isBeingExtinguished)
        {
            Spread();
        }
        else 
        {
            block.Enable();
            Remove();
        }
    }

    public void Spread()
    {
        block = GetComponent<Block>();
        FireManager.Spread(this);

        if(block.states.ContainsKey(State.Damaged))
        {
            block.Destroy();
        }
        else
        {
            block.AddState(State.Damaged);
            Remove();
        }
    }

    public override void Remove()
    {
        block.effects.Desactivate(GameManager.instance.library.extinguishParticle);
        block.effects.Desactivate(GameManager.instance.library.onFireParticle);
        GameManager.instance.soundManager.Play("StoppingFire");
        base.Remove();
    }
}
