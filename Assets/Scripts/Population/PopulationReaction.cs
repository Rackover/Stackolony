using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulationReaction : MonoBehaviour 
{
	[Header("Settings")]
	public float angryCap = 0.2f;
	public float chanceInc = 0.02f;
	public float chanceDec = 0.02f;
	public Text test;

	void Start () 
	{
		GameManager.instance.temporality.OnNewCycle += OnNewCycle;
		GameManager.instance.temporality.OnNewMicroCycle += OnNewMicrocycle;
	}

	void Update()
	{	
		test.text = "";

		foreach (KeyValuePair<Population, PopulationManager.PopulationInformation> p in GameManager.instance.populationManager.populations)
		{
			test.text += p.Value.averageMood.ToString() + " ";
		}
	}

    public void OnNewCycle()
    {
		
    }

    public void OnNewMicrocycle()
    {
		foreach (KeyValuePair<Population, PopulationManager.PopulationInformation> p in GameManager.instance.populationManager.populations)
		{
			if(p.Value.averageMood <= angryCap)
			{
				p.Value.riotRisk += chanceInc / GameManager.instance.temporality.nbMicroCyclePerCycle;
				if(p.Value.riotRisk > 1f) p.Value.riotRisk = 1f;
			}
			else
			{
				p.Value.riotRisk -= chanceDec / GameManager.instance.temporality.nbMicroCyclePerCycle;
				if(p.Value.riotRisk < 0f) p.Value.riotRisk = 0f;
			}

			if(Random.Range(0f, 1f) < p.Value.riotRisk)
			{
				RampOccupation(p.Key);
				p.Value.riotRisk = 0f;
			}
		}
    }

	void RampOccupation(Population pop)
	{
		Block[] targets;
		// Get Target of the wanted population type
		targets = GetTargets(pop);
 
		// If there is an occupation of my type
		if(targets.Length > 0)
		{
			targets[Random.Range(0, targets.Length)].AddState(BlockState.OnRiot);
		}
		else
		{
			targets = GetTargets();
			if(targets.Length > 0)
			{
				targets[Random.Range(0, targets.Length)].AddState(BlockState.OnRiot);
			}
			else
			{
				GameManager.instance.systemManager.AllBlocks[Random.Range(0, GameManager.instance.systemManager.AllBlocks.Count)].AddState(BlockState.OnRiot);
			}
		}
	}

	Block[] GetTargets(Population pop = null)
	{
		// Initialize an empty block list of potential target
		List<Block> targets = new List<Block>();

		// Check all Occupators in the city
		foreach(Occupator occupator in GameManager.instance.systemManager.AllOccupators)
		{
			// If the Occupator is designed for {pop} and isn't being Ramped right now
			if(IsForMe(pop, occupator.acceptedPopulation) && !occupator.block.states.Contains(BlockState.OnRiot))
			{
				// Add the block as a potentialTarget
				targets.Add(occupator.block);
			}
		}
		return targets.ToArray();
	}

	bool IsForMe(Population me, Population[] acceptedPopulations)
	{
		foreach(Population p in acceptedPopulations)
		{
			if(me == p || me == null) return true;
		}
		return false;
	}
}
