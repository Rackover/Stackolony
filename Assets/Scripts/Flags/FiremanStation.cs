using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiremanStation : Flag 
{
	[Header("Policeman")]
	public int range;
	public bool selecting;
	public List<BlockLink> targets = new List<BlockLink>();
	public List<Extinctor> extinctors = new List<Extinctor>();

	override public void Use()
	{
		selecting = true;
	}

	override public void OnNewCycle()
	{
		foreach(BlockLink target in targets)
		{
			target.RemoveState(BlockState.OnFire);
		}
		foreach(Extinctor extinctor in extinctors)
		{
			Destroy(extinctor.gameObject);
		}

		targets.Clear();
		extinctors.Clear();
	}

	override public void UpdateFlag()
	{
		if(selecting)
		{
			if (Input.GetButtonDown("MouseLeft")) 
			{
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out hit))
				{
					BlockLink sBlock = hit.transform.gameObject.GetComponent<BlockLink>();
					if(sBlock != null)
					{
						selecting = false;
						targets.Add(sBlock);
						extinctors.Add(Instantiate(myBlockLink.lib.extinctorPrefab, transform.position, Quaternion.identity).GetComponent<Extinctor>());
						extinctors[extinctors.Count-1].target = targets[targets.Count-1].transform;
					}
				}
			}
		}
	}
}
