using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unpowered : StateBehavior  
{
    public override void Start()
    {
		base.Start();
		if(block.states.ContainsKey(State.Powered))
		{
			block.states[State.Powered].Remove();
		}
		else
		{
			block.effects.Activate(GameManager.instance.library.unpoweredParticle);
			foreach(Flag f in block.activeFlags) {f.Disable(); }
		}
    }

    public override void Remove()
    {
        base.Remove();

		if(block.states.ContainsKey(State.Powered))
		{
			block.AddState(State.Powered);
		}
		else
		{
			block.effects.Desactivate(GameManager.instance.library.unpoweredParticle);
			foreach(Flag f in block.activeFlags){ f.Enable(); }
		}

		Destroy(this);
    }
}
