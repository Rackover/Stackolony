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
        Debug.Log("STARTING ENERGY EMISSION");
        Debug.Log(activeCoroutineRelatedToPower);
        MissionManager.Mission myMission = mission;
        int count = 0;
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
                  //  yield return new WaitForSeconds(0.01f);
                    Debug.Log("EMITTING ENERGY");
                }
            }
            count++;
        }
       //yield return new WaitForSeconds(0.7f);
        activeCoroutineRelatedToPower--;
        if (activeCoroutineRelatedToPower == 0) {
            if (systemReferences != null) {
                systemReferences.UpdateBlocksRequiringPower();
                Debug.Log("Calculs finished");
            } else {
                Debug.LogWarning("No reference to system found");
            }
        }
        yield return null;
    }
}
