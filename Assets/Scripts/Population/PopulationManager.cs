using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : MonoBehaviour {

    public Population[] populationTypeList; //Liste de chaques type de population
    public List<Citizen> citizenList = new List<Citizen>(); //Liste de chaque citoyen de la colonie

    public Dictionary<Population, List<Citizen>> populationCitizenList = new Dictionary<Population, List<Citizen>>(); //Assign every citizen to it's population
    private Dictionary<Population, float> averageMoods = new Dictionary<Population, float>();  // average moods between 0 and 1
    public Dictionary<Population, List<MoodModifier>> moodModifiers = new Dictionary<Population, List<MoodModifier>>(); //List of every active moodmodifiers for every population

    public float moodModifierIfNoHabitation = -20f;

    [System.Serializable]
    public class Citizen
    {
        public string name;
        public Population type;
        public House habitation;
        public bool jobless = true;
    }

    public class MoodModifier
    {
        public int reasonId;
        public float amount;
        public int cyclesRemaining;
    }
    
    void Start()
    {
        foreach(Population pop in populationTypeList) {
            averageMoods[pop] = 50f;
            populationCitizenList[pop] = new List<Citizen>();
            moodModifiers[pop] = new List<MoodModifier>();
        }
    }

    public Population GetPopulationByID(int id)
    {
        foreach (Population type in populationTypeList)
        {
            if (type.ID == id)
            {
                return type;
            }
        }
        Debug.LogWarning("System searched for population with ID " + id + " and couldn't find it");
        Logger.Warn("System searched for population with ID " + id + " and couldn't find it");
        return populationTypeList[0];
    }

    public Population GetPopulationByName(string name)
    {
        foreach (Population type in populationTypeList)
        {
            if (type.codeName == name)
            {
                return type;
            }
        }
        Debug.LogWarning("System searched for population with name " + name + " and couldn't find it");
        Logger.Warn("System searched for population with name " + name + " and couldn't find it");
        return populationTypeList[0];
    }

    //Generates a moodmodifier for a given population
    public void GenerateMoodModifier(Population popType, int reasonId, float amount, int cyclesRemaining)
    {
        MoodModifier newMoodModifier = new MoodModifier();
        newMoodModifier.reasonId = reasonId;
        newMoodModifier.amount = amount;
        newMoodModifier.cyclesRemaining = cyclesRemaining;
        moodModifiers[popType].Add(newMoodModifier);
    }

    //This function just changes the index of the populations in the array containing every populations
    public void ChangePopulationPriority(Population type, int priority) //priority 0 means first
    {
        if (priority >= populationTypeList.Length)
            priority = populationTypeList.Length - 1;
        for (int i = 0; i < populationTypeList.Length; i++) {
            if (populationTypeList[i] == type)
            {
                //Switch the found population object with the one at the wanted index
                Population destinationPop = populationTypeList[priority];
                populationTypeList[priority] = type;
                populationTypeList[i] = destinationPop;
            }
        }
    }


    public void ChangePopulationMood(Population type, float amount)
    {
        float oldValue = averageMoods[type];
        averageMoods[type] += amount;
        float newValue = averageMoods[type];
        Logger.Debug("Population " + type.codeName + " mood has been changed from " + oldValue + " to " + newValue);
    }

    public float GetAverageMood(Population type)
    {
        float averageMood = GetRawAverageMood(type);
        foreach (MoodModifier moodModifier in moodModifiers[type])
        {
            averageMood += moodModifier.amount;
        }
        return averageMood;
    }

    public float GetRawAverageMood(Population type)
    {
        return averageMoods[type];
    }

    public void ApplyMoodModifiers()
    {
        foreach (KeyValuePair<Population, List<MoodModifier>> moodModifiers in moodModifiers)
        {
            foreach (MoodModifier moodModifier in moodModifiers.Value)
            {
                averageMoods[moodModifiers.Key] += moodModifier.amount;
            }
        }
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
