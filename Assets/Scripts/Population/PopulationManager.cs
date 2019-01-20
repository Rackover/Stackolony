﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PopulationManager : MonoBehaviour {

    public Population[] populationTypeList; //Liste de chaques type de population
    public List<Citizen> citizenList = new List<Citizen>(); //Liste de chaque citoyen de la colonie

    public Dictionary<Population, List<Citizen>> populationCitizenList = new Dictionary<Population, List<Citizen>>(); //Assign every citizen to it's population
    public Dictionary<Population, float> averageMoods = new Dictionary<Population, float>();  // average moods between 0 and 1


    public float moodModifierIfNoHabitation = -20f;

    [System.Serializable]
    public class Citizen
    {
        public string name;
        public Population type;
        public House habitation;
        public bool jobless = true;
    }
    
    void Start()
    {
        foreach(Population pop in populationTypeList) {
            //Temporary - Later should be initialized at 1f;
            //averageMoods[pop] = Random.value;
            averageMoods[pop] = 50f;
            populationCitizenList[pop] = new List<Citizen>();
        }
    }

    public void ChangePopulationMood(Population type, float amount)
    {
        averageMoods[type] += amount;
        Logger.Debug("Population " + type.codeName + " mood has been changed by " + amount);
    }

    //Generates a new citizen on the colony, shouldn't be called directly, use the other function with the amount parameter
    Citizen AddCitizen(Population type)
    {
        Citizen newCitizen = new Citizen();

        newCitizen.name = ""; //No name right now
        newCitizen.habitation = null;
        newCitizen.type = type;
        citizenList.Add(newCitizen);
        populationCitizenList[type].Add(newCitizen);
        return newCitizen;
    }

    //Generates new citizens on the colony
    public List<Citizen> SpawnCitizens(Population type, int amount=1)
    {
        List<Citizen> citizens = new List<Citizen>();
        for (int i = 0; i < amount; i++) {
            citizens.Add(AddCitizen(type));
        }
        Logger.Debug("Spawned " + amount + " citizens of type " + type.codeName + " to the citizen list");
        GameManager.instance.systemManager.OnNewMicrocycle();
        return citizens;
    }

    //Generates a new citizen on the colony
    public Citizen SpawnCitizen(Population type)
    {
        Citizen citizen = SpawnCitizens(type, 1)[0];
        return citizen;
    }

    //Kill a citizen
    public void KillCitizen(Citizen citizen)
    {
        if (citizenList.Contains(citizen))
        {
            populationCitizenList[citizen.type].Remove(citizen);
            citizenList.Remove(citizen);
            Debug.Log("Citizen " + citizen.name + " has been killed");
        }
    }
}
