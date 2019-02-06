using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repairer : Flag, Flag.IFlag
{
	public int range;
    private int nuisanceImpact;


    override public void UpdateNuisanceImpact()
    {
        range += nuisanceImpact;
        nuisanceImpact = block.nuisance * block.scheme.sensibility;
        range -= nuisanceImpact;
    }

    public System.Type GetFlagType()
    {
        return GetType();
    }

    public string GetFlagDatas()
    {
        return "Repairer_" + range;
    }
}
