using UnityEngine;

public class Unpowered : StateBehavior  
{
    public override void Start()
    {
		disabler = true;
		base.Start();

		if(block.states.ContainsKey(State.Powered))
		{
			block.RemoveState(State.Powered);
		}
		else
		{
			block.effects.Activate(GameManager.instance.library.unpoweredParticle);
			block.DisableFlags();
		}
    }

    public override void Remove()
    {
		if(!block.states.ContainsKey(State.Powered))
		{
			block.AddState(State.Powered);
		}
		else
		{
			block.effects.Desactivate(GameManager.instance.library.unpoweredParticle);
			block.EnableFlags();
		}

        base.Remove();
    }
}
