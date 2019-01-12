using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour {

    public List<Generator> AllGenerators = new List<Generator>();
    public List<BlockLink> AllBlocksRequiringPower = new List<BlockLink>();
    public List<BlockLink> AllBlockLinks = new List<BlockLink>();
    public List<WorkingHours> AllTimeRelatedBlocks = new List<WorkingHours>();
    public List<Occupator> AllOccupators = new List<Occupator>();
    public List<House> AllHouses = new List<House>();
    public List<FoodProvider> AllFoodProviders = new List<FoodProvider>();

    public void UpdateSystem()
    {
        StartCoroutine(RecalculateSystem());
    }

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

    public void UpdateJobsDistribution()
    {
        StartCoroutine(ResetJobs());
        StartCoroutine(RecalculateJobs());
    }

    public void UpdateOccupators()
    {
        StartCoroutine(ResetOccupators());
        StartCoroutine(RecalculateOccupators());
    }

    public void UpdateFoodProviders()
    {
        StartCoroutine(ResetFoodConsumption());
        StartCoroutine(RecalculateFoodConsumption());
    }

    public void UpdateCycle() {
        foreach (BlockLink block in AllBlockLinks) {
            block.NewCycle();
        }
    }

    public void CheckWorkingHours() {
        foreach (WorkingHours workingHour in AllTimeRelatedBlocks) {
            if (GameManager.instance.temporality.GetCurrentcycleProgression() > workingHour.startHour && workingHour.hasStarted == false) {
                workingHour.StartWork();
            } else if (GameManager.instance.temporality.GetCurrentcycleProgression() > workingHour.endHour && workingHour.hasStarted == true) {
                workingHour.EndWork();
            }
        }
    }

    IEnumerator RecalculateSystem()
    {
        StartCoroutine(ResetBlocksPower());
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

    //Si un block qui requiert du courant n'a pas croisé d'explorer, alors on l'eteint. Sinon on l'allume
    public void UpdateBlocksRequiringPower()
    {
        foreach (BlockLink block in AllBlocksRequiringPower)
        {
            if (block.isConsideredUnpowered == true)
            {
                block.currentPower = 0;
                block.ChangePower(0);
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



    IEnumerator ResetBlocksPower()
    {
        foreach (BlockLink block in AllBlocksRequiringPower)
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
}
