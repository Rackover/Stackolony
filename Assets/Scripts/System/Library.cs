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

	[Header("Particules")]
	public GameObject citizenInParticle;
	public GameObject citizenOutParticle;
	public GameObject unpoweredParticle;
	public GameObject onFireParticle;
	public GameObject onRiotParticle;
	public GameObject damagedParticle;
	
	[Header("Building")]
    public List<Sprite> buildingsIcons = new List<Sprite>();

	[Header("Sounds")]
	public Sounds soundBank;
	public AudioClip mainMusic;

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

    public BlockScheme GetRandomBlock()
    {
        List<BlockScheme> candidates = new List<BlockScheme>();
        foreach (BlockScheme b in blocks)
        {
            if (b.isBuyable == true && !GameManager.instance.cityManager.IsLocked(b.ID))
            {
                candidates.Add(b);
            }
        }
        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count - 1)];
        }
        return null;
    }

    public bool BlockExists(int id)
    {
        foreach (BlockScheme b in blocks) {
            if (b.ID == id) {
                return true;
            }
        }
        return false;
    }
}
