using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extinctor : MonoBehaviour 
{

	public float distance = 1f;
	public float speed = 1f;
	public Transform pivot;

	void Start()
	{
		//pivot = transform.position + Vector3.forward * 2;
	}

	void Update()
	{
		transform.LookAt(pivot);
		transform.position = new Vector3((pivot.position.x + Mathf.Sin(Time.time * speed))*distance, pivot.position.y, (pivot.position.z + Mathf.Cos(Time.time * speed))*distance);
	}

}
