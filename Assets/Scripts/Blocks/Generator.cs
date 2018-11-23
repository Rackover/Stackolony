using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
	[HideInInspector] public BlockLink link;
	public Generation myGeneration;
	public List<BlockLink> Gainers = new List<BlockLink>();

	void NewGainer(BlockLink b)
	{
		b.currentPower += myGeneration.power;
		Gainers.Add(b);
	}

	void RemoveGainer(BlockLink b)
	{
		b.currentPower -= myGeneration.power;
		Gainers.Remove(b);
	}


	// HUGE PERFORMANCE PROBLEM : GETCOMPONENT, COMPARE FORLOOPS SUSPECTED !
	public void RefreshGainers()
	{
		BlockLink targetLink;
		List<BlockLink> nearBlocks = new List<BlockLink>();

		int coordx = link.gridCoordinates.x;
		int coordy = link.gridCoordinates.y;
		int coordz = link.gridCoordinates.z;

		for( int i = 1; i < myGeneration.range; i++ ) // Above blocks
		{
			if(coordy - i < link.gridManager.maxHeight && link.gridManager.grid[coordx, coordy + i, coordz] != null) 
			{
				targetLink = link.gridManager.grid[coordx, coordy + i, coordz].GetComponent<BlockLink>();
                if(targetLink != null && targetLink.myBlock != null)
					nearBlocks.Add(targetLink);
			}

			if(coordy - i >= 0 && link.gridManager.grid[coordx, coordy - i, coordz] != null) 
			{
				targetLink = link.gridManager.grid[coordx, coordy - i, coordz].GetComponent<BlockLink>();
                if(targetLink != null && targetLink.myBlock != null)
					nearBlocks.Add(targetLink);
			}
		}

		// Remove Blocks that arn't near the generator anymore
		for( int i = 0; i < Gainers.Count; i++ )
		{
			if(!nearBlocks.Contains(Gainers[i]))
                RemoveGainer(Gainers[i]);
		}

		// Add the new Blocks that has appeared near the generator
		for( int i = 0; i < nearBlocks.Count; i++ )
		{
			if(!Gainers.Contains(nearBlocks[i]))
                NewGainer(nearBlocks[i]);
		}
	}
}
