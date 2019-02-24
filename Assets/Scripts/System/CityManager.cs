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
    public enum MoodAffect { none, everythingFine, wrongPopulationType, noFood, noOccupations, noPower, damaged, noHouse, nuisance, habitationBuff };
    public string cityName = "Valenciennes";
    public readonly State[] accidentStates = { State.OnFire, State.OnRiot, State.Damaged };
    public Dictionary<Population, List<KeyValuePair<House, PopulationManager.MoodEvolution>>> topHabitations = new Dictionary<Population, List<KeyValuePair<House, PopulationManager.MoodEvolution>>>(); // List of the best habitations (sorted from best to worst)
    public bool isTutorialRun = true;
    public ConditionalUnlocks conditionalUnlocker = new ConditionalUnlocks();

    List<int> lockedBuildings = new List<int>();

    [Header("Mines and Nests")]
    public int minesAtStart = 3;
    public int nestsAtStart = 4;
    public float nestSpawnChance = 0.1f;

    [System.Serializable]
    public class MoodValues
    {
        public int goodNotationTreshold = 0; //Above this note, house is considered "Good"
        public int badNotationTreshold = -5; //Under this note, house is considered "Bad"
        public Dictionary<MoodAffect, float> values = new Dictionary<MoodAffect, float>() {
            {MoodAffect.wrongPopulationType , -2 },
            {MoodAffect.noFood , -3 },
            {MoodAffect.noOccupations , -2},
            {MoodAffect.noPower , -2},
            {MoodAffect.damaged , -2},
            {MoodAffect.everythingFine , +3},
            {MoodAffect.noHouse, -20},

            // Special, should stay 0
            {MoodAffect.none, 0 },
            {MoodAffect.nuisance, 0 },
            {MoodAffect.habitationBuff, 0 }
        };
    }

    public MoodValues moodValues;

    public BlockScheme mine;
    public BlockScheme nest;


    private void Start()
    {
        // Load building unlocks
        conditionalUnlocker.LoadConditionalUnlocks();

        foreach (Population pop in GameManager.instance.populationManager.populationTypeList) {
            topHabitations[pop] = new List<KeyValuePair<House, PopulationManager.MoodEvolution>>();
        }
    }
    
	public void GenerateEnvironmentBlocks()
	{
		for( int i = 0; i < minesAtStart; i++){SpawnEnvironmentBlock(mine.ID);}
		for( int i = 0; i < nestsAtStart; i++){SpawnEnvironmentBlock(nest.ID);}
	}

	public void SpawnEnvironmentBlock(int which)
	{
        Vector2Int pos = Vector2Int.zero; 
        for(int i = 0; i < 3; i++)
        {
            pos = GameManager.instance.gridManagement.GetRandomCoordinates();

            if(GameManager.instance.gridManagement.IsPositionFree(pos))
            {
		        GameManager.instance.gridManagement.LayBlock(which, pos);
                return;
            }
        }

        for(int i = 0; i < GameManager.instance.gridManagement.maxHeight; i++)
        {
            if(GameManager.instance.gridManagement.grid[pos.x, i, pos.y] != null)
            {
                Block block = GameManager.instance.gridManagement.grid[pos.x, i, pos.y].GetComponent<Block>();
                if(block != null)
                {
                    GameManager.instance.gridManagement.DestroyBlock(block);
                }
                else
                {
                    BridgeInfo bridge = GameManager.instance.gridManagement.grid[pos.x, i, pos.y].GetComponent<BridgeInfo>();
                    if(bridge != null)
                    {
                        GameManager.instance.gridManagement.DestroyBridge(bridge.gameObject);
                    }
                }
            }
        }
        GameManager.instance.gridManagement.LayBlock(which, pos);
	}
    
	public void OnNewCycle()
	{
        if(Random.Range(0f, 1f) < nestSpawnChance)
		{
			SpawnEnvironmentBlock(nest.ID);
		}
	}

    public List<int> GetLockedBuildings()
    {
        return lockedBuildings.ToArray().ToList();
    }

    public void ClearLocks()
    {
        lockedBuildings.Clear();
    }

    public void LockBuilding(int id)
    {
        lockedBuildings.RemoveAll(i => i == id);
        lockedBuildings.Sort();
        lockedBuildings.Add(id);
        Logger.Debug("Locked building " + id + " : current locked buildings are " + string.Join(",", lockedBuildings.Select(x => x.ToString()).ToArray()));
    }

    public void UnlockBuilding(int id)
    {
        lockedBuildings.RemoveAll(i => i==id);
        lockedBuildings.Sort();
        Logger.Debug("Unlocking building " + id+" : current locked buildings are "+string.Join(",", lockedBuildings.Select(x => x.ToString()).ToArray()));
    }

    public bool IsLocked(int id)
    {
        return lockedBuildings.Contains(id) || !conditionalUnlocker.CanBeUnlocked(id);
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
        Logger.Debug("Reset mood change for " + pop.codeName);
        GameManager.instance.populationManager.populations[pop].lastMoodChange = new PopulationManager.MoodEvolution();
        foreach (PopulationManager.Citizen citizen in GameManager.instance.populationManager.populations[pop].citizens)
        {
            if (topHabitations[pop].Count > 0)
            {
                House foundHouse = topHabitations[pop][0].Key;
                foundHouse.FillWithCitizen(citizen);
                //Applique le changement d'humeur au type de population
                GameManager.instance.populationManager.ChangePopulationMood(pop, topHabitations[pop].First().Value);

                //Si la maison est désormais remplie, on la retire de la liste des habitations pour chaque population
                if (foundHouse.affectedCitizen.Count >= foundHouse.slotAmount)
                {
                    foreach (Population popType in GameManager.instance.populationManager.populationTypeList)
                    {
                        topHabitations[popType].RemoveAll(o=> o.Key == foundHouse);
                    }
                }
            } else
            {
                Logger.Debug("Citizen " + citizen.name + " of type " + citizen.type.codeName + " could not find a house");
                citizen.habitation = null;
                //Si le citoyen n'a pas pu se loger, il applique le malus d'humeur à son type de population
                GameManager.instance.populationManager.ChangePopulationMood(pop, moodValues.values[MoodAffect.noHouse] * x, MoodAffect.noHouse);
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
            Dictionary<House, PopulationManager.MoodEvolution> habitationNote = new Dictionary<House, PopulationManager.MoodEvolution>(); // Attribute a note to every habitation
            foreach (House house in systemManager.AllHouses)
            {
                PopulationManager.MoodEvolution houseAttraction = GetHouseNotation(house, pop);
                if (houseAttraction.Get(MoodAffect.noHouse) == 0f) {
                    // House is inhabitable
                    habitationNote[house] = houseAttraction;
                }
            }

            //Convert the dictionary to a sorted list
            List<KeyValuePair<House, PopulationManager.MoodEvolution>> sortedHabitations = new List<KeyValuePair<House, PopulationManager.MoodEvolution>>();            
            foreach (KeyValuePair<House, PopulationManager.MoodEvolution> notedHabitation in habitationNote)
            {
                sortedHabitations.Add(new KeyValuePair<House, PopulationManager.MoodEvolution>(notedHabitation.Key, notedHabitation.Value));
            }
            // Sorting
            sortedHabitations = sortedHabitations.OrderBy(x => -x.Value.ToFloat()).ToList();
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
    public PopulationManager.MoodEvolution GetHouseNotation(House house, Population populationType)
    {
        house.UpdateHouseInformations();
        PopulationManager.MoodEvolution notation = new PopulationManager.MoodEvolution();

        //If house isn't connected to spatioport, it sucks
        if (!house.block.isLinkedToSpatioport){
            notation.Add(MoodAffect.noHouse, GameManager.instance.populationManager.moodModifierIfNoHabitation);
            return notation;
        }

        //If house is already full, it also sucks
        if (house.affectedCitizen.Count >= house.slotAmount) {
            notation.Add(MoodAffect.noHouse, GameManager.instance.populationManager.moodModifierIfNoHabitation);
            return notation;
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
        Logger.Debug("For house " + house.GetInstanceID() + " there are " + house.foodProvidersInRange.Count + " food providers in range");
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
            notation.Add(MoodAffect.wrongPopulationType, moodValues.values[MoodAffect.wrongPopulationType]);
        }
        if (!powered)
        {
            notation.Add(MoodAffect.noPower, moodValues.values[MoodAffect.noPower]);
        }
        if (!foodLeft)
        {
            notation.Add(MoodAffect.noFood, moodValues.values[MoodAffect.noFood]);
        }
        if (!jobLeft)
        {
            notation.Add(MoodAffect.noOccupations, moodValues.values[MoodAffect.noOccupations]);
        }
        if (damaged)
        {
            notation.Add(MoodAffect.damaged, moodValues.values[MoodAffect.damaged]);
        }

        if (notation.ToFloat() >= 0)
            notation.Add(MoodAffect.everythingFine, moodValues.values[MoodAffect.everythingFine]);

        notation.Add(MoodAffect.nuisance, -house.nuisanceImpact);
        foreach (NotationModifier notationModifier in house.notationModifiers)
        {
            notation.Add(MoodAffect.habitationBuff , notationModifier.amount);
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