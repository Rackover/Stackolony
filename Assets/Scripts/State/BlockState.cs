using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockState : MonoBehaviour 
{

	[HideInInspector] public Block block;

	// When the State is added
	virtual public void Start()
	{
		if(block == null) block = gameObject.GetComponent<Block>();
		if(block == null) Destroy(this);
	}

	// When the State is removed
    virtual public void Remove() 
	{
		if(block.states.ContainsValue(this))
		{
			foreach(KeyValuePair<State, BlockState> state in block.states)
			{
				if(Object.ReferenceEquals(state.Value, this))
				{
					block.states.Remove(state.Key);
					break;
				}
			}
		}
	}

	// When a new Cycle start
	virtual public void OnNewCycle() {}

	// When a new Microcycle start
	virtual public void OnNewMicrocycle() {}
}
