using UnityEngine;

public class OnRiot : StateBehavior 
{
    [Header("On riot")]
    public bool isBeingRepressed = false;
    int microcycleCount = 0;

    public override void Start()
    {
        disabler = true;
        base.Start();

        block.effects.Activate(GameManager.instance.library.onRiotParticle);
        block.visuals.animator.SetBool("OnRiot", true);
        block.Disable();
    }

    public override void OnGridUpdate()
    {
        base.OnGridUpdate();
        CancelRepress();
    }

    public override void OnNewMicrocycle()
    {
        base.OnNewMicrocycle();
        microcycleCount++;
        
        if( microcycleCount > 1 ) 
        {
            FinishRepress();
        }
    }

    public override void OnNewCycle()
    {
        base.OnNewCycle();
        if(!isBeingRepressed){ block.Destroy(); }
    }

    public void StartRepress()
    {
        if(!isBeingRepressed)
        {
            microcycleCount = 0;
            isBeingRepressed = true;
            if(block != null) block.effects.Activate(GameManager.instance.library.repressParticle);
        }
    }

    void FinishRepress()
    {
        AchievementManager.achievements.AddToValue("stoppedRiots");
        if(block != null) block.effects.Desactivate(GameManager.instance.library.repressParticle);
        Remove();
    }

    void CancelRepress()
    {
        microcycleCount = 0;
        isBeingRepressed = false;
        if(block != null) block.effects.Desactivate(GameManager.instance.library.repressParticle);
    }

    public override void Remove()
    {
        block.effects.Desactivate(GameManager.instance.library.onRiotParticle);
        block.visuals.animator.SetBool("OnRiot", false);
        block.Enable();
        base.Remove();
    }
}
