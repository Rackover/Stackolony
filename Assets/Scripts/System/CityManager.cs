using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NotationModifier
{
    public float amount;
    public int cyclesRemaining;
}

public class ConsumptionModifier
{
    public int amount;
    public int cyclesRemaining;
}

public class FlagModifier
{
    public string flagInformations;
    public int cyclesRemaining;
}

public class TempFlag
{
    public string flagInformations;
    public int cyclesRemaining;
    public System.Type flagType;
}

public class TempFlagDestroyer
{
    public System.Type flagType;
    public int cyclesRemaining;
    public string flagInformations;
}

public class FireRiskModifier
{
    public int amountInPercent;
    public int cyclesRemaining;
}

public class CityManager : MonoBehaviour {

    public enum BuildingType { Habitation = 0, Services = 1, Occupators = 2 };
    public string cityName = "Valenciennes";
    public readonly State[] accidentStates = { State.OnFire, State.OnRiot, State.Damaged };
    public Dictionary<Population, Dictionary<House, float>> topHabitations = new Dictionary<Population, Dictionary<House, float>>(); // List of the best habitations (sorted from best to worst)
    public bool isTutorialRun = true;
    public ConditionalUnlocks conditionalUnlocker = new ConditionalUnlocks();

    List<int> lockedBuildings = new List<int >();

    [Header("Mines and Nests")]
	public int minesAtStart = 3;
	public int nestsAtStart = 4;
	public float nestSpawnChance = 0.1f;
    
    [System.Serializable]
    public class MoodValues
    {
        public int goodNotationTreshold = 0; //Above this note, house is considered "Good"
        public int badNotationTreshold = -5; //Under this note, house is considered "Bad"
        public int wrongPopulationType = -2;
        public int noFood = -3;
        public int noOccupations = -2;
        public int noPower = -2;
        public int damaged = -2; //NOT TAKEN IN ACCOUNT YETTTTTTTTTTTTTTTTTTT
        public int everythingFine = +3;
        public int noHouse = -20;
    }

    public MoodValues moodValues;


    private void Start()
    {
        // Load building unlocks
        conditionalUnlocker.LoadConditionalUnlocks();

        foreach (Population pop in GameManager.instance.populationManager.populationTypeList) {
            topHabitations[pop] = new Dictionary<House, float>();
        }
    }
    
	public void GenerateEnvironmentBlocks()
	{
		for( int i = 0; i < minesAtStart; i++){SpawnMine();}
		for( int i = 0; i < nestsAtStart; i++){SpawnNest();}
	}

	public void SpawnMine()
	{
		GameManager.instance.gridManagement.LayBlock(28, GameManager.instance.gridManagement.GetRandomCoordinates());
	}

	public void SpawnNest()
	{
		GameManager.instance.gridManagement.LayBlock(29, GameManager.instance.gridManagement.GetRandomCoordinates());
	}

	public void OnNewCycle()
	{
        if(Random.Range(0f, 1f) < nestSpawnChance)
		{
			SpawnNest();
		}
	}

    public void LockBuilding(int id)
    {
        UnlockBuilding(id);
        lockedBuildings.Add(id);
    }

    public void UnlockBuilding(int id)
    {
        lockedBuildings.RemoveAll(i => i==id);
    }

    public bool IsLocked(int id)
    {
        return lockedBuildings.Contains(id);
    }

    //Finds a house for every citizens (Soon it'll take a priority order into account)
    public void HouseEveryone(float x)
    {
        ResetHousesHabitants();
        GetBestHouses();
        for (int i = 0; i < GameManager.instance.populationManager.populationTypeList.Length; i++)
        {
            HousePopulation(GameManager.instance.populationManager.populationTypeList[i], x);
        }
    }

    private void ResetHousesHabitants()
    {
        foreach (House house in GameManager.instance.systemManager.AllHouses)
        {
            house.affectedCitizen.Clear();
        }
    }

    //Generates a fireRiskModifier for a gien block
    public void GenerateFireRiskModifier(Block block, int amountInPercent, int cyclesRemaining)
    {
        FireRiskModifier newFireRiskModifier = new FireRiskModifier();
        newFireRiskModifier.amountInPercent = amountInPercent;
        newFireRiskModifier.cyclesRemaining = cyclesRemaining;
        block.fireRiskModifiers.Add(newFireRiskModifier);
    }

    //Generates a notationModifier for a given house
    public void GenerateNotationModifier(House house, float newAmount, int cyclesRemaining)
    {
        NotationModifier newNotationModifier = new NotationModifier();
        newNotationModifier.amount = newAmount;
        newNotationModifier.cyclesRemaining = cyclesRemaining;
        house.notationModifiers.Add(newNotationModifier);
    }

    public void GenerateConsumptionModifier(Block block, int newAmount, int cyclesRemaining)
    {
        ConsumptionModifier newConsumptionModifier = new ConsumptionModifier();
        newConsumptionModifier.amount = newAmount;
        newConsumptionModifier.cyclesRemaining = cyclesRemaining;
        block.consumptionModifiers.Add(newConsumptionModifier);
    }

    public void GenerateFlagModifier(Block block, string flagInformations, int cyclesRemaining)
    {
        string[] flagElements = flagInformations.Split(new char[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
        bool flagFound = false;
        foreach (Flag.IFlag flags in block.activeFlags)
        {
            if (flags.GetFlagType() == System.Type.GetType(flagElements[0])) {
                flagFound = true;
            }
        }
        if (!flagFound)
        {
            return;
        }
        FlagModifier newFlagModifier = new FlagModifier();
        newFlagModifier.cyclesRemaining = cyclesRemaining;
        newFlagModifier.flagInformations = flagInformations;
        //Apply the flag modification
        GameManager.instance.flagReader.ReadFlag(block, flagInformations);
        block.flagModifiers.Add(newFlagModifier);
    }

    public void GenerateTempFlag(Block block, string flagInformations, int cyclesRemaining)
    {
        string[] flagElements = flagInformations.Split(new char[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (Flag.IFlag flags in block.activeFlags)
        {
            if (flags.GetFlagType() == System.Type.GetType(flagElements[0]))
            {
                return;
            }
        }
        TempFlag newTempFlag = new TempFlag();
        newTempFlag.cyclesRemaining = cyclesRemaining;
        newTempFlag.flagInformations = flagInformations;
        newTempFlag.flagType = System.Type.GetType(flagElements[0]);
        //Generates the flag
        GameManager.instance.flagReader.ReadFlag(block, flagInformations);
        block.tempFlags.Add(newTempFlag);
    }

    public void GenerateTempFlagDestroyer(Block block, System.Type flag, int cyclesRemaining)
    {
        TempFlagDestroyer newTempFlagDestroyer = new TempFlagDestroyer();
        newTempFlagDestroyer.cyclesRemaining = cyclesRemaining;
        newTempFlagDestroyer.flagType = flag;
        newTempFlagDestroyer.flagInformations = FindFlag(block, flag).GetFlagDatas(); //Save the flag informations
        Destroy(block.GetComponent(flag)); //Destroys the flag
        block.tempFlagDestroyers.Add(newTempFlagDestroyer);
    }

    //Finds a house for every citizens from a defined population
    public void HousePopulation(Population pop, float x)
    {
        foreach (PopulationManager.Citizen citizen in GameManager.instance.populationManager.populations[pop].citizens)
        {
            if (topHabitations[pop].Count > 0)
            {
                House foundHouse = topHabitations[pop].First().Key;
                foundHouse.FillWithCitizen(citizen);
                Logger.Debug("Citizen " + citizen.name + " of type " + citizen.type.codeName + " has been housed at the house " + foundHouse);
                //Applique le changement d'humeur au type de population
                GameManager.instance.populationManager.ChangePopulationMood(pop, topHabitations[pop].First().Value * x);

                //Si la maison est désormais remplie, on la retire de la liste des habitations pour chaque population
                if (foundHouse.affectedCitizen.Count >= foundHouse.slotAmount)
                {
                    foreach (Population popType in GameManager.instance.populationManager.populationTypeList)
                    {
                        topHabitations[popType].Remove(foundHouse);
                    }
                }
            } else
            {
                Logger.Debug("Citizen " + citizen.name + " of type " + citizen.type.codeName + " could not find a house");
                citizen.habitation = null;
                //Si le citoyen n'a pas pu se loger, il applique le malus d'humeur à son type de population
                GameManager.instance.populationManager.ChangePopulationMood(pop, moodValues.noHouse * x);
            }
        }
    }

    //Get the best houses for each category, and updates the dictionary "topHabitation"
    public void GetBestHouses()
    {
        SystemManager systemManager = GameManager.instance.systemManager;
        foreach (Population pop in GameManager.instance.populationManager.populationTypeList)
        {
            //Creates a dictionary assigning each house to it's attraction note
            Dictionary<House, float> habitationNote = new Dictionary<House, float>(); // Attribute a note to every habitation
            foreach (House house in systemManager.AllHouses)
            {
                float houseAttraction = GetHouseNotation(house, pop);
                if (houseAttraction > GameManager.instance.populationManager.moodModifierIfNoHabitation)
                habitationNote[house] = houseAttraction;
            }

            //Convert the dictionary to a sorted list
            Dictionary<House, float> sortedHabitations = new Dictionary<House, float>();
            foreach (KeyValuePair<House, float> notedHabitation in habitationNote.OrderByDescending(key => key.Value))
            {
                sortedHabitations[notedHabitation.Key] = notedHabitation.Value;
            }
            topHabitations[pop] = sortedHabitations;
        }
    }

    public Flag.IFlag FindFlag(Block block, System.Type type)
    {
            for (int i = 0; i < block.activeFlags.Count; i++)
            {
                if (block.activeFlags[i].GetFlagType() == type)
                {
                    return block.activeFlags[i];
                }
            }
        return null;
    }

    public Block FindRandomBlockWithFlag(System.Type type)
    {
        List<Block> candidates = new List<Block>();
        foreach (Block block in GameManager.instance.systemManager.AllBlocks)
        {
            for (int i = 0; i < block.activeFlags.Count; i++)
            {
                if (block.activeFlags[i].GetFlagType() == type)
                {
                    candidates.Add(block);
                }
            }
        }
        int random = Random.Range(0, candidates.Count - 1);
        Block result = candidates[random];
        if (result != null)
        {
            return result;
        }
        return null;
    }


    //Return an int, the bigger it is, the more attractive is the house
    public float GetHouseNotation(House house, Population populationType)
    {
        house.UpdateHouseInformations();
        float notation = 0;

        //If house isn't connected to spatioport, it sucks
        if (!house.block.isLinkedToSpatioport)
        {
            return GameManager.instance.populationManager.moodModifierIfNoHabitation;
        }

        //If house is already full, it also sucks
        if (house.affectedCitizen.Count >= house.slotAmount)
        {
            return GameManager.instance.populationManager.moodModifierIfNoHabitation;
        }


        bool profileFound = false;
        bool powered = false;
        bool foodLeft = false;
        bool jobLeft = false;
        bool damaged = false;

        foreach (Population profile in house.acceptedPop)
        {
            if (profile == populationType)
            {
                profileFound = true;
                break;
            }
        }

        if (house.powered)
        {
            powered = true;
        }

        float foodStock = 0;
        foreach (FoodProvider distributor in house.foodProvidersInRange)
        {
            foodStock += distributor.foodLeft;
        }
        if (foodStock > 0 && foodStock >= GameManager.instance.populationManager.GetFoodConsumption(populationType))
        {
            foodLeft = true;
        } else
        {
            foodLeft = false;
        }



        foreach (Occupator occupator in house.occupatorsInRange)
        {
            foreach (Population pop in occupator.acceptedPopulation)
            {
                if (pop == populationType)
                {
                    jobLeft = true;
                    break;
                }
            }
        }

        foreach (KeyValuePair<State,StateBehavior> state in house.block.states)
        {
            if(state.Key == State.Damaged)
            {
                damaged = true;
                break;
            }
        }

        if(!profileFound)
        {
            notation += moodValues.wrongPopulationType;
        }
        if (!powered)
        {
            notation += moodValues.noPower;
        }
        if (!foodLeft)
        {
            notation += moodValues.noFood;
        }
        if (!jobLeft)
        {
            notation += moodValues.noOccupations;
        }
        if (damaged)
        {
            notation += moodValues.damaged;
        }

        if (notation >= 0)
            notation += moodValues.everythingFine;

        notation -= house.nuisanceImpact;
        foreach (NotationModifier notationModifier in house.notationModifiers)
        {
            notation += notationModifier.amount;
        }
        return notation;
    }
    
    public void TriggerAccident(State accident)
    {
        if(GameManager.instance.systemManager.AllBlocks.Count == 0) return;

        if( IsConsideredAccident(accident) )
        {
            int rand = Random.Range(0, GameManager.instance.systemManager.AllBlocks.Count);
            int blockMet = 0;
            while( GameManager.instance.systemManager.AllBlocks[rand].states.ContainsKey( accident ) || GameManager.instance.systemManager.AllBlocks[rand].scheme.riotProof )
            {
                if(blockMet++ > GameManager.instance.systemManager.AllBlocks.Count) 
                {
                    Logger.Debug("All blocks already have " + accident + " as a state");
                    return;
                }
                rand = Random.Range(0, GameManager.instance.systemManager.AllBlocks.Count);
            }
            GameManager.instance.systemManager.AllBlocks[rand].AddState(accident);
        }
        else Logger.Debug( accident + " is not considered as a accident" );
    }

    bool IsConsideredAccident(State state)
    {
        foreach(State s in accidentStates)
        {
            if(state == s)
            {
                return true;
            }
        }
        return false;
    }
}