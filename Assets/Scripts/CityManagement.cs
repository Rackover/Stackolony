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
        missionManager.EndMission(myMission);

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
