using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityManagement : MonoBehaviour {
    public GridManagement gridManager;
    public MissionManager missionManager;

    public GameObject prefabRemoving;
    public GameObject prefabEmitting;

    public MissionManager.Mission mission;


    IEnumerator EmitEnergy()
    {

        MissionManager.Mission myMission = mission;
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
                    block.isConsideredUnpowered = false;
                    myMission.power--;
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }
        yield return null;
    }

    IEnumerator RemoveEnergy()
    {
        MissionManager.Mission myMission = mission;
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
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }
        yield return null;
    }
}
