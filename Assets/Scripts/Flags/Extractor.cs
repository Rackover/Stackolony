using UnityEngine;

public class Extractor : Occupator
{
	[Header("Extraction")]
	public Mine mine;

	Mine pMine;

	public override void OnGridUpdate()
	{
		pMine = mine;

		base.OnGridUpdate();
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
				mine.Caged();
				break;
			}
		}
		if(mine == null)
		{
			isEnabled = false;
		}

		Debug.Log(pMine + " " + mine);
		if(pMine != null && pMine != mine) pMine.Uncaged();
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
