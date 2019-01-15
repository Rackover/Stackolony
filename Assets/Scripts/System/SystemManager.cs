using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour {

    public List<Generator> AllGenerators = new List<Generator>();
    public List<Block> AllBlocksRequiringPower = new List<Block>();
    public List<Block> AllBlockLinks = new List<Block>();
    public List<WorkingHours> AllTimeRelatedBlocks = new List<WorkingHours>();
    public List<Occupator> AllOccupators = new List<Occupator>();
    public List<House> AllHouses = new List<House>();
    public List<FoodProvider> AllFoodProviders = new List<FoodProvider>();
    public List<Spatioport> AllSpatioports = new List<Spatioport>();


    // removes a buliding from the system entirely
    public void RemoveBuilding(GameObject target)
    {
        AllGenerators.RemoveAll(o => o.gameObject == target);
        AllBlocksRequiringPower.RemoveAll(o => o.gameObject == target);
        AllBlockLinks.RemoveAll(o => o.gameObject == target);
        AllTimeRelatedBlocks.RemoveAll(o => o.gameObject == target);
        AllHouses.RemoveAll(o => o.gameObject == target);
        AllFoodProviders.RemoveAll(o => o.gameObject == target);
        AllSpatioports.RemoveAll(o => o.gameObject == target);
    }

    //Met à jour tout le système
    public void UpdateSystem()
    {
        StartCoroutine(RecalculateSystem());
    }

    public void UpdateElectricitySystem()
    {
        StartCoroutine(RecalculatePropagation());
    }

    //Actualise l'influence du spatioport
    public void UpdateSpatioportInfluence()
    {
        StartCoroutine(ResetSpatioportInfluence());
        StartCoroutine(RecalculateSpatioportInfluence());
    }

    //Actualise les données de chaque maisons
    public void UpdateHousesInformations()
    {
        foreach (House house in AllHouses)
        {
            if (house.gameObject.layer != LayerMask.NameToLayer("StoredBlock"))
            {
                house.UpdateHouseInformations();
            }
        }
        UpdateFoodProviders();
        UpdateOccupators();
    }

    //Assigne un travail à chaque citoyen
    public void UpdateJobsDistribution()
    {
        StartCoroutine(ResetJobs());
        StartCoroutine(RecalculateJobs());
    }

    //Recalcule les maisons influencées par les occupators (Generateurs de travail)
    public void UpdateOccupators()
    {
        StartCoroutine(ResetOccupators());
        StartCoroutine(RecalculateOccupators());
    }

    //Recalcule la distribution de nourriture
    public void UpdateFoodProviders()
    {
        StartCoroutine(ResetFoodConsumption());
        StartCoroutine(RecalculateFoodConsumption());
    }

    //Indique à chaque bloc qu'un cycle est passé
    public void UpdateCycle() {
        foreach (Block block in AllBlockLinks) {
            block.NewCycle();
        }
    }

    //Active ou desactive les blocs qui ont un script "WorkingHour" en fonction de l'heure du jeu
    public void CheckWorkingHours() {
        foreach (WorkingHours workingHour in AllTimeRelatedBlocks) {
            if (GameManager.instance.temporality.GetCurrentCycleProgression() > workingHour.startHour && workingHour.hasStarted == false) {
                workingHour.StartWork();
            } else if (GameManager.instance.temporality.GetCurrentCycleProgression() > workingHour.endHour && workingHour.hasStarted == true) {
                workingHour.EndWork();
            }
        }
    }


    #region SystemCoroutines
    //Système de coroutine qui permet de mettre à jour tout le système
    IEnumerator RecalculateSystem()
    {
        StartCoroutine(ResetBlocksPower());
        UpdateSpatioportInfluence();
        yield return StartCoroutine(RecalculateSystem2());
    }
    IEnumerator RecalculateSystem2()
    {
        StartCoroutine(RecalculatePropagation());
        yield return StartCoroutine(RecalculateSystem3());
    }
    IEnumerator RecalculateSystem3()
    {
        yield return new WaitForSeconds(0.5f);
        UpdateHousesInformations();
        yield return StartCoroutine(RecalculateSystem4());
    }
    IEnumerator RecalculateSystem4()
    {
        yield return new WaitForSeconds(0.5f);
        UpdateJobsDistribution();
        yield return null;
    }
    #endregion

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
        foreach (Block block in AllBlockLinks)
        {
            if (block.isConsideredDisabled && block.GetComponent<Spatioport>() == null)
            {
                block.Disable();
            }
        }
    }



    IEnumerator RecalculateJobs()
    {
        foreach (House house in AllHouses)
        {
            for (int i = 0; i < house.citizenCount; i++)
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

    IEnumerator RecalculateOccupators()
    {
        foreach (Occupator occupator in AllOccupators)
        {
            if (occupator.gameObject.layer != LayerMask.NameToLayer("StoredBlock"))
            {
                occupator.Invoke("OnBlockUpdate", 0f);
                yield return new WaitForEndOfFrame();
            }
        }
        yield return null;
    }

    IEnumerator RecalculateFoodConsumption()
    {
        foreach (FoodProvider foodProvider in AllFoodProviders)
        {
            if (foodProvider.gameObject.layer != LayerMask.NameToLayer("StoredBlock"))
            {
                foodProvider.Invoke("OnBlockUpdate", 0f);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator RecalculatePropagation()
    {
        foreach (Generator generator in AllGenerators)
        {
            if (generator.gameObject.layer != LayerMask.NameToLayer("StoredBlock"))
            {
                generator.Invoke("OnBlockUpdate", 0f);
                yield return new WaitForEndOfFrame();
            }
        }
        yield return null;
    }

    IEnumerator RecalculateSpatioportInfluence()
    {
        foreach (Spatioport spatioport in AllSpatioports)
        {
            spatioport.Invoke("OnBlockUpdate", 0f);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }


    IEnumerator ResetBlocksPower()
    {
        foreach (Block block in AllBlocksRequiringPower)
        {
            block.isConsideredUnpowered = true;
            block.currentPower = 0;
        }
        yield return null;
    }

    IEnumerator ResetJobs()
    {
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
        foreach (House house in AllHouses)
        {
            house.foodReceived = 0;
            house.foodProvidersInRange.Clear();
        }
        yield return null;
    }

    IEnumerator ResetSpatioportInfluence()
    {
        foreach (Block block in AllBlockLinks)
        {
            block.isConsideredDisabled = true;
        }
        yield return null;
    }
}
