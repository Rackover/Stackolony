using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiremanStation : Flag, Flag.IFlag
{
	[Header("Fireman Behavior")]
	public int range;


	public override void Enable()
	{
		base.Enable();
		if(isEnabled) GameManager.instance.missionManager.StartMission(block.gridCoordinates, "Extinguish", range);
	}
	
	public override void OnGridUpdate()
	{
		base.OnGridUpdate();
		if(!isEnabled) return;
		if(isEnabled) GameManager.instance.missionManager.StartMission(block.gridCoordinates, "Extinguish", range);
	}

   	public System.Type GetFlagType(){ return GetType(); }
}
