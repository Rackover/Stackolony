using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateRemover : Flag, Flag.IFlag
{
	public int range;
	public string targetState;
	public bool activable;

    public System.Type GetFlagType()
    {
        return GetType();
    }

    public string GetFlagDatas()
    {
        return "StateRemover_" + range;
    }
}
