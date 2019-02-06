﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrack : Occupator {

	[Header("Holding")]
	public Nest nest;

	public override void OnGridUpdate()
	{
		base.OnGridUpdate();
		nest = null;
        MissionManager.Mission newMission = GameManager.instance.missionManager.PrepareNewMission();
        newMission.position = block.gridCoordinates;
        newMission.callBack = "";
        newMission.range = 1;

		Block[] blocks = GameManager.instance.missionManager.CheckAdjacentBlocks(block.gridCoordinates, newMission).ToArray();
		GameManager.instance.missionManager.EndMission(newMission);

		foreach(Block b in blocks)
		{
			Nest n = b.GetComponent<Nest>();
			if(n != null)
			{
				nest = n;
				break;
			}
		}

		if(nest == null)
		{
			isEnabled = false;
		}
	}

    public override void OnNewMicrocycle()
    {
        if(!isEnabled) return;
		if(nest != null)
		{
			nest.health -= affectedCitizen.Count;
		}
    }
}