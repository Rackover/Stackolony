using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGenerator", menuName = "Block/Generator")]
public class Generator : Block 
{
	public int range;
	public float count;

	public List<Block> reachedBlocks = new List<Block>();

	override public void UpdateBlock()
	{
		if(base.IsPowered())
		{
			if(jobs.Length == 0) // This generator don't require any job to be filled
			{
				//manager.blockLink.gridManager;
			}
		}
	}
}
