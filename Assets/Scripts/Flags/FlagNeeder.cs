using UnityEngine;

public class FlagNeeder : Flag, Flag.IFlag
{
	public string needed;
	public int range;

	public override void OnBlockUpdate()
	{
		base.OnBlockUpdate();
		// Check if there is the [needed] flag in the [range]
		/*
		if(true)
		{
			foreach (Flag flags in block.activeFlags) 
			{
				flags.Disable();
			}
		}
		*/
	}

    public System.Type GetFlagType()
    {
        return GetType();
    }

    public string GetFlagDatas()
    {
        return "FlagNeeder_" + needed + "_" + range;
    }
}
