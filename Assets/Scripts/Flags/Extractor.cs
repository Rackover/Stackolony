using UnityEngine;

public class Extractor : Occupator
{
	[Header("Extractor")]
	public Mine mine;

	public override void OnGridUpdate()
	{
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
				break;
			}
		}

		if(mine == null)
		{
			isEnabled = false;
		}
	}

    public override void OnNewMicrocycle()
    {
        if(!isEnabled) return;
		if(mine != null)
		{
			mine.health -= 5f;
		}
    }

	public System.Type GetFlagType(){ return GetType(); }
	public string GetFlagDatas(){ return "Extractor"; }
}
