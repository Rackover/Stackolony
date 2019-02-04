using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repairer : Flag, Flag.IFlag
{
	public int range;

	public override void Enable()
	{
		base.Enable();
		if(isEnabled) GameManager.instance.missionManager.StartMission(block.gridCoordinates, "Repair", range);
	}

	public override void OnGridUpdate()
	{
		base.OnGridUpdate();
		if(!isEnabled) return;
		if(isEnabled) GameManager.instance.missionManager.StartMission(block.gridCoordinates, "Repair", range);
	}

    public System.Type GetFlagType(){ return GetType(); }
    public string GetFlagDatas(){ return "Repairer_" + range; }
}
