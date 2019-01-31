using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CityManager : MonoBehaviour {

    public enum BuildingType { Habitation = 0, Services = 1, Occupators = 2 };
    public string cityName = "Valenciennes";
    public BlockState[] accidentStates = { BlockState.OnFire, BlockState.OnRiot, BlockState.Damaged };
    public Dictionary<Population, Dictionary<House, float>> topHabitations = new Dictionary<Population, Dictionary<House, float>>(); // List of the best habitations (sorted from best to worst)

    List<int> lockedBuildings = new List<int >();

    //Comment fonctionne la notation d'une maison :
    // 
    [System.Serializable]
    public class MoodValues
    {
        public int goodNotationTreshold = -5; //Above this note, house is considered "Good"
        public int badNotationTreshold = -10; //Under this note, house is considered "Bad"
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
        foreach (Population pop in GameManager.instance.populationManager.populationTypeList) {
            topHabitations[pop] = new Dictionary<House, float>();
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
        GetBestHouses();
        for (int i = 0; i < GameManager.instance.populationManager.populationTypeList.Length; i++)
        {
            HousePopulation(GameManager.instance.populationManager.populationTypeList[i], x);
        }
    }

    //Finds a house for every citizens from a defined population
    public void HousePopulation(Population pop, float x)
    {
        foreach (PopulationManager.Citizen citizen in GameManager.instance.populationManager.populationCitizenList[pop])
        {
            if (topHabitations[pop].Count > 0)
            {
                House foundHouse = topHabitations[pop].First().Key;
                foundHouse.FillWithCitizen(citizen);
                Logger.Debug("Citizen " + citizen.name + " of type " + citizen.type.codeName + " has been housed at the house " + foundHouse);
                //Applique le changement d'humeur au type de population
                GameManager.instance.populationManager.ChangePopulationMood(pop, topHabitations[pop].First().Value);

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

        foreach (Population profile in house.acceptedPop)
        {
            if (profile == populationType)
            {
                profileFound = true;
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
        if (foodStock > 0 && foodStock >= house.foodConsumptionPerHabitant)
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
                }
            }
        }

        if (!profileFound)
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

        if (notation >= 0)
            notation += moodValues.everythingFine;

        return notation;
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
        else Logger.Debug( accident + " is not considered as a accident" );
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
