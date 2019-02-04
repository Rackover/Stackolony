using System.Collections;
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
        GameManager.instance.populationManager.OnNewCycle();
        
        foreach (Block block in AllBlocks)
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
        GameManager.instance.populationManager.OnNewMicrocycle();
        yield return StartCoroutine(UpdateHousesInformations());
        yield return StartCoroutine(RecalculateFoodConsumption());
        yield return StartCoroutine(RecalculateOccupators());
        yield return StartCoroutine(UpdateHousesInformations());
        yield return StartCoroutine(RecalculateHabitation(GameManager.instance.temporality.GetMicroCoef()));
        yield return StartCoroutine(UpdateHousesInformations());
        yield return StartCoroutine(RecalculateJobs());
        yield return StartCoroutine(OnGridUpdate());
    }

    //S'execute à chaques fois qu'un bloc est déplacé dans la grille
    public IEnumerator OnGridUpdate()
    {
        StopAllCoroutines();
        yield return StartCoroutine(RecalculateSpatioportInfluence());
        yield return new WaitForSeconds(0.5f); //Clumsy, à changer rapidement, la propagation doit s'effectuer une fois que le spatioport a tout mis à jour
        yield return StartCoroutine(RecalculatePropagation());
        yield return StartCoroutine(RecalculateNuisance());
        yield return StartCoroutine(UpdateOverlay());
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
        foreach (Block block in AllBlocksRequiringPower)
        {
            if (block.isConsideredUnpowered == true)
            {
                block.ChangePower(0);
            }
        }
    }

    //Si un bloc consideré disabled n'a pas reçu d'explorer provenant du spatioport, il s'eteint. || Lancé automatiquement à la fin des calculs liés au spatioport
    public void UpdateBlocksDisabled()
    {
        foreach (Block block in AllBlocks)
        {
            if (block.isConsideredDisabled && block.GetComponent<Spatioport>() == null)
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
        foreach (House house in AllHouses)
        {
            house.UpdateHouseInformations();
        }
        yield return null;
    }

    public IEnumerator RecalculateHabitation(float x)
    {
        StartCoroutine(ResetHabitation());
        yield return new WaitForEndOfFrame();
        GameManager.instance.cityManager.HouseEveryone(x);
        yield return null;
    }

    public IEnumerator RecalculateJobs()
    {
        StartCoroutine(ResetJobs());
        yield return new WaitForEndOfFrame();
        foreach (House house in AllHouses)
        {
            for (int i = 0; i < house.affectedCitizen.Count; i++)
            {
                if (house.affectedCitizen[i].jobless == true)
                {
                    foreach (Occupator occupator in house.occupatorsInRange)
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
        yield return null;
    }

    public IEnumerator RecalculateOccupators()
    {
        StartCoroutine(ResetOccupators());
        foreach (Occupator occupator in AllOccupators)
        {
            occupator.Invoke("GenerateOccupations", 0f);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    public IEnumerator RecalculateFoodConsumption()
    {
        StartCoroutine(ResetFoodConsumption());
        foreach (FoodProvider foodProvider in AllFoodProviders)
        {
            foodProvider.Invoke("GenerateFood", 0f);
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator RecalculatePropagation()
    {
        yield return StartCoroutine(ResetBlocksPower());
        foreach (Generator generator in AllGenerators)
        {
            generator.Invoke("GenerateEnergy", 0f);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    public IEnumerator RecalculateSpatioportInfluence()
    {
        StartCoroutine(ResetSpatioportInfluence());
        foreach (Spatioport spatioport in AllSpatioports)
        {
            spatioport.Invoke("OnBlockUpdate", 0f);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    public IEnumerator RecalculateNuisance()
    {
        StartCoroutine(ResetNuisance());
        foreach (NuisanceGenerator nuisanceGenerator in AllNuisanceGenerators)
        {
            nuisanceGenerator.Invoke("GenerateNuisance", 0f);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }


    IEnumerator ResetBlocksPower()
    {
        Logger.Debug("Resetting block power ");
        foreach (Block block in AllBlocksRequiringPower)
        {
            block.isConsideredUnpowered = true;
            block.ChangePower(0);
        }
        yield return null;
    }

    IEnumerator ResetHabitation()
    {
        Logger.Debug("Resetting citizens habitations");
        foreach (House house in AllHouses)
        {
            house.affectedCitizen.Clear();
        }
        foreach (PopulationManager.Citizen citizen in GameManager.instance.populationManager.citizenList)
        {
            citizen.habitation = null;
        }
        yield return null;
    }

    IEnumerator ResetJobs()
    {
        Logger.Debug("Resetting jobs");
        foreach (Occupator occupator in AllOccupators)
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
        foreach (House house in AllHouses)
        {
            house.occupatorsInRange.Clear();
        }
        yield return null;
    }

    IEnumerator ResetFoodConsumption()
    {
        Logger.Debug("Resetting food consumption");
        foreach (House house in AllHouses)
        {
            house.foodReceived = 0;
            house.foodProvidersInRange.Clear();
        }
        yield return null;
    }

    IEnumerator ResetSpatioportInfluence()
    {
        Logger.Debug("Resetting spatioport influence");
        foreach (Block block in AllBlocks)
        {
            block.isLinkedToSpatioport = false;
            block.isConsideredDisabled = true;
        }
        yield return null;
    }

    IEnumerator ResetNuisance()
    {
        Logger.Debug("Resetting nuisance");
        foreach (Block block in AllBlocks)
        {
            block.nuisance = 0;
        }
        yield return null;
    }
}
