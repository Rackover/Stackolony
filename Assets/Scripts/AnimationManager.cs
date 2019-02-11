using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour {

    public float towerElevationTime = 2f;
    public float towerDelevationTime = 4f;
    public float towerElevationAmount = 2f;
    public void ElevateTower(Vector3Int coordinates, float time = -1)
    {
        if (time < 0) { time = towerElevationTime; }
        GridManagement gridManager = GameManager.instance.gridManagement;
        for (int i = coordinates.y; i < gridManager.gridSize.y; i++)
        {
            GameObject checkedObj = gridManager.grid[coordinates.x, i, coordinates.z];
            if (checkedObj != null)
            {
                Block foundBlock = checkedObj.GetComponent<Block>();
                if (foundBlock!= null)
                {
                    if (!foundBlock.scheme.isMovable) { return; }
                    ElevateBlock(foundBlock, towerElevationAmount, time);
                }
            }
        }
    }

    public void EndElevateTower(Vector2Int coordinates, float time = -1)
    {
        if (time < 0) { time = towerDelevationTime; }
        GridManagement gridManager = GameManager.instance.gridManagement;
        for (int i = 0; i < gridManager.gridSize.y; i++)
        {
            GameObject checkedObj = gridManager.grid[coordinates.x, i, coordinates.y];
            if (checkedObj != null)
            {
                Block foundBlock = checkedObj.GetComponent<Block>();
                if (foundBlock != null)
                {
                    EndElevateBlock(foundBlock, time);
                }
            }
        }
    }

    public void ElevateBlock(Block block, float elevationAmount, float time)
    {
        StartCoroutine(ElevateBlockC(block, elevationAmount, time));
    }

    public void EndElevateBlock(Block block, float time)
    {
        StartCoroutine(EndElevateC(block, time));
    }

    IEnumerator ElevateBlockC(Block block, float elevationAmount, float time)
    {
        GameObject visuals = block.visuals.gameObject;
        Vector3 startingPosition = block.transform.position;
        Vector3 endPosition = block.transform.position + new Vector3(0, elevationAmount, 0);
        for (float i = 0; i < time; i+=Time.deltaTime)
        {
            yield return null;
            visuals.transform.position = Vector3.Lerp(startingPosition, endPosition, i / time);
        }
        visuals.transform.position = endPosition;
        yield return null;
    }

    IEnumerator EndElevateC(Block block, float time)
    {
        GameObject visuals = block.visuals.gameObject;
        Vector3 startingPosition = visuals.transform.position;
        Vector3 endPosition = block.transform.position;
        for (float i = 0; i < time; i+=Time.deltaTime)
        {
            yield return new WaitForEndOfFrame();
            visuals.transform.position = Vector3.Lerp(startingPosition, endPosition, i / time);
        }
        visuals.transform.position = block.transform.position;
        yield return null;
    }
}
