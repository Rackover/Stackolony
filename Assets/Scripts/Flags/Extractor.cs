using UnityEngine;

public class Extractor : Flag, Flag.IFlag
{
	public Occupator occupator;
	public Mine mine;

	public  void Awake()
	{
		base.Awake();
		occupator = gameObject.GetComponent<Occupator>();
		if(occupator == null) Destroy(this);
	}

	public override void OnGridUpdate()
	{
		base.OnGridUpdate();
		mine = null;
        MissionManager.Mission newMission = GameManager.instance.missionManager.PrepareNewMission();
        newMission.position = block.gridCoordinates;
        newMission.callBack = "";
        newMission.range = 1;
/*
		Block[] blocks = GameManager.instance.missionManager.CheckAdjacentBlocks(block.gridCoordinates, newMission).ToArray();

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
			occupator.isEnabled = false;
		}
*/
	}

    public override void OnNewMicrocycle()
    {
        if(!isEnabled) return;

		if(mine != null)
		{
			mine.health -= occupator.affectedCitizen.Count;
		}
    }

	public System.Type GetFlagType(){ return GetType(); }
}
