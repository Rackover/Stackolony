using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiremanStation : Flag, Flag.IFlag
{
	public int range;

	public override void Update()
	{
		base.Update();
	}

	public override void OnNewMicrocycle()
	{	
		base.OnNewMicrocycle();
		Debug.Log("FireMan duty");
	}

    public System.Type GetFlagType(){ return GetType(); }
}
