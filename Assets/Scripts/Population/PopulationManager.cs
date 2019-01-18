using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulationManager : MonoBehaviour {

    public Population[] populationTypeList; //Liste de chaques type de population
    public List<Citizen> citizenList = new List<Citizen>(); //Liste de chaque citoyen de la colonie
    public Dictionary<Population, float> averageMoods = new Dictionary<Population, float>();  // average moods between 0 and 1

    [Header("Mood modifiers")]
    public float MMglobalModifier = 1; //Coefficient par lequel est multiplié le moodmodifier quand il est appliqué
    public int MMnoHabitation = -4;
    public int MMnoAppropriatedHabitation = -2;
    public int MMnoFood = -3;
    public int MMnotEnoughFood = -1;
    public int MMnoOccupation = -2;
    public int MMnoPower = -2;
    public int MMdamagedHabitation = -2;
    public int MMeverythingFine = +3;

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
        }
    }

    //Calculate the mood of every popluation
    public void CalculateMoods()
    {
        foreach (Population pop in populationTypeList)
        {
            CalculateMood(pop);
        }
    }

    //Calculate the mood of a population type
    public void CalculateMood(Population pop) 
    {
        float moodModifier = 0; //How much "mood points" the population must loose / gain
        foreach (Citizen citizen in citizenList)
        {
            int citizenMoodModifier = 0;
            if (citizen.type = pop)
            {
                if (citizen.habitation = null)
                {
                    citizenMoodModifier += MMnoHabitation;
                } else
                {
                    bool houseSupportType = false;
                    foreach (Population type in citizen.habitation.acceptedPop)
                    {
                        if (citizen.type == type)
                        {
                            houseSupportType = true;
                            break;
                        }
                    }
                    if (!houseSupportType) citizenMoodModifier += MMnoAppropriatedHabitation;
                    if (citizen.habitation.foodReceived <= 0)
                    {
                        citizenMoodModifier += MMnoFood;
                    } else if (citizen.habitation.foodReceived < citizen.habitation.foodConsumption)
                    {
                        citizenMoodModifier += MMnotEnoughFood;
                    }
                    if (citizen.jobless) citizenMoodModifier += MMnoOccupation;
                    if (citizen.habitation.block.currentPower < citizen.habitation.block.scheme.consumption) citizenMoodModifier += MMnoPower;
                    foreach (BlockState state in citizen.habitation.block.states)
                    {
                        if (state == BlockState.Damaged || state == BlockState.OnFire)
                        {
                            citizenMoodModifier += MMdamagedHabitation;
                        }
                    }
                }
                if (citizenMoodModifier == 0) citizenMoodModifier += MMeverythingFine;
                moodModifier += citizenMoodModifier;
            }
        }
        moodModifier *= MMglobalModifier;
        averageMoods[pop] += moodModifier;
        Logger.Debug("Population of type " + pop.codeName + " has been modified by " + moodModifier + " and is now at " + averageMoods[pop]);
    }

    //Generates a new citizen on the colony, shouldn't be called directly, use the other function with the amount parameter
    Citizen AddCitizen(Population type)
    {
        Citizen newCitizen = new Citizen();

        newCitizen.name = ""; //No name right now
        newCitizen.habitation = null;
        newCitizen.type = type;
        citizenList.Add(newCitizen);
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
            citizenList.Remove(citizen);
            Debug.Log("Citizen " + citizen.name + " has been killed");
        }
    }
}
