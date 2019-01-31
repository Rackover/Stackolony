using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodModifier : Flag, Flag.IFlag {

    public int range;
    public int amount;
	public string[] profiles;

    public System.Type GetFlagType()
    {
        return GetType();
    }
}
