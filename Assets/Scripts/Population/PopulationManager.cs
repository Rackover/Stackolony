﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulationManager : MonoBehaviour {

    public Population[] populationTypeList; //Liste de chaques type de population
    public List<Citizen> citizenList = new List<Citizen>(); //Liste de chaque citoyen de la colonie
    public Dictionary<Population, float> averageMoods = new Dictionary<Population, float>();  // average moods between 0 and 1

    [System.Serializable]
    public class Citizen
    {
        public string name;
        public Population type;
        public House habitation;
        public bool jobless = true;
    }

    //USED FOR DEBUG ONLY
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            AutoHouseCitizen(SpawnCitizen(populationTypeList[0]));
        }
    }

    void Start()
    {
        foreach(Population pop in populationTypeList) {
            //Temporary - Later should be initialized at 1f;
            averageMoods[pop] = Random.value;
        }
    }

    //Generates a new citizen on the colony
    public Citizen SpawnCitizen(Population type)
    {
        Citizen newCitizen = new Citizen();

        newCitizen.name = ""; //No name right now
        newCitizen.habitation = null;
        citizenList.Add(newCitizen);

        Debug.Log("Citizen " + newCitizen.name + " landed on the spatioport");
        return newCitizen;
    }

    //Kill a citizen
    public void KillCitizen(Citizen citizen)
    {
        if (citizenList.Contains(citizen))
        {
            citizenList.Remove(citizen);
            Debug.Log("Citizen " + citizen.name + " has been killed");
        }
    }

    //Assign the best house found to a citizen
    public void AutoHouseCitizen(Citizen citizen)
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
            bestHouse.FillWithCitizen(citizen);
        } else
        {
            Debug.Log("Citizen can't find a house");
        }
    }

    //Return an int, the bigger it is, the more attractive is the house
    public float GetHouseAttraction(House house, Population populationType)
    {
        float attraction = 0;
        if (house.connectedToSpatioport)
            attraction+=2;

        foreach (Population profile in house.acceptedPop)
        {
            if (profile == populationType)
            {
                attraction+=4;
            }
        }

        if (house.powered)
            attraction+=3;

        bool foodLeft = false;
        foreach (FoodProvider distributor in house.foodProvidersInRange)
        {
            if (distributor.foodLeft >= house.foodConsumptionPerHabitant)
            {
                foodLeft = true;
            }
        }
        if (foodLeft)
            attraction+=2;

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
            attraction+=2;

        attraction -= house.distanceToGround * 0.2f;
        return attraction;
    }
}