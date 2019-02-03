using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consequences : MonoBehaviour {

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


    //TO DO
    //Generates a new flag, taking the informations like in flag declaration (Ex : Generator_1_3), changes flag values if flag is already here
    public void GenerateNewFlag(Block block, string flagInformations)
    {

    }

    //Destroys the specified flag
    public void DestroyFlag(Block block, System.Type flagName)
    {

    }

    //Modify the flag with the new settings, only if flag already exists
    public void ModifyFlag(Block block, ModifierReason reason, string flagInformations, int durationInCycle)
    {

    }

    //Change the state of a block (Damaged, Fire, Riot)
    public void ChangeState(Block block, BlockState state)
    {

    }

    //Generates a flag that'll be removed after X time, only if the flag isn't already there
    public void GenerateTempFlag(Block block, ModifierReason reason, string flagInformations, int durationInCycle)
    {

    }

    //Spawn a block at a random location, choose a random block if no blockID is specified
    public void SpawnBlockAtRandomLocation(int blockID, int amount)
    {

    }

    //Spawn a block on a position, choose a random block if no blockID is specified
    public void SpawnBlock(int BlockID, Vector3Int coordinates, int amount)
    {

    }

    //Randomly spawns a mine on the map
    public void SpawnMine(int amount)
    {

    }
}
