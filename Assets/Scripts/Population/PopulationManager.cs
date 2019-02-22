using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MoodModifier
{
    public int eventId;
    public float amount;
    public int cyclesRemaining;
}

public class FoodModifier
{
    public float amount;
    public int cyclesRemaining;
}

public class PopulationManager : MonoBehaviour 
{
    [System.Serializable]
    public class Citizen
    {
        public string name;
        public Population type;
        public House habitation;
        public bool jobless = true;
    }

    public class MoodEvolution
    {
        Dictionary<CityManager.MoodAffect, float> evolutions = new Dictionary<CityManager.MoodAffect, float>();

        public MoodEvolution()
        {
            Clear();
        }

        public void Clear()
        {
            evolutions = new CityManager.MoodValues().values;
            foreach (CityManager.MoodAffect valueType in new CityManager.MoodValues().values.Keys) {
                evolutions[valueType] = 0f;
            }
        }

        public void Add(CityManager.MoodAffect type, float value)
        {
            evolutions[type] += value;
        }

        public float Get(CityManager.MoodAffect type)
        {
            return evolutions[type];
        }

        public float ToFloat()
        {
            float value = 0f;
            foreach (CityManager.MoodAffect valueType in evolutions.Keys) {
                value += evolutions[valueType];
            }
            return value;
        }

        public Dictionary<CityManager.MoodAffect, float>.KeyCollection GetKeys()
        {
            return evolutions.Keys;
        }
    }

    public class PopulationInformation
    {
        public float averageMood; // average moods between 0 and 1
        public float riotRisk;
        public MoodEvolution lastMoodChange = new MoodEvolution();

        public List<Citizen> citizens = new List<Citizen>(); //Assign every citizen to it's population
        public List<MoodModifier> moodModifiers = new List<MoodModifier>(); //List of every active moodmodifiers for every population
        public List<FoodModifier> foodModifiers = new List<FoodModifier>(); //List of every food modifier affecting every population
    }

    [Header("System")]
    public Population[] populationTypeList; //Liste de chaques type de population
    public List<Citizen> citizenList = new List<Citizen>(); //Liste de chaque citoyen de la colonie
    public Dictionary<Population, PopulationInformation> populations = new Dictionary<Population, PopulationInformation>();

    [Header("Mood settings")]
    public float startingMood = 50f;
    public float maxMood = 100f;
    public float moodModifierIfNoHabitation = -20f;
    public int maxCitizenNameLength = 20;

    [Header("Reaction settings")]
	public float angryCap = 0.2f;
	public float chanceInc = 0.02f;
	public float chanceDec = 0.02f;


    public System.Action<int, Population> CitizenArrival;
    List<string> names;

    private void Awake()
    {
        LoadNames();
    }

    void Start()
    {
        Clear();
    }

    public void OnNewCycle()
    {

    }

    public void OnNewMicrocycle()
    {
		foreach (KeyValuePair<Population, PopulationInformation> p in populations)
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
		    targets[Random.Range(0, targets.Length)].AddState(State.OnRiot);
		}
		else
		{
			targets = GetTargets();
			if(targets.Length > 0)
			{
				targets[Random.Range(0, targets.Length)].AddState(State.OnRiot);
			}
			else
			{
				GameManager.instance.cityManager.TriggerAccident(State.OnRiot);
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
			if(IsForMe(pop, occupator.acceptedPopulation) && !occupator.block.states.ContainsKey(State.OnRiot))
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

    void LoadNames()
    {
        // Default name
        names = new List<string>() { "Citizen" };

        // Loading names from file
        List<string> newNames;
        try {
            newNames = new List<string>(File.ReadAllLines(Paths.GetNamesFile())) ;
        }
        catch (FileNotFoundException) {
            Logger.Error("Could not find name file - this should not happen. Defaulting to 'Citizen' name.");
            return;
        }

        // Adding only names that aren't too long
        foreach(string name in newNames) {
            string newName = name;
            if (name.Length > maxCitizenNameLength) {
                newName = newName.Remove(maxCitizenNameLength);
            }
            names.Add(newName);
        }
    }

    public string GetRandomName()
    {
        return names[Mathf.FloorToInt(Random.value * names.Count)];
    }

    public Population GetRandomPopulation()
    {
        return populationTypeList[Mathf.FloorToInt(Random.value * populationTypeList.Length)];
    }

    //Return the food consumed by a type of population
    public float GetFoodConsumption(Population popType)
    {
        float foodConsumption = popType.baseFoodConsumption;
        foreach (FoodModifier foodModifier in populations[popType].foodModifiers)
        {
            foodConsumption += foodModifier.amount;
        }
        return foodConsumption;
    }

    public Population GetPopulationByID(int ID)
    {
        foreach (Population pop in populationTypeList)
        {
            if (pop.ID == ID)
            {
                return pop;
            }
        }
        return null;
    }
    //Generates a foodmodifier for a given population
    public void GenerateFoodModifier(Population popType, float newAmount, int cyclesRemaining)
    {
        FoodModifier newFoodModifier = new FoodModifier();
        newFoodModifier.amount = newAmount;
        newFoodModifier.cyclesRemaining = cyclesRemaining;
        populations[popType].foodModifiers.Add(newFoodModifier);
    }

    //Generates a moodmodifier for a given population
    public void GenerateMoodModifier(Population popType, float amount, int cyclesRemaining, int eventId=0)
    {
        MoodModifier newMoodModifier = new MoodModifier();
        newMoodModifier.amount = amount;
        newMoodModifier.cyclesRemaining = cyclesRemaining;
        newMoodModifier.eventId = eventId;
        populations[popType].moodModifiers.Add(newMoodModifier);
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

    public void ChangePopulationMood(Population type, MoodEvolution evolution)
    {
        foreach (CityManager.MoodAffect affect in evolution.GetKeys()) {
            ChangePopulationMood(type, evolution.Get(affect), affect);
        }
    }

    public void ChangePopulationMood(Population type, float amount, CityManager.MoodAffect reason=CityManager.MoodAffect.none)
    {
        float oldValue = populations[type].averageMood;
        populations[type].averageMood += amount/populations[type].citizens.Count;

        // Clamping mood
        if(populations[type].averageMood < 0) populations[type].averageMood = 0f;
        if(populations[type].averageMood > maxMood) populations[type].averageMood = 100f;

        float newValue = populations[type].averageMood;

        if (reason != CityManager.MoodAffect.none) {
            populations[type].lastMoodChange.Add(reason, oldValue - newValue);
        }

        Logger.Debug("Population " + type.codeName + " mood has been changed from " + oldValue + " to " + newValue);
    }

    public float GetAverageMood(Population type)
    {
        float averageMood = GetRawAverageMood(type);
        foreach (MoodModifier moodModifier in populations[type].moodModifiers)
        {
            averageMood += moodModifier.amount;
        }
        return averageMood;
    }

    public float GetRawAverageMood(Population type)
    {
        return populations[type].averageMood;
    }

    public void ApplyMoodModifiers()
    {
        foreach (KeyValuePair<Population, PopulationInformation> pc in populations)
        {
            foreach (MoodModifier moodModifier in pc.Value.moodModifiers)
            {
                pc.Value.averageMood += moodModifier.amount;
            }
        }
    }

    public int GetHomelessCount(Population population)
    {
        int count = 0;
        foreach(Citizen citizen in populations[population].citizens) {
            count += citizen.habitation == null ? 1 : 0;
        }
        return count;
    }

    public int GetHomelessCount()
    {
        int count = 0;
        foreach(Population pop in populationTypeList) {
            count += GetHomelessCount(pop);
        }
        return count;
    }

    //Generates a new citizen on the colony, shouldn't be called directly, use the other function with the amount parameter
    Citizen AddCitizen(Population type)
    {
        Citizen newCitizen = new Citizen();

        newCitizen.name = GetRandomName();
        newCitizen.habitation = null;
        newCitizen.type = type;
        citizenList.Add(newCitizen);
        populations[type].citizens.Add(newCitizen);
        
        return newCitizen;
    }

    //Generates new citizens on the colony
    public List<Citizen> SpawnCitizens(Population type, int amount=1)
    {
        List<Citizen> citizens = new List<Citizen>();
        for (int i = 0; i < amount; i++) {
            citizens.Add(AddCitizen(type));
        }
        
        GameManager.instance.soundManager.Play("NewSettler");

        Logger.Debug("Spawned " + amount + " citizens of type " + type.codeName + " to the citizen list");

        try {
            CitizenArrival.Invoke(citizens.Count, type);
        }
        catch {
            // Nothing to do, no listeners
        }
        
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
            populations[citizen.type].citizens.Remove(citizen);
            citizenList.Remove(citizen);
            Debug.Log("Citizen " + citizen.name + " has been killed");
        }
    }

    public Population GetPopulationByCodename(string codeName)
    {
        foreach(Population pop in populationTypeList) {
            if (pop.codeName == codeName) {
                return pop;
            }
        }

        return null;
    }

    public void Clear()
    {

        citizenList.Clear();
        populations.Clear();

        foreach (Population pop in populationTypeList) {
            populations.Add(pop, new PopulationInformation());

            populations[pop].averageMood = startingMood;
            populations[pop].citizens = new List<Citizen>();
            populations[pop].moodModifiers = new List<MoodModifier>();
        }

        ClearListeners();
    }

    public void ClearListeners()
    {
        try {
            foreach (System.Delegate d in CitizenArrival.GetInvocationList()) {
                CitizenArrival -= (System.Action<int, Population>)d;
            }
        }
        catch {
            // Nothing to do
        }
    }
}
