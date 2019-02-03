using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCallbackManager : MonoBehaviour 
{
    public GameObject prefabRemoving;
    public GameObject prefabEmitting;
    public MissionManager.Mission mission;
    public int activeCoroutinesRelatedToPower;
    public int activeCoroutinesRelatedToSpatioport;
    

    IEnumerator DistributeFood()
    {
        MissionManager.Mission myMission = mission;
        FoodProvider foodProvider = GameManager.instance.gridManagement.grid[myMission.position.x, myMission.position.y, myMission.position.z].GetComponent<FoodProvider>();
        float foodLeft = foodProvider.foodTotal;
        foreach (Block blocklink in myMission.blocksFound)
        {
            House house = blocklink.GetComponent<House>();
            house.foodProvidersInRange.Add(foodProvider);
            if (foodLeft > 0)
            {
                if (foodLeft > house.foodConsumption)
                {
                    house.foodReceived = house.foodConsumption;
                    foodLeft -= house.foodConsumption;
                } else
                {
                    house.foodReceived = foodLeft;
                    foodLeft = 0;
                }
            }
        }
        foodProvider.foodLeft = foodLeft;
        GameManager.instance.missionManager.EndMission(myMission);
        activeCoroutinesRelatedToPower--;
        yield return null;
    }

    IEnumerator EmitOccupators()
    {
        MissionManager.Mission myMission = mission;
        Occupator linkedOccupator = GameManager.instance.gridManagement.grid[myMission.position.x, myMission.position.y, myMission.position.z].GetComponent<Occupator>();
        foreach (Block blocklink in myMission.blocksFound)
        {
            House house = blocklink.GetComponent<House>();
            house.occupatorsInRange.Add(linkedOccupator);
        }
        GameManager.instance.missionManager.EndMission(myMission);
        yield return null;
    }

    IEnumerator EmitEnergy()
    {
        activeCoroutinesRelatedToPower++;
        MissionManager.Mission myMission = mission;
        foreach (Block blocklink in myMission.blocksFound)
        {
            for (int i = blocklink.scheme.consumption - blocklink.currentPower; i > 0; i--)
            {
                if (myMission.power > 0)
                {
                    GameObject energyFeedback = Instantiate(prefabEmitting);
                    if (!GameManager.instance.DEBUG_MODE) {
                        prefabEmitting.GetComponent<MeshRenderer>().enabled = false;
                        foreach(MeshRenderer mesh in prefabEmitting.GetComponentsInChildren<MeshRenderer>()) {
                            mesh.enabled = false;
                        }
                    }
                    energyFeedback.transform.position = blocklink.transform.position;
                    Destroy(energyFeedback, 2);
                    blocklink.AddPower(1);
                    myMission.power--;
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        GameManager.instance.missionManager.EndMission(myMission);
        activeCoroutinesRelatedToPower--;
        if (activeCoroutinesRelatedToPower <= 0)
        {
            if (GameManager.instance.systemManager != null)
            {
                GameManager.instance.systemManager.UpdateBlocksRequiringPower();
                GameManager.instance.systemManager.OnCalculEnd();
            }
        }
        yield return null;
    }

    IEnumerator EmitSpatioportInfluence()
    {
        activeCoroutinesRelatedToSpatioport++;
        MissionManager.Mission myMission = mission;
        foreach (Block blocklink in mission.blocksFound)
        {
            blocklink.isConsideredDisabled = false;
            blocklink.isLinkedToSpatioport = true;
            blocklink.Enable();
            yield return new WaitForEndOfFrame();
        }
        GameManager.instance.missionManager.EndMission(myMission);
        activeCoroutinesRelatedToSpatioport--;
        if (activeCoroutinesRelatedToSpatioport <= 0)
        {
            if (GameManager.instance.systemManager != null)
            {
                GameManager.instance.systemManager.UpdateBlocksDisabled();
                GameManager.instance.systemManager.OnCalculEnd();
            }
        }
        yield return null;
    }

    IEnumerator EmitNuisance()
    {
        MissionManager.Mission myMission = mission;
        NuisanceGenerator linkedGenerator = GameManager.instance.gridManagement.grid[myMission.position.x, myMission.position.y, myMission.position.z].GetComponent<NuisanceGenerator>();
        for (int i = 0; i < myMission.blocksFound.Count; i++)
        {
            int distanceToCenter = myMission.blockDistanceToCenter[i] + 1;
            if (distanceToCenter > linkedGenerator.amount) distanceToCenter = linkedGenerator.amount; //In case the range is
            myMission.blocksFound[i].nuisance += linkedGenerator.amount - myMission.blockDistanceToCenter[i] + 1; //+1 because the first block doesn't count
        }
        GameManager.instance.missionManager.EndMission(myMission);
        yield return null;
    }

    IEnumerator Extinguish() 
    {
        MissionManager.Mission myMission = mission;
        for (int i = 1; i < myMission.blocksFound.Count; i++)
        {
            if(myMission.blocksFound[i].states.ContainsKey(State.OnFire))
            {
                OnFire state = myMission.blocksFound[i].states[State.OnFire] as OnFire;

                if(!state.beingExtinguished)
                {
                    state.StartExtinguish();
                    break;
                    yield return null;
                }
            }
        }
        GameManager.instance.missionManager.EndMission(myMission);
        yield return null;
    }

    IEnumerator Repress() 
    {
        MissionManager.Mission myMission = mission;
        for (int i = 1; i < myMission.blocksFound.Count; i++)
        {
            if(myMission.blocksFound[i].states.ContainsKey(State.OnRiot))
            {
                OnRiot state = myMission.blocksFound[i].states[State.OnRiot] as OnRiot;
                if(!state.beingRepressed)
                {
                    state.StartRepress();
                    break;
                    yield return null;
                }
            }
        }
        GameManager.instance.missionManager.EndMission(myMission);
        yield return null;
    }

    IEnumerator Repair() 
    {
        MissionManager.Mission myMission = mission;
        for (int i = 1; i < myMission.blocksFound.Count; i++)
        {
            if(myMission.blocksFound[i].states.ContainsKey(State.Damaged))
            {
                Damaged state = myMission.blocksFound[i].states[State.Damaged] as Damaged;
                if(!state.beingRepaired)
                {
                    state.StartRepair();
                    break;
                    yield return null;
                }
            }
        }
        GameManager.instance.missionManager.EndMission(myMission);
        yield return null;
    }
}
