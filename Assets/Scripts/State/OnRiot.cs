using UnityEngine;

public class OnRiot : StateBehavior 
{
    [Header("On riot")]
    public bool beingRepressed = false;
    int microcycleCount = 0;

    public override void Start()
    {
        disabler = true;
        base.Start();

        block.effects.Activate(GameManager.instance.library.onRiotParticle);
        block.visuals.animator.SetBool("OnRiot", true);
        block.DisableFlags();
        StartCoroutine(GameManager.instance.systemManager.OnGridUpdate());
    }

    public override void OnGridUpdate()
    {
        base.OnGridUpdate();
        CancelRepress();
    }

    public override void OnNewMicrocycle()
    {
        base.OnNewMicrocycle();
        if( microcycleCount++ > 1 ) FinishRepress();
    }

    public override void OnNewCycle()
    {
        base.OnNewCycle();
        if(!beingRepressed){ block.Destroy(); }
    }

    public void StartRepress()
    {
        if(!beingRepressed)
        {
            microcycleCount = 0;
            beingRepressed = true;
            if(block != null) block.effects.Activate(GameManager.instance.library.repressParticle);
        }
    }

    void FinishRepress()
    {
        if(block != null) block.effects.Desactivate(GameManager.instance.library.repressParticle);
        Remove();
    }

    void CancelRepress()
    {
        microcycleCount = 0;
        beingRepressed = false;
        if(block != null) block.effects.Desactivate(GameManager.instance.library.repressParticle);
    }

    public override void Remove()
    {
        Debug.Log("RemoveRiot");

        block.effects.Desactivate(GameManager.instance.library.onRiotParticle);
        block.visuals.animator.SetBool("OnRiot", false);
        block.EnableFlags();
        
        StartCoroutine(GameManager.instance.systemManager.OnGridUpdate());
        base.Remove();
    }
}
