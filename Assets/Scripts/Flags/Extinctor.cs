using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extinctor : MonoBehaviour 
{
	[Header("Setting")]
	public float distance = 1f;
	public float speed = 1f;

	[Header("Referencies")]
	public GameObject waterParticule;
	public Transform target;
	bool onTarget = false;

	void Update()
	{
		if(!onTarget)
		{
			transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
			if(Vector3.Distance(transform.position, target.position) < distance)
			{
				waterParticule.SetActive(true);
				onTarget = true;
			}	
		}
		else
		{		
			transform.LookAt(target);
			transform.position = new Vector3((target.position.x + Mathf.Sin(Time.time * speed/4))*distance, target.position.y, (target.position.z + Mathf.Cos(Time.time * speed/4))*distance);
		}
	}
}
