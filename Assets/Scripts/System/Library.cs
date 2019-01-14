using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Library : MonoBehaviour 
{
	[Header("Prefabs")]
	public GameObject blockPrefab;
	public GameObject extinctorPrefab;
    public GameObject spatioportSpawnerPrefab;
    public GameObject disabledBlockPrefab;

	[Space(1)][Header("Blocks")]
	public BlockScheme[] blocks;
	public Color[] blockContainerColors;

	[Space(1)][Header("Sprites")]
	public Sprite cursorSprite;
	public Sprite destroyIcon;
	public Sprite buildIcon;
    public Sprite dragIcon;
    public Sprite bridgeIcon;

	public BlockScheme GetBlockByID(int id)
	{	
		foreach(BlockScheme b in blocks)
		{
			if(b.ID == id)
			{
				return b;
			}
		}
		return null;
	}
}
