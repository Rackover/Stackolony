using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiremanStation : Flag, Flag.IFlag
{
	[Header("FiremanStation")]
	public int range;
	public bool selecting;
	public List<Block> targets = new List<Block>();
	public List<Extinctor> extinctors = new List<Extinctor>();
    public int nuisanceImpact;

    override public void Use()
	{
		selecting = true;
	}

    override public void UpdateNuisanceImpact()
    {
        range += nuisanceImpact;
        nuisanceImpact = block.nuisance * block.scheme.sensibility;
        range -= nuisanceImpact;
    }

    override public void OnNewCycle()
	{
		foreach(Block target in targets)
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
		base.UpdateFlag();
		
		if(selecting)
		{
			if (Input.GetButtonDown("Select")) 
			{
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out hit))
				{
					Block sBlock = hit.transform.gameObject.GetComponent<Block>();
					if(sBlock != null)
					{
						selecting = false;
						targets.Add(sBlock);
						extinctors.Add(Instantiate(GameManager.instance.library.extinctorPrefab, transform.position, Quaternion.identity).GetComponent<Extinctor>());
						extinctors[extinctors.Count-1].target = targets[targets.Count-1].transform;
					}
				}
			}
		}
	}

    public System.Type GetFlagType()
    {
        return GetType();
    }
}
