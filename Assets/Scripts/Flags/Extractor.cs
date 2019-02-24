using UnityEngine;

public class Extractor : Occupator
{
	[Header("Extraction")]
	public Mine mine;
	Mine oldMine;

	public override void OnGridUpdate()
	{
		base.OnGridUpdate();
		oldMine = mine;
		mine = null;

        MissionManager.Mission newMission = GameManager.instance.missionManager.PrepareNewMission();
        newMission.position = block.gridCoordinates;
        newMission.callBack = "";
        newMission.range = 1;
		Block[] blocks = GameManager.instance.missionManager.CheckAdjacentBlocks(block.gridCoordinates, newMission).ToArray();
		GameManager.instance.missionManager.EndMission(newMission);

		foreach(Block b in blocks)
		{
			Mine m = b.GetComponent<Mine>();
			if(m != null)
			{
				mine = m;
				if(mine != oldMine) mine.Cage();
				break;
			}
		}

		if(oldMine != null && oldMine != mine) oldMine.Uncage();
		if(mine == null) isEnabled = false;
	}

    public override void OnNewMicrocycle()
    {
        if(!isEnabled) return;
		if(mine != null)
		{
			mine.health -= affectedCitizen.Count;
		}
    }

	public System.Type GetFlagType(){ return GetType(); }
	public string GetFlagDatas(){ return "Extractor"; }
}
