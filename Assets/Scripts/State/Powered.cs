using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powered : StateBehavior 
{
    public override void Start()
    {
		base.Start();

		if(block.states.ContainsKey(State.Unpowered))
		{
			block.states[State.Unpowered].Remove();
		}
		else
		{
			block.effects.Desactivate(GameManager.instance.library.unpoweredParticle);
			foreach(Flag f in block.activeFlags) { f.Enable(); }
		}
    }

    public override void Remove()
    {
        base.Remove();

        if(block.states.ContainsKey(State.Unpowered))
		{
			block.AddState(State.Unpowered);
		}
		else
		{
			block.effects.Activate(GameManager.instance.library.unpoweredParticle);
			foreach(Flag f in block.activeFlags){ f.Disable(); }
		}

		Destroy(this);
    }
}
