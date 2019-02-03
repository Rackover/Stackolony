using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour 
{
	static int currentFire = 0;

	public static void Ignite(Block block)
	{
		block.AddState(State.OnFire);
		OnFire of = (OnFire)block.states[State.OnFire];
		of.id = currentFire++;
	}

	public static void Spread(OnFire fire)
	{
        MissionManager.Mission newMission = GameManager.instance.missionManager.PrepareNewMission();
        newMission.position = fire.block.gridCoordinates;
        newMission.callBack = "";
        newMission.range = 1;

		Block[] blocks = GameManager.instance.missionManager.CheckAdjacentBlocks(fire.block.gridCoordinates, newMission).ToArray();

		foreach(Block block in blocks)
		{
			if(block != null)
			{
				if(block.states.ContainsKey(State.Damaged))
				{
					Damaged d = (Damaged)block.states[State.Damaged];
					if(d.lastFireId != fire.id)
					{
						if(!block.states.ContainsKey(State.OnFire))
						{
							block.AddState(State.OnFire);
							OnFire f = (OnFire)block.states[State.OnFire];
							f.id = fire.id;	
						}
					}
				}
				else
				{
					if(!block.states.ContainsKey(State.OnFire))
					{
						block.AddState(State.OnFire);
						OnFire f = (OnFire)block.states[State.OnFire];
						f.id = fire.id;	
					}
				}
			}
		}
	}
}
