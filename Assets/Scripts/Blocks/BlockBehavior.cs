using UnityEngine;

public class BlockBehavior : MonoBehaviour 
{
	public Block myBlock;
	
	// TEMP
	[HideInInspector] public BlockLink blockLink;

	void Awake()
	{
		blockLink = GetComponent<BlockLink>();
	}
}
