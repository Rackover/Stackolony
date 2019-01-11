using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Library : MonoBehaviour 
{
	[Header("Prefabs")]
	public GameObject blockPrefab;
	public GameObject extinctorPrefab;

	[Space(1)][Header("Blocks")]
	public Block[] blocks;

	[Space(1)][Header("Sprites")]
	public Sprite cursorSprite;
	public Sprite destroyIcon;
	public Sprite buildIcon;
	public Sprite bridgeIcon;

	public Block GetBlockByID(int id)
	{	
		foreach(Block b in blocks)
		{
			if(b.ID == id)
			{
				return b;
			}
		}
		return null;
	}
}
