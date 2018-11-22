using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BlockBehaviour : MonoBehaviour 
{
	[Header("Referencies")]
	public Block myBlock;
	public GameObject myBlockObject;
	[HideInInspector] public BlockLink blockLink;

	public GameObject powerParticule;

	[Header("System settings")]
	public float BlockRefreshCooldown = 1f;
	float timer;

	void Start()
	{
		blockLink = GetComponent<BlockLink>();
		myBlockObject = Instantiate(myBlock.model, transform.position, Quaternion.identity, transform);
        myBlockObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

		myBlock.behaviour = this;
		myBlock.LoadBlock();

		timer = Time.time;
	}

	void Update()
	{
		/* 
		if(timer >= Time.time)
		{
			myBlock.UpdateBlock();
			timer = Time.time + BlockRefreshCooldown;
		}
		*/

		myBlock.UpdateBlock();
	}
}
