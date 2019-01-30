using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationReaction : MonoBehaviour 
{
    [System.Serializable]
    public class MoodEvent
    {
        public string function;
        public float moodValue;
        [HideInInspector] public bool occured = false;
    }

    [System.Serializable]
    public class Reaction
    {
        public Population type;
        public MoodEvent[] events;
    }

    public Reaction[] reactions; //Liste de chaques type de population

	PopulationManager popManager;

	[Header("Settings")]
	public float angryCap = 0.2f;
	public float chanceIncrementation = 0.02f;

	Population angryPopulation;
	float riotChance = 0f;

	void Start () 
	{
		popManager = GameManager.instance.populationManager;

        EventManager.StartListening ("OnNewCycle", OnNewCycle);
        EventManager.StartListening ("OnNewMicrocycle", OnNewCycle);
	}

	void Update()
	{
		UpdateReactions();
	}

	void UpdateReactions()
    {
        foreach(Reaction r in reactions)
        {
            float averageMood = popManager.GetAverageMood(r.type);
            foreach(MoodEvent me in r.events)
            {
                if(averageMood <= me.moodValue)
                {
                    if(!me.occured)
                    {
                        me.occured = true;
                        Invoke(me.function, 0f);
                    }
                }
                else
                {
                    if(me.occured)
                    {
                        me.occured = false;
                    }
                }

				if(averageMood < angryCap)
				{
					angryPopulation = r.type;
				}
            }
        }
    }
	
    public void OnNewCycle()
    {
		
    }

    public void OnNewMicrocycle()
    {
		if(Random.Range(0f, 1f) < riotChance)
		{
			RampOccupation(angryPopulation);
			riotChance = 0f;
		}
		else
		{
			riotChance += 0.02f;
		}
    }

	void RampOccupation(Population pop)
	{
		// Initialize an empty block list of potential target
		List<Block> potentialTarget = new List<Block>();

		// Check all Occupators in the city
		foreach(Occupator occupator in GameManager.instance.systemManager.AllOccupators)
		{
			// If the Occupator is designed for {pop} and isn't being Ramped right now
			if(IsForMe(pop, occupator.acceptedPopulation) && !occupator.block.states.Contains(BlockState.OnRiot))
			{
				// Add the block as a potentialTarget
				potentialTarget.Add(occupator.block);
			}
		}
		// Rand between all the potentialTarget
		potentialTarget[Random.Range(0, potentialTarget.Count)].AddState(BlockState.OnRiot);
	}

	bool IsForMe(Population me, Population[] acceptedPopulations)
	{
		foreach(Population p in acceptedPopulations)
		{
			if(me == p) return true;
		}
		return false;
	}

	public bool CheckLoose()
	{
		int angryCount = 0;
		foreach(Population pop in popManager.populationTypeList)
		{
			if(popManager.GetAverageMood(pop) <= angryCap)
			{
				angryCount++;
			}
		}
		
		if(angryCount >= 2) { return true; }
		else { return false; }
	}
}
