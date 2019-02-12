using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamerManager : MonoBehaviour
{
	List<RoamerBehavior> roamers = new List<RoamerBehavior>();

	[Header("Prefabs")]
	public GameObject roamer;
	public GameObject[] bystanderPrefabs;

	void Start()
	{
		NewRoamer();
	}

	void NewRoamer()
	{
		RoamerBehavior roamer = GetRoamer();
		roamer.manager = this;
		roamer.LoadVisual();
		roamer.NewPath(GetRandomPosition(10));
		roamer.visual.animator.Play("Running");
	}

	List<Vector3> GetRandomPosition(int lenght)
	{
		List<Vector3> randomPath = new List<Vector3>();
		for( int i = 0; i < lenght; i++ )
		{
			randomPath.Add(new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f)));
		}
		return randomPath;
	}

	RoamerBehavior GetRoamer()
	{
		foreach(RoamerBehavior roamer in roamers)
		{
			if(roamer.available) return roamer;
		}
		roamers.Add(Instantiate(roamer).GetComponent<RoamerBehavior>());
		return roamers[roamers.Count - 1];
	}
}	
