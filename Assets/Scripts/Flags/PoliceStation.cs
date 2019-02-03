using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceStation : Flag, Flag.IFlag
{
	public int range;

    public System.Type GetFlagType()
    {
        return GetType();
    }

    public string GetFlagDatas()
    {
        return "PoliceStation_" + range;
    }
}
