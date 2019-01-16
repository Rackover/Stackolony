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
            for (int i = blocklink.block.consumption - blocklink.currentPower; i > 0; i--)
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
                    blocklink.ChangePower(1);
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
            }
        }
        yield return null;
    }

}
