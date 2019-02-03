using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsequencesManager : MonoBehaviour {

    //DONE
    //Add a moodModifier to a type of population
    public void GenerateMoodModifier(Population pop, ModifierReason reason, float amount, int durationInCycle)
    {
        GameManager.instance.populationManager.GenerateMoodModifier(pop, reason, amount, durationInCycle);
    }

    //Change the consumption of a population type, affecting the houses they're in
    public void GenerateFoodConsumptionModifier(Population pop, ModifierReason reason, float amount, int durationInCycle)
    {
        GameManager.instance.populationManager.GenerateFoodModifier(pop, reason, amount, durationInCycle);
    }

    //Changes the notation of a house (Raising the mood gained when a citizen chose this house)
    public void ChangeHouseNotation(House house, ModifierReason reason, float amount, int durationInCycle)
    {
        GameManager.instance.cityManager.GenerateNotationModifier(house, reason, amount, durationInCycle);
    }

    //Changes the energy consumption of a block
    public void GenerateConsumptionModifier(Block block, ModifierReason reason, int amount, int durationInCycle)
    {
        GameManager.instance.cityManager.GenerateConsumptionModifier(block, reason, amount, durationInCycle);
    }

    //Destroys the specified flag
    public void DestroyFlag(Block block, System.Type flag)
    {
        if (GameManager.instance.cityManager.FindFlag(block, flag) != null)
        {
            Destroy(block.GetComponent<Flag>());
        }
    }

    //Change the state of a block (Damaged, Fire, Riot)
    public void AddState(Block block, State state)
    {
        block.AddState(state);
    }

    public void RemoveState(Block block, State state)
    {
        block.RemoveState(state);
    }

    //Spawn a X blocks at a random location, choose a random block if no blockID is specified
    public void SpawnBlocksAtRandomLocation(int amount, int blockID = -1)
    {
        StartCoroutine(SpawnBlocksAtRandomLocationC(amount, blockID));
    }
    IEnumerator SpawnBlocksAtRandomLocationC(int amount, int blockID)
    {
        Vector3Int location = GameManager.instance.gridManagement.GetRandomCoordinates();
        location.y = GameManager.instance.gridManagement.gridSize.y - 1;
        for (int i = 0; i < amount; i++)
        {
            yield return new WaitForSeconds(0.1f);
            if (blockID < 0)
            {
                int randomID = GameManager.instance.library.GetRandomBlock().ID;
                GameManager.instance.gridManagement.LayBlock(randomID, new Vector2Int(location.x, location.z));
            }
            else
            {
                GameManager.instance.gridManagement.LayBlock(blockID, new Vector2Int(location.x, location.z));
            }
        }
        yield return null;
    }

    //Spawn a block on a position, choose a random block if no blockID is specified
    public void SpawnBlock(Vector2Int coordinates, int amount, int blockID = -1)
    {
        StartCoroutine(SpawnBlockC(coordinates, amount, blockID));
    }
    IEnumerator SpawnBlockC(Vector2Int coordinates, int amount, int blockID = -1)
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

    public void DestroyBlock(Block block)
    {
        GameManager.instance.gridManagement.DestroyBlock(block.gridCoordinates);
    }

    //Generates a new flag, taking the informations like in flag declaration (Ex : Generator_1_3), overrides flag values if flag is already here
    public void GenerateNewFlag(Block block, string flagInformations)
    {
        string[] flagElements = flagInformations.Split(new char[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
        DestroyFlag(block, flagElements[0].GetType());
        GameManager.instance.flagReader.ReadFlag(block, flagInformations);
    }


    //TO DO
    //Modify the flag with the new settings, only if flag already exists
    public void ModifyFlag(Block block, ModifierReason reason, string flagInformations, int durationInCycle)
    {

    }

    //Generates a flag that'll be removed after X time, only if the flag isn't already there
    public void GenerateTempFlag(Block block, ModifierReason reason, string flagInformations, int durationInCycle)
    {

    }


    //Randomly spawns a mine on the map
    public void SpawnMine(int amount)
    {

    }

    public Block GetRandomBuildingOfId(int id)
    {
        List<Block> concerned = new List<Block>();
        foreach(Block block in GameManager.instance.systemManager.AllBlocks) {
            if (block.scheme.ID == id) {
                concerned.Add(block);
            }
        }
        if (concerned.Count > 0) {
            return null;
        }
        return concerned[Mathf.FloorToInt(Random.value * concerned.Count)];
    }
}
