using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempIgniter : MonoBehaviour {

	void Update () 
	{
		if (Input.GetButtonDown("Select") && Input.GetKey(KeyCode.F)) 
		{ 
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit))
			{
				BlockLink block = hit.collider.gameObject.GetComponent<BlockLink>();
				if(block != null)
				{
					if(!block.states.Contains(BlockState.OnFire))
						block.AddState(BlockState.OnFire);
					else
						block.RemoveState(BlockState.OnFire);
				}
			}
		}
	}
}
