using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityManagement : MonoBehaviour 
{
    public GameObject prefabRemoving;
    public GameObject prefabEmitting;
    public MissionManager.Mission mission;
    public int activeCoroutineRelatedToPower;

    IEnumerator EmitOccupators()
    {
        MissionManager.Mission myMission = mission;
        foreach (BlockLink blocklink in myMission.blocksFound)
        {
            House house = blocklink.GetComponent<House>();
            Occupator linkedOccupator = GameManager.instance.gridManagement.grid[myMission.position.x, myMission.position.y, myMission.position.z].GetComponent<Occupator>();
            house.occupatorsInRange.Add(linkedOccupator);
        }
        GameManager.instance.missionManager.EndMission(myMission);
        yield return null;
    }

    IEnumerator EmitEnergy()
    {
        activeCoroutineRelatedToPower++;
        MissionManager.Mission myMission = mission;
        foreach (BlockLink blocklink in myMission.blocksFound)
        {
            for (int i = blocklink.block.consumption - blocklink.currentPower; i > 0; i--)
            {
                if (myMission.power > 0)
                {
                    GameObject energyFeedback = Instantiate(prefabEmitting);
                    energyFeedback.transform.position = blocklink.transform.position;
                    Destroy(energyFeedback, 2);
                    blocklink.ChangePower(1);
                    myMission.power--;
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        GameManager.instance.missionManager.EndMission(myMission);

        activeCoroutineRelatedToPower--;
        if (activeCoroutineRelatedToPower == 0) {
            if (GameManager.instance.systemManager != null) {
                GameManager.instance.systemManager.UpdateBlocksRequiringPower();
            } else {
                Debug.LogWarning("No reference to system found");
            }
        }
        yield return null;
    }
}
