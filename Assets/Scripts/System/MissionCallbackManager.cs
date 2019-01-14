using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCallbackManager : MonoBehaviour 
{
    public GameObject prefabRemoving;
    public GameObject prefabEmitting;
    public MissionManager.Mission mission;
    public int activeCoroutines;
    
    //Must be called at the start of every callback function
    void InitCallback()
    {
        activeCoroutines++;
        MissionManager.Mission myMission = mission;
    }

    //Must be called at the end of every callback function
    void EndCallBack()
    {
        activeCoroutines--;
        if (activeCoroutines <= 0)
        {
            if (GameManager.instance.systemManager != null)
            {
                GameManager.instance.systemManager.OnCalculEnd();
            }
        }
    }

    IEnumerator DistributeFood()
    {
        InitCallback();
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
        EndCallBack();
        yield return null;
    }
    IEnumerator EmitOccupators()
    {
        InitCallback();
        MissionManager.Mission myMission = mission;
        Occupator linkedOccupator = GameManager.instance.gridManagement.grid[myMission.position.x, myMission.position.y, myMission.position.z].GetComponent<Occupator>();
        foreach (Block blocklink in myMission.blocksFound)
        {
            House house = blocklink.GetComponent<House>();
            house.occupatorsInRange.Add(linkedOccupator);
        }
        GameManager.instance.missionManager.EndMission(myMission);
        EndCallBack();
        yield return null;
    }
    IEnumerator EmitEnergy()
    {
        InitCallback();
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

        EndCallBack();
        yield return null;
    }

    IEnumerator EmitSpatioportInfluence()
    {
        InitCallback();


        EndCallBack();
        yield return null;
    }

}
