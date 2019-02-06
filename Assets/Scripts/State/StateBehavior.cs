using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateBehavior : MonoBehaviour 
{
	[HideInInspector] public Block block; // Reference to the linked block
	[HideInInspector] public bool disabler = false;	// Determine if this state disable all flags or not

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
			foreach(KeyValuePair<State, StateBehavior> state in block.states)
			{
				if(Object.ReferenceEquals(state.Value, this))
				{
					block.states.Remove(state.Key);
					break;
				}
			}
		}
		Destroy(this);
	}

	// When a new Cycle start
	virtual public void OnNewCycle() {}

	// When a new Microcycle start
	virtual public void OnNewMicrocycle() {

	}

	virtual public void OnGridUpdate() {}
}
