using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repairer : Flag, Flag.IFlag
{
	public int range;

    public System.Type GetFlagType()
    {
        return GetType();
    }
}
