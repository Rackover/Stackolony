using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamerBehavior : MonoBehaviour
{
	[HideInInspector] public RoamerManager manager;
	[HideInInspector] public Bystander visual;
	[HideInInspector] public bool available;

	public List<Vector3> path = new List<Vector3>();
	int cPoint;

	void Update()
	{
		if(cPoint < path.Count)
		{

		}
	}

	public void NewPath(List<Vector3> newPath)
	{
		path = newPath;
		cPoint = 0;
		transform.position = path[0];
	}

	void MoveToNext()
	{
		cPoint++;

		if(cPoint < path.Count)
		{
			Debug.Log("Next");
		}
		else
		{
			Debug.Log("Arrived");
		}
	}

	public void LoadVisual()
	{
		if(visual != null) Destroy(visual.gameObject);

		visual = Instantiate(
			manager.bystanderPrefabs[Random.Range(0, manager.bystanderPrefabs.Length)],
			transform.position,
			Quaternion.identity,
			transform
		).GetComponent<Bystander>();
	}
}
