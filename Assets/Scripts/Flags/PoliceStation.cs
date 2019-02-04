using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceStation : Flag, Flag.IFlag
{
    [Header("Police Behavior")]
	public int range;

	public override void Enable()
	{
		base.Enable();
		//if(isEnabled) GameManager.instance.missionManager.StartMission(block.gridCoordinates, "Repress", range);
	}

	public override void OnGridUpdate()
	{
		base.OnGridUpdate();
		if(!isEnabled) return;
		if(isEnabled) GameManager.instance.missionManager.StartMission(block.gridCoordinates, "Repress", range);
	}

    public System.Type GetFlagType() { return GetType(); }
    public string GetFlagDatas() { return "PoliceStation_" + range; }
}
