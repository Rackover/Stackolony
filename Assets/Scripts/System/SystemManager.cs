﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour {

    public List<Generator> AllGenerators = new List<Generator>();
    public List<Block> AllBlocksRequiringPower = new List<Block>();
    public List<Block> AllBlocks = new List<Block>();
    public List<Occupator> AllOccupators = new List<Occupator>();
    public List<House> AllHouses = new List<House>();
    public List<FoodProvider> AllFoodProviders = new List<FoodProvider>();
    public List<Spatioport> AllSpatioports = new List<Spatioport>();
    public List<NuisanceGenerator> AllNuisanceGenerators = new List<NuisanceGenerator>();
    public List<FireRiskGenerator> AllFireRiskGenerators = new List<FireRiskGenerator>();

    private bool systemResetted;
    public bool occupatorsUpdated;
    public bool foodUpdated;


    /* FONCTIONNEMENT DU SYSTEME 
     * Système recalculé à chaque déplacement de block : 
     *      - Spatioport influence
     *      - Power propagation
     *      - Nuisance
     *      
     * Système recalculé à chaque cycle :
     *      - 
     *      
     * Système recalculé à chaque microcycle :
     *      - Food distribution
     *      - Occupators influence
     *      - House information
     *      - Job distribution
     *      - Mood
     *      - Achievements
    */

    

    // removes a buliding from the system entirely
    public void RemoveBuilding(GameObject target)
    {
        AllGenerators.RemoveAll(o => o.gameObject == target);
        AllBlocksRequiringPower.RemoveAll(o => o.gameObject == target);
        AllBlocks.RemoveAll(o => o.gameObject == target);
        AllHouses.RemoveAll(o => o.gameObject == target);
        AllFoodProviders.RemoveAll(o => o.gameObject == target);
        AllSpatioports.RemoveAll(o => o.gameObject == target);
    }

    public void ClearSystem()
    {
        AllGenerators.Clear();
        AllBlocksRequiringPower.Clear();
        AllBlocks.Clear();
        AllHouses.Clear();
        AllSpatioports.Clear();
        AllFoodProviders.Clear();
    }

    public void ResetSystem()
    {
        if (systemResetted == false)
        {
            GameManager.instance.missionCallbackManager.Reset();
            StopAllCoroutines();
            systemResetted = true;
        }
    }

    //Met à jour tout le système (Only on load)
    public void UpdateSystem()
    {
        StartCoroutine(RecalculateSystem());
    }

    IEnumerator RecalculateSystem()
    {
        StartCoroutine(OnGridUpdate());
        StartCoroutine(OnNewCycle());
        StartCoroutine(OnNewMicrocycle());
        yield return null;
    }

    //S'execute à chaques fois qu'un cycle passe
    public IEnumerator OnNewCycle()
    {
        ResetSystem();
        GameManager.instance.populationManager.OnNewCycle();
        GameManager.instance.cityManager.OnNewCycle();

        foreach (Block block in AllBlocks.ToArray())
        {
            block.OnNewCycle();
        }
        
        RefreshMoodModifiers();
        RefreshFoodModifiers();
        RefreshNotationModifiers();
        RefreshConsumptionModifiers();
        RefreshFlagModifiers();
        RefreshTempFlags();
        RefreshTempFlagDestroyers();
        RefreshFireRiskModifiers();

        yield return StartCoroutine(OnNewMicrocycle());
        yield return null;
    }

    //S'execute à chaques fois qu'un microcycle passe
    public IEnumerator OnNewMicrocycle()
    {
        ResetSystem();
        GameManager.instance.populationManager.OnNewMicrocycle();
        GameManager.instance.roamerManager.OnNewMicrocycle();

        // ToArray() is to prevent foreach errors by copying the AllBlocks array
        foreach (Block block in AllBlocks.ToArray())
        {
            if(block != null) block.OnNewMicroycle();
        }
        yield return StartCoroutine(RecalculateSpatioportInfluence());
        yield return StartCoroutine(RecalculatePropagation());
        yield return StartCoroutine(UpdateHousesInformations());

        foodUpdated = false;
        yield return StartCoroutine(RecalculateFoodConsumption());
        while (!foodUpdated)
        {
            yield return null;
        }
        occupatorsUpdated = false;
        yield return StartCoroutine(RecalculateOccupators());
        while (!occupatorsUpdated)
        {
            yield return null;
        }
        yield return StartCoroutine(UpdateHousesInformations());
        yield return StartCoroutine(RecalculateHabitation(GameManager.instance.temporality.GetMicroCoef()));
        yield return StartCoroutine(UpdateHousesInformations());
        yield return StartCoroutine(RecalculateJobs());
        yield return StartCoroutine(OnGridUpdate());
        yield return StartCoroutine(RecalculateNuisance());
        yield return StartCoroutine(UpdateOverlay());
        yield return StartCoroutine(RecalculateFireRisks());
        yield return StartCoroutine(GameManager.instance.achievementManager.achiever.CheckAllAchievements());
    }

    //S'execute à chaques fois qu'un bloc est déplacé dans la grille
    public IEnumerator OnGridUpdate()
    {
        ResetSystem();
        foreach (Block block in AllBlocks.ToArray())
        {
            if(block != null) block.OnGridUpdate();
        }
		
        systemResetted = false;
        yield return null;
    }

    public IEnumerator UpdateOverlay()
    {
        GameManager.instance.overlayManager.UpdateOverlay();
        yield return null;
    }

    public void OnCalculEnd()
    {
        StartCoroutine(UpdateOverlay());
    }

    //Met à jour le système electrique
    public void UpdateElectricitySystem()
    {
        StartCoroutine(RecalculatePropagation());
    }

    //Actualise les données de chaque maisons
    public IEnumerator UpdateHousesInformations()
    {
        StartCoroutine(CalculateHouseInformation());
        yield return null;
    }


    //Si un block qui requiert du courant n'a pas croisé d'explorer, alors on l'eteint. Sinon on l'allume || Lancé automatiquement à la fin des calculs liés au power
    public void UpdateBlocksRequiringPower()
    {
        foreach (Block block in AllBlocks)
        {
            block.UpdatePower();
        }
    }

    //Si un bloc consideré disabled n'a pas reçu d'explorer provenant du spatioport, il s'eteint. || Lancé automatiquement à la fin des calculs liés au spatioport
    public void UpdateBlocksDisabled()
    {
        foreach (Block block in AllBlocks)
        {
            if (block.isConsideredDisabled && block.GetComponent<Spatioport>() == null && block.scheme.relyOnSpatioport)
            {
                block.Pack();
            }
        }
    }

    //Remove 1 cycle on each moodmodifiers duration
    public void RefreshMoodModifiers()
    {
        foreach (KeyValuePair<Population, PopulationManager.PopulationInformation> p in GameManager.instance.populationManager.populations)
        {
            List<MoodModifier> newMoodModifiers = new List<MoodModifier>();
            foreach (MoodModifier moodModifier in p.Value.moodModifiers)
            {
                moodModifier.cyclesRemaining--;

                if (moodModifier.cyclesRemaining != 0)
                {
                    newMoodModifiers.Add(moodModifier);
                }
            }
            p.Value.moodModifiers = newMoodModifiers;
        }
    }

    public void RefreshTempFlagDestroyers()
    {
        foreach (Block block in AllBlocks)
        {
            List<TempFlagDestroyer> newTempFlagDestroyerList = new List<TempFlagDestroyer>();
            foreach (TempFlagDestroyer tempFlagDestroyer in block.tempFlagDestroyers)
            {
                tempFlagDestroyer.cyclesRemaining--;
                if (tempFlagDestroyer.cyclesRemaining == 0)
                {
                    //Recreate the flag
                    GameManager.instance.flagReader.ReadFlag(block, tempFlagDestroyer.flagInformations);
                }
                else
                {
                    newTempFlagDestroyerList.Add(tempFlagDestroyer);
                }
            }
            block.tempFlagDestroyers = newTempFlagDestroyerList;
        }
    }

    //Remove 1 cycle on each notationModifier duration
    public void RefreshNotationModifiers()
    {
        foreach (House house in AllHouses)
        {
            List<NotationModifier> newNotationModifiers = new List<NotationModifier>();
            foreach (NotationModifier notationModifier in house.notationModifiers)
            {
                notationModifier.cyclesRemaining--;

                if (notationModifier.cyclesRemaining != 0)
                {
                    newNotationModifiers.Add(notationModifier);
                }
            }
            house.notationModifiers = newNotationModifiers;
        }
    }

    public void RefreshFireRiskModifiers()
    {
        foreach (Block block in AllBlocks)
        {
            List<FireRiskModifier> newFireRiskModifiers = new List<FireRiskModifier>();
            foreach (FireRiskModifier fireRiskModifier in block.fireRiskModifiers)
            {
                fireRiskModifier.cyclesRemaining--;
                if (fireRiskModifier.cyclesRemaining != 0)
                {
                    newFireRiskModifiers.Add(fireRiskModifier);
                }
            }
            block.fireRiskModifiers = newFireRiskModifiers;
        }
    }

    //Remove 1 cycle on each foodmodifiers
    public void RefreshFoodModifiers()
    {
        foreach (KeyValuePair<Population, PopulationManager.PopulationInformation> p in GameManager.instance.populationManager.populations)
        {
            List<FoodModifier> newFoodModifierList = new List<FoodModifier>();
            foreach (FoodModifier fm in p.Value.foodModifiers)
            {
                fm.cyclesRemaining--;
                if (fm.cyclesRemaining != 0)
                {
                    newFoodModifierList.Add(fm);
                }
            }
            p.Value.foodModifiers = newFoodModifierList;
        }
    }

    public void RefreshConsumptionModifiers()
    {
        foreach (Block block in AllBlocks)
        {
            List<ConsumptionModifier> newConsumptionModifierList = new List<ConsumptionModifier>();
            foreach (ConsumptionModifier consumptionModifier in block.consumptionModifiers)
            {
                consumptionModifier.cyclesRemaining--;
                if (consumptionModifier.cyclesRemaining != 0)
                {
                    newConsumptionModifierList.Add(consumptionModifier);
                }
            }
            block.consumptionModifiers = newConsumptionModifierList;
        }
    }

    public void RefreshFlagModifiers()
    {
        foreach (Block block in AllBlocks)
        {
            List<FlagModifier> newFlagModifierList = new List<FlagModifier>();
            foreach (FlagModifier flagModifier in block.flagModifiers)
            {
                flagModifier.cyclesRemaining--;
                if (flagModifier.cyclesRemaining == 0)
                {
                    //Removes the flagModifier effect
                    string invertedFlagDatas = GameManager.instance.flagReader.GetInvertedFlag(flagModifier.flagInformations);
                    GameManager.instance.flagReader.ReadFlag(block, invertedFlagDatas);
                } else
                {
                    newFlagModifierList.Add(flagModifier);
                }
            }
            block.flagModifiers = newFlagModifierList;
        }
    }

    public void RefreshTempFlags()
    {
        foreach (Block block in AllBlocks)
        {
            List<TempFlag> newTempFlagList = new List<TempFlag>();
            foreach (TempFlag tempFlag in block.tempFlags)
            {
                tempFlag.cyclesRemaining--;
                if (tempFlag.cyclesRemaining == 0)
                {
                    System.Type flagToRemove = tempFlag.flagType;
                    Destroy(block.GetComponent(flagToRemove));
                }
                else
                {
                    newTempFlagList.Add(tempFlag);
                }
            }
            block.tempFlags = newTempFlagList;
        }
    }

    public IEnumerator CalculateHouseInformation()
    {
        foreach (House house in AllHouses.ToArray())
        {
            house.UpdateHouseInformations();
        }
        yield return null;
    }

    public IEnumerator RecalculateFireRisks()
    {
        yield return StartCoroutine(ResetFireRisks());
        yield return new WaitForEndOfFrame();
        foreach (FireRiskGenerator fg in AllFireRiskGenerators.ToArray())
        {
            fg.Invoke("GenerateFireRisks", 0f);
        }
        yield return null;
    }

    public IEnumerator RecalculateHabitation(float x)
    {
        GameManager.instance.cityManager.HouseEveryone(x);
        yield return null;
    }

    public IEnumerator RecalculateJobs()
    {
        yield return StartCoroutine(ResetJobs());
        yield return new WaitForEndOfFrame();
        foreach (House house in AllHouses)
        {
            for (int i = 0; i < house.affectedCitizen.Count; i++)
            {
                if (house.affectedCitizen[i].jobless == true)
                {
                    foreach (Occupator occupator in house.occupatorsInRange.ToArray())
                    {
                        if(occupator != null)
                        {
                            if (occupator.affectedCitizen.Count < occupator.slots && !occupator.affectedCitizen.Contains(house.affectedCitizen[i]))
                            {
                                foreach (Population acceptedPop in occupator.acceptedPopulation)
                                {
                                    if (house.affectedCitizen[i].type == acceptedPop)
                                    {
                                        occupator.affectedCitizen.Add(house.affectedCitizen[i]);
                                        house.affectedCitizen[i].jobless = false;
                                        yield return null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        yield return null;
    }

    public IEnumerator RecalculateOccupators()
    {
        yield return StartCoroutine(ResetOccupators());
        if (AllOccupators.Count <= 0)
        {
            occupatorsUpdated = true;
        }
        foreach (Occupator occupator in AllOccupators.ToArray())
        {
            if(occupator != null)
            {
                occupator.GenerateOccupations();
                yield return new WaitForEndOfFrame();
            }
        }
        yield return null;
    }

    public IEnumerator RecalculateFoodConsumption()
    {
        yield return StartCoroutine(ResetFoodConsumption());
        if (AllFoodProviders.Count <= 0)
        {
            foodUpdated = true;
        }
        foreach (FoodProvider foodProvider in AllFoodProviders.ToArray())
        {
            if(foodProvider != null)
            {
                foodProvider.GenerateFood();
                yield return new WaitForEndOfFrame();
            }

        }
    }

    public IEnumerator RecalculatePropagation()
    {
        yield return StartCoroutine(ResetBlocksPower());
        if (AllGenerators.Count <= 0) { UpdateBlocksRequiringPower(); }
        foreach (Generator generator in AllGenerators.ToArray())
        {
            if(generator != null)
            {
                generator.GenerateEnergy();
                yield return new WaitForEndOfFrame();
            }
        }
        yield return null;
    }

    public IEnumerator RecalculateSpatioportInfluence()
    {
        yield return StartCoroutine(ResetSpatioportInfluence());
        foreach (Spatioport spatioport in AllSpatioports.ToArray())
        {
            if(spatioport != null)
            {
                spatioport.OnBlockUpdate();
                yield return new WaitForEndOfFrame();
            }
        }
        yield return null;
    }

    public IEnumerator RecalculateNuisance()
    {
        yield return StartCoroutine(ResetNuisance());
        foreach (NuisanceGenerator nuisanceGenerator in AllNuisanceGenerators)

        {
            nuisanceGenerator.GenerateNuisance();
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }


    IEnumerator ResetBlocksPower()
    {
        Logger.Debug("Resetting block power ");
        foreach (Block block in AllBlocksRequiringPower.ToArray())
        {
            block.hiddenPower = 0;
        }
        yield return null;
    }

    IEnumerator ResetFireRisks()
    {
        Logger.Debug("Resetting fire risks");
        foreach (Block block in AllBlocks.ToArray())
        {
            block.fireRiskPercentage = 0;
        }
        yield return null;
    }

    IEnumerator ResetJobs()
    {
        Logger.Debug("Resetting jobs");
        foreach (Occupator occupator in AllOccupators.ToArray())
        {
            occupator.affectedCitizen.Clear();
        }

        foreach (PopulationManager.Citizen citizen in GameManager.instance.populationManager.citizenList)
        {
            citizen.jobless = true;
        }
        yield return null;
    }

    IEnumerator ResetOccupators()
    {
        foreach (House house in AllHouses.ToArray())
        {
            house.occupatorsInRange.Clear();
        }
        yield return null;
    }

    IEnumerator ResetFoodConsumption()
    {
        Logger.Debug("Resetting food consumption");
        foreach (House house in AllHouses.ToArray())
        {
            house.foodReceived = 0;
            house.foodProvidersInRange.Clear();
        }
        yield return null;
    }

    IEnumerator ResetSpatioportInfluence()
    {
        Logger.Debug("Resetting spatioport influence");
        foreach (Block block in AllBlocks.ToArray())
        {
            block.isLinkedToSpatioport = false;
            block.isConsideredDisabled = true;
        }
        yield return null;
    }

    IEnumerator ResetNuisance()
    {
        Logger.Debug("Resetting nuisance");
        foreach (Block block in AllBlocks.ToArray())
        {
            block.nuisance = 0;
        }
        yield return null;
    }
}
