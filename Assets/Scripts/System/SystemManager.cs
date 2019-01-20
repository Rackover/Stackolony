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
        foreach (Block block in AllBlocks)
        {
            block.NewCycle();
        }
        yield return null;
    }

    //S'execute à chaques fois qu'un microcycle passe
    public IEnumerator OnNewMicrocycle()
    {
        yield return StartCoroutine(UpdateHousesInformations());
        yield return StartCoroutine(RecalculateHabitation());
    }

    //S'execute à chaques fois qu'un bloc est déplacé dans la grille
    public IEnumerator OnGridUpdate()
    {
        yield return StartCoroutine(RecalculateSpatioportInfluence());
        yield return new WaitForSeconds(0.5f); //Clumsy, à changer rapidement, la propagation doit s'effectuer une fois que le spatioport a tout mis à jour
        yield return StartCoroutine(RecalculatePropagation());
        yield return StartCoroutine(RecalculateNuisance());
    }


    //Met à jour le système electrique
    public void UpdateElectricitySystem()
    {
        StartCoroutine(RecalculatePropagation());
    }

    //Actualise les données de chaque maisons
    public IEnumerator UpdateHousesInformations()
    {
        StartCoroutine(RecalculateFoodConsumption());
        StartCoroutine(RecalculateOccupators());
        StartCoroutine(RecalculateJobs());
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
                block.currentPower = 0;
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
                block.Disable();
            }
        }
    }

    public IEnumerator CalculateHouseInformation()
    {
        foreach (House house in AllHouses)
        {
            if (house.gameObject.layer != LayerMask.NameToLayer("StoredBlock"))
            {
                house.UpdateHouseInformations();
            }
        }
        yield return null;
    }

    public IEnumerator RecalculateHabitation()
    {
        StartCoroutine(ResetHabitation());
        GameManager.instance.cityManager.HouseEveryone();
        yield return null;
    }

    public IEnumerator RecalculateJobs()
    {
        StartCoroutine(ResetJobs());
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
            if (occupator.gameObject.layer != LayerMask.NameToLayer("StoredBlock"))
            {
                occupator.Invoke("GenerateOccupations", 0f);
                yield return new WaitForEndOfFrame();
            }
        }
        yield return null;
    }

    public IEnumerator RecalculateFoodConsumption()
    {
        StartCoroutine(ResetFoodConsumption());
        foreach (FoodProvider foodProvider in AllFoodProviders)
        {
            if (foodProvider.gameObject.layer != LayerMask.NameToLayer("StoredBlock"))
            {
                foodProvider.Invoke("GenerateFood", 0f);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator RecalculatePropagation()
    {
        StartCoroutine(ResetBlocksPower());
        foreach (Generator generator in AllGenerators)
        {
            if (generator.gameObject.layer != LayerMask.NameToLayer("StoredBlock"))
            {
                generator.Invoke("GenerateEnergy", 0f);
                yield return new WaitForEndOfFrame();
            }
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
            block.currentPower = 0;
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
