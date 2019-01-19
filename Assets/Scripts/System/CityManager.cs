using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityManager : MonoBehaviour {

    public string cityName = "Valenciennes";
    public BlockState[] accidentStates = { BlockState.OnFire, BlockState.OnRiot, BlockState.Damaged };


    //Assign the best house found to a citizen
    public void AutoHouseCitizen(PopulationManager.Citizen citizen)
    {
        SystemManager systemManager = GameManager.instance.systemManager;
        float attraction = -1;
        House bestHouse = null;
        foreach (House house in systemManager.AllHouses)
        {
            if (house.citizenCount < house.slotAmount)
            {
                float houseAttraction = GetHouseAttraction(house, citizen.type);
                if (houseAttraction > attraction)
                {
                    attraction = houseAttraction;
                    bestHouse = house;
                }
            }
        }
        if (bestHouse != null)
        {
            if (citizen.habitation != null)
            {
                citizen.habitation.affectedCitizen.Remove(citizen);
            }
            bestHouse.FillWithCitizen(citizen);
        }
        else
        {
            Debug.Log("Citizen can't find a house");
        }
    }

    //Return an int, the bigger it is, the more attractive is the house
    public float GetHouseAttraction(House house, Population populationType)
    {
        float attraction = 0;

        foreach (Population profile in house.acceptedPop)
        {
            if (profile == populationType)
            {
                attraction += 4;
            }
        }

        if (house.powered)
            attraction += 3;

        bool foodLeft = false;
        foreach (FoodProvider distributor in house.foodProvidersInRange)
        {
            if (distributor.foodLeft >= house.foodConsumptionPerHabitant)
            {
                foodLeft = true;
            }
        }
        if (foodLeft)
            attraction += 2;

        bool jobLeft = false;
        foreach (Occupator occupator in house.occupatorsInRange)
        {
            foreach (Population pop in occupator.acceptedPopulation)
            {
                if (pop == populationType)
                {
                    jobLeft = true;
                }
            }
        }
        if (jobLeft)
            attraction += 2;

        attraction -= house.distanceToGround * 0.2f;
        return attraction;
    }
    
    public void TriggerAccident(BlockState accident)
    {
        if(GameManager.instance.systemManager.AllBlocks.Count == 0) return;

        if( IsConsideredAccident( accident ) )
        {
            int rand = Random.Range(0, GameManager.instance.systemManager.AllBlocks.Count);
            int blockMet = 0;
            while( GameManager.instance.systemManager.AllBlocks[rand].states.Contains( accident ))
            {
                if(blockMet++ > GameManager.instance.systemManager.AllBlocks.Count) 
                {
                    Logger.Debug("All blocks already have " + accident + " as a state");
                    return;
                }
                rand = Random.Range(0, GameManager.instance.systemManager.AllBlocks.Count);
            }
            GameManager.instance.systemManager.AllBlocks[rand].AddState( accident );
        }
        else
        {
            Debug.Log( accident + " is not considered as a accident" );
        }
    }

    bool IsConsideredAccident(BlockState state)
    {
        foreach(BlockState s in accidentStates)
        {
            if(state == s)
            {
                return true;
            }
        }
        return false;
    }
}
