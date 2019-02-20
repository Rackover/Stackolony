using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamerBehavior : MonoBehaviour
{
	[HideInInspector] public RoamerManager manager;
	[HideInInspector] public Bystander visual;
	[HideInInspector] public bool available;

	public float moveSpeed;
	public Vector3 destination;
	public GameObject cBridge;
	Vector3 bridgePosition;

	void Update()
	{	
		if(!available) UpdateMovement();
	}

	void UpdateMovement()
	{
		if(cBridge == null || bridgePosition != cBridge.transform.position)
		{
			End();
		}

		transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);
		if(Vector3.Distance(transform.position, destination) < 0.5f)
		{		
			End();
		}
	}

	void End()
	{
		visual.gameObject.SetActive(false);
		available = true;
	}

	public void Initialize(int id, Vector3 from, Vector3 to, GameObject bridge)
	{
		if(visual != null) Destroy(visual.gameObject);
		visual = Instantiate(
			manager.bystanderPrefabs[id],
			transform.position,
			Quaternion.identity,
			transform
		).GetComponent<Bystander>();

		moveSpeed = Random.Range(0.5f, 1.5f);
		available = false;
		visual.gameObject.SetActive(true);
		transform.position = from;
		destination = to;
		visual.transform.LookAt(destination);
		visual.animator.Play("Running");

		cBridge = bridge;
		bridgePosition = cBridge.transform.position;
	}
}
