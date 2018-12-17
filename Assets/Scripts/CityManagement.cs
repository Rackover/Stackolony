using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityManagement : MonoBehaviour 
{
    GameManager gameManager;

    public GameObject prefabRemoving;
    public GameObject prefabEmitting;
    public MissionManager.Mission mission;
    public int activeCoroutineRelatedToPower;

    void Awake() 
    {
        gameManager = FindObjectOfType<GameManager>();
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
        yield return new WaitForSeconds(0f);
        activeCoroutineRelatedToPower--;
        if (activeCoroutineRelatedToPower == 0) {
            if (gameManager.systemReferences != null) {
                gameManager.systemReferences.UpdateBlocksRequiringPower();
            } else {
                Debug.LogWarning("No reference to system found");
            }
        }
        yield return null;
    }
}
