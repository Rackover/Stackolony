using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityManagement : MonoBehaviour {
    public GridManagement gridManager;
    public MissionManager missionManager;

    public GameObject prefabRemoving;
    public GameObject prefabEmitting;

    public int missionID;


    IEnumerator EmitEnergy()
    {
        int checkedMissionID = missionID;
        MissionManager.Mission myMission = missionManager.missionList[checkedMissionID];
        foreach (BlockLink block in myMission.blocksFound)
        {
            for (int i = block.myBlock.consumption - block.currentPower; i > 0; i--)
            {
                if (myMission.power > 0)
                {
                    GameObject energyFeedback = Instantiate(prefabEmitting);
                    energyFeedback.transform.position = block.transform.position;
                    Destroy(energyFeedback, 2);
                    block.currentPower++;
                    myMission.power--;
                }
            }
        }
        yield return null;
    }

    IEnumerator RemoveEnergy()
    {
        int checkedMissionID = missionID;
        MissionManager.Mission myMission = missionManager.missionList[checkedMissionID];
        foreach (BlockLink block in myMission.blocksFound)
        {
            for (int i = block.currentPower; i > 0; i--)
            {
                if (myMission.power > 0)
                {
                    GameObject energyFeedback = Instantiate(prefabRemoving);
                    energyFeedback.transform.position = block.transform.position;
                    Destroy(energyFeedback, 2);
                    block.currentPower--;
                    myMission.power--;
                }
            }
        }
        yield return null;
    }
}
