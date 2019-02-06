using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsequencesManager : MonoBehaviour {

    //DONE
    //Add a moodModifier to a type of population
    static public void GenerateMoodModifier(Population pop, float amount, int durationInCycle)
    {
        GameManager.instance.populationManager.GenerateMoodModifier(pop, amount, durationInCycle);
    }

    //Change the consumption of a population type, affecting the houses they're in
    static public void GenerateFoodConsumptionModifier(Population pop, float amount, int durationInCycle)
    {
        GameManager.instance.populationManager.GenerateFoodModifier(pop, amount, durationInCycle);
    }

    //Changes the notation of a house (Raising the mood gained when a citizen chose this house)
    static public void ChangeHouseNotation(House house, float amount, int durationInCycle)
    {
        GameManager.instance.cityManager.GenerateNotationModifier(house, amount, durationInCycle);
    }

    //Generates a fireRiskModifier, to change the chances of a blockbeing set on fire each cycle
    static public void GenerateFireRiskModifier(Block block, int amountInPercent, int durationInCycle)
    {
        GameManager.instance.cityManager.GenerateFireRiskModifier(block, amountInPercent, durationInCycle);
    }

    //Changes the energy consumption of a block
    static public void GenerateConsumptionModifier(Block block, int amount, int durationInCycle)
    {
        GameManager.instance.cityManager.GenerateConsumptionModifier(block, amount, durationInCycle);
    }

    //Destroys the specified flag
    static public void DestroyFlag(Block block, System.Type flag)
    {
        if (GameManager.instance.cityManager.FindFlag(block, flag) != null)
        {
            Destroy(block.GetComponent(flag));
        }
    }

    //Destroys temporaly the specified flag
    static public void DestroyFlagTemporarily(Block block, System.Type flag, int cyclesRemaining)
    {
        if (GameManager.instance.cityManager.FindFlag(block, flag) != null)
        {
            GameManager.instance.cityManager.GenerateTempFlagDestroyer(block, flag, cyclesRemaining);
        }
    }

    //Change the state of a block (Damaged, Fire, Riot)
    static public void AddState(Block block, State state)
    {
        block.AddState(state);
    }

    static public void RemoveState(Block block, State state)
    {
        block.RemoveState(state);
    }

    static public void AddSettlerBonusForNextWave(Population pop, int amount)
    {
        GameManager.instance.timelineController.AddSettlerBonus(new KeyValuePair<Population, int>(pop, amount));
    }

    //Spawn a X blocks at a random location, choose a random block if no blockID is specified
    static public void SpawnBlocksAtRandomLocation(int amount, int blockID = -1)
    {
        Vector2Int location = GameManager.instance.gridManagement.GetRandomCoordinates();
        GameManager.instance.StartCoroutine(SpawnBlocksAtLocationC(amount, blockID, location));
    }

    static public void SpawnBlocksAtLocation(int amount, int blockID, Vector2Int location)
    {
        GameManager.instance.StartCoroutine(SpawnBlocksAtLocationC(amount, blockID, location));
    }
    static IEnumerator SpawnBlocksAtLocationC(int amount, int blockID, Vector2Int location)
    {
        location.y = GameManager.instance.gridManagement.gridSize.y - 1;
        for (int i = 0; i < amount; i++)
        {
            yield return new WaitForSeconds(1f);
            if (blockID < 0)
            {
                int randomID = GameManager.instance.library.GetRandomBlock().ID;
                GameManager.instance.gridManagement.LayBlock(randomID, new Vector2Int(location.x, location.y));
            }
            else
            {
                GameManager.instance.gridManagement.LayBlock(blockID, new Vector2Int(location.x, location.y));
            }
        }
        yield return null;
    }

    //Spawn a block on a position, choose a random block if no blockID is specified
    static public void SpawnBlock(Vector2Int coordinates, int amount, int blockID = -1)
    {
        GameManager.instance.StartCoroutine(SpawnBlockC(coordinates, amount, blockID));
    }
    static IEnumerator SpawnBlockC(Vector2Int coordinates, int amount, int blockID = -1)
    {
        for (int i = 0; i < amount; i++)
        {
            yield return new WaitForSeconds(0.1f);
            if (blockID < 0)
            {
                int randomID = GameManager.instance.library.GetRandomBlock().ID;
                GameManager.instance.gridManagement.LayBlock(randomID, coordinates);
            }
            else
            {
                GameManager.instance.gridManagement.LayBlock(blockID, coordinates);
            }
        }
        yield return null;
    }

    static public void DestroyBlock(Block block)
    {
        GameManager.instance.gridManagement.DestroyBlock(block.gridCoordinates);
    }

    static public void GenerateFlag(Block block, string flagInformation)
    {
        string[] flagElements = flagInformation.Split(new char[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (GameManager.instance.cityManager.FindFlag(block, System.Type.GetType(flagElements[0])) == null)
        {
            GameManager.instance.flagReader.ReadFlag(block, flagInformation);
        }
    }

    //Generates a new flag, taking the informations like in flag declaration (Ex : Generator_1_3), overrides flag values if flag is already here
    static public void GenerateNewFlag(Block block, string flagInformations)
    {
        string[] flagElements = flagInformations.Split(new char[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
        DestroyFlag(block, flagElements[0].GetType());
        GameManager.instance.flagReader.ReadFlag(block, flagInformations);
    }

    //Modify the flag with the new settings, only if flag already exists
    static public void ModifyFlag(Block block, string flagInformations, int durationInCycle)
    {
        string[] flagElements = flagInformations.Split(new char[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (GameManager.instance.cityManager.FindFlag(block, System.Type.GetType(flagElements[0])) != null)
        {
            GameManager.instance.cityManager.GenerateFlagModifier(block, flagInformations, durationInCycle);
        }
    }

    //Generates a flag that'll be removed after X time, only if the flag isn't already there
    static public void GenerateTempFlag(Block block, string flagInformations, int durationInCycle)
    {
        string[] flagElements = flagInformations.Split(new char[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (GameManager.instance.cityManager.FindFlag(block, System.Type.GetType(flagElements[0])) == null)
        {
            GameManager.instance.cityManager.GenerateTempFlag(block, flagInformations, durationInCycle);
        }
    }

    static public Block GetRandomBuildingOfId(int id)
    {
        List<Block> concerned = new List<Block>();
        foreach(Block block in GameManager.instance.systemManager.AllBlocks) {
            if (block.scheme.ID == id) {
                concerned.Add(block);
            }
        }
        if (concerned.Count < 1) {
            return null;
        }

        return concerned[Mathf.FloorToInt(Random.value * concerned.Count)];
    }

    static public House GetRandomHouseOf(Population pop)
    {
        List<House> houses = new List<House>();
        foreach(PopulationManager.Citizen citizen in GameManager.instance.populationManager.citizenList) {
            if (citizen.type == pop && citizen.habitation != null) {
                houses.Add(citizen.habitation);
            }
        }

        if (houses.Count < 1) {
            return null;
        }

        return houses[Mathf.FloorToInt(Random.value * houses.Count)];
    }


    //TO DO
    //Randomly spawns a mine on the map
    public void SpawnMine(int amount)
    {

    }
}
