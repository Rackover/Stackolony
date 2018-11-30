using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityManagement : MonoBehaviour {
    public GridManagement gridManager;
    public MissionManager missionManager;


    public List<BlockLink> affectedBlocks;
    public List<int> distancesToCenter;
    public Vector3Int position;
    public int range;
    public int power;

    public void EmitEnergy()
    {
        foreach (BlockLink block in affectedBlocks)
        {
            Debug.Log("BLOCK " + block.name + " NEEDS " + (block.myBlock.consumption - block.currentPower) + " POWER");
            for (int i = block.myBlock.consumption - block.currentPower; i>0; i--)
            {
                if (power > 0)
                {
                    Debug.Log("BLOCK " + block.name + " IS GETTING POWERED");
                    block.currentPower++;
                    power--;
                }
            }
        }
    }

    public void RemoveEnergy()
    {
        foreach (BlockLink block in affectedBlocks)
        {
            Debug.Log("BLOCK " + block.name + " HAS " + (block.currentPower) + " POWER");
            for (int i = block.currentPower; i > 0; i--)
            {
                if (power > 0)
                {
                    Debug.Log("BLOCK " + block.name + " IS GETTING UNPOWERED");
                    block.currentPower--;
                    power--;
                }
            }
        }
    }
}
