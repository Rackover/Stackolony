using UnityEngine;

public class Powered : StateBehavior 
{
    public override void Start()
    {
		base.Start();

		if(block.states.ContainsKey(State.Unpowered))
		{
			block.RemoveState(State.Unpowered);
		}
		else
		{
			//block.effects.Desactivate(GameManager.instance.library.unpoweredParticle);
      block.Enable();
		}
    }

    public override void Remove()
    {
        if(!block.states.ContainsKey(State.Unpowered))
		{
			block.AddState(State.Unpowered);
		}
		else
		{
			//block.effects.Activate(GameManager.instance.library.unpoweredParticle);
      block.Disable();
		}
		
		base.Remove();
    }
}
