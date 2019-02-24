using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamerManager : MonoBehaviour
{
	List<RoamerBehavior> roamers = new List<RoamerBehavior>();

	[Header("Prefabs")]
	public GameObject roamer;
	public GameObject[] bystanderPrefabs;

	[Header("Settings")]
	public float minSpawnRate = 0.1f;
	public float maxSpawnRate = 5f;

	Transform roamerHolder;
	float spawnRate = 1f;
	float timer = 0f;
	List<int> popInCity = new List<int>();
	int totalPopulation;

	int simulationTimeScale;

	void Start()
	{
		roamerHolder = new GameObject().transform;
		roamerHolder.name = "RoamerHolder";
	}

	public void OnNewMicrocycle()
	{
		RefreshPopulationInCity();

		spawnRate = maxSpawnRate / (totalPopulation + 1);
		if(spawnRate < minSpawnRate) spawnRate = maxSpawnRate;
	}

	void Update()
	{
		if(!GameManager.instance.IsInGame())
		{
			End();
			return;
		}

		timer += Time.deltaTime;
		if(timer > spawnRate)
		{
			if(GameManager.instance.gridManagement.bridgesList.Count > 0
			&& popInCity.Count > 0
			&& totalPopulation > 0
			&& simulationTimeScale > 0)
			{
				NewRoamer(popInCity[Random.Range(0, popInCity.Count)]);
			}
			timer = 0f;
		}
	}

	public void ChangeTimeScale(int scale)
	{
		simulationTimeScale = scale;

		foreach(RoamerBehavior rb in roamers)
		{
			rb.currentSpeed = rb.baseSpeed * simulationTimeScale;
			rb.visual.animator.SetFloat("Speed", simulationTimeScale);
		}
	}

	public void End()
	{
		foreach(RoamerBehavior rb in roamers)
		{
			Destroy(rb.gameObject);
		}
		roamers.Clear();
	}

	public void NewRoamer(int which)
	{
		// Get the reference to a random bridge
		Vector3 hShift = new Vector3(0f, 0.25f, 0f);
        BridgeInfo bridge = GameManager.instance.gridManagement.bridgesList[
            Random.Range(0, GameManager.instance.gridManagement.bridgesList.Count-1)
        ].GetComponent<BridgeInfo>();

		// If the bridge is on a disabled block, cancel the Roamer spawn
		Block linkedBlock = GameManager.instance.gridManagement.grid[bridge.origin.x, bridge.origin.y, bridge.origin.z].GetComponent<Block>();
		if(linkedBlock != null && !linkedBlock.isEnabled)
		{
			return;
		}
		
		// Pull a new available roamer from the list
		RoamerBehavior roamer = GetRoamer();

		Vector3 from = GameManager.instance.gridManagement.IndexToWorldPosition(bridge.origin);
		Vector3 to = GameManager.instance.gridManagement.IndexToWorldPosition(bridge.destination);

		// From start to end or from end to start ? and initialyze the Roamer
		if(Random.Range(0, 2) == 0)
		{
			roamer.Initialize(which, from - hShift, to - hShift, bridge.gameObject);
		}
		else
		{
			roamer.Initialize(which, to - hShift, from - hShift, bridge.gameObject);
		}

		roamer.currentSpeed = roamer.baseSpeed * simulationTimeScale;
		roamer.visual.animator.SetFloat("Speed", simulationTimeScale);
	}

	void RefreshPopulationInCity()
	{
		popInCity.Clear();
		int tPop = 0;
		foreach(KeyValuePair<Population, PopulationManager.PopulationInformation> entry in GameManager.instance.populationManager.populations)
		{
			if(entry.Value.citizens.Count > 0)
			{
				popInCity.Add(entry.Key.ID);
				tPop += entry.Value.citizens.Count;
			}
		}
		totalPopulation = tPop;
	}

	RoamerBehavior GetRoamer()
	{
		// Search for an available roamer
		foreach(RoamerBehavior roamer in roamers)
		{
			if(roamer.available) return roamer;
		}

		// If none are available, spawn a fresh new one
		roamers.Add(Instantiate(roamer).GetComponent<RoamerBehavior>());
		roamers[roamers.Count - 1].transform.SetParent(roamerHolder);
		roamers[roamers.Count - 1].manager = this;
		return roamers[roamers.Count - 1];
	}
}