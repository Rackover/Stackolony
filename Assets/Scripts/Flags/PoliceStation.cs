using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceStation : Flag, Flag.IFlag
{
    [Header("Police Behavior")]
	public int range;
    public int nuisanceImpact;

    public override void Awake()
    {
        base.Awake();
    }

	public override void OnGridUpdate()
	{
		base.OnGridUpdate();
		if(!isEnabled) return;
		GameManager.instance.missionManager.StartMission(block.gridCoordinates, "Repress", range);
	}
  
    public System.Type GetFlagType() { return GetType(); }


    override public void UpdateNuisanceImpact()
    {
        range += nuisanceImpact;
        nuisanceImpact = block.nuisance * block.scheme.sensibility;
        range -= nuisanceImpact;

    public string GetFlagDatas()
    {
        return "PoliceStation_" + range;
    }
}
