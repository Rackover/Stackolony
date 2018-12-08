using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityManagement : MonoBehaviour {
    public GridManagement gridManager;
    public MissionManager missionManager;
    public SystemReferences systemReferences;

    public GameObject prefabRemoving;
    public GameObject prefabEmitting;

    public MissionManager.Mission mission;

    public int activeCoroutineRelatedToPower;

    IEnumerator EmitEnergy()
    {
        activeCoroutineRelatedToPower++;
        yield return new WaitForSeconds(0.05f);
        Debug.Log("EMITTING POWER");
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
                    block.ChangePower(1);
                    myMission.power--;
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
        activeCoroutineRelatedToPower--;
        if (activeCoroutineRelatedToPower == 0) {
            if (systemReferences != null) {
                systemReferences.UpdateBlocksRequiringPower();
            } else {
                Debug.LogWarning("No reference to system found");
            }
        }
        yield return null;
    }
}
