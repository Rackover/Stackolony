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
        Debug.Log("ENERGY EMISSION FROM " + position + " AFFECTING " + affectedBlocks.Count + " BLOCKS");
        foreach (BlockLink block in affectedBlocks)
        {
            for (int i = block.myBlock.consumption - block.currentPower; i>0; i--)
            {
                if (power > 0)
                {
                    block.currentPower++;
                    power--;
                }
            }
        }
    }

    public void RemoveEnergy()
    {
        Debug.Log("REMOVING ENERGY EMITTED FROM " + position + " AFFECTING " + affectedBlocks.Count + " BLOCKS");
        foreach (BlockLink block in affectedBlocks)
        {
            for (int i = block.currentPower; i > 0; i--)
            {
                if (power > 0)
                {
                    block.currentPower--;
                    power--;
                }
            }
        }
    }
}
