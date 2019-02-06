using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour {

    public float towerElevationSpeed = 2f;
    public float towerElevationAmount = 1.2f;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Elevating");
            ElevateTower(GameManager.instance.cursorManagement.posInGrid);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Delevating");
            EndElevateTower(new Vector2Int(GameManager.instance.cursorManagement.posInGrid.x, GameManager.instance.cursorManagement.posInGrid.z));
        }
    }

    public void ElevateTower(Vector3Int coordinates)
    {
        GridManagement gridManager = GameManager.instance.gridManagement;
        for (int i = coordinates.y; i < gridManager.gridSize.y; i++)
        {
            GameObject checkedObj = gridManager.grid[coordinates.x, i, coordinates.z];
            if (checkedObj != null)
            {
                Block foundBlock = checkedObj.GetComponent<Block>();
                if (foundBlock!= null)
                {
                    ElevateBlock(foundBlock, towerElevationAmount, towerElevationSpeed);
                }
            }
        }
    }

    public void EndElevateTower(Vector2Int coordinates)
    {
        GridManagement gridManager = GameManager.instance.gridManagement;
        for (int i = 0; i < gridManager.gridSize.y; i++)
        {
            GameObject checkedObj = gridManager.grid[coordinates.x, i, coordinates.y];
            if (checkedObj != null)
            {
                Block foundBlock = checkedObj.GetComponent<Block>();
                if (foundBlock != null)
                {
                    EndElevateBlock(foundBlock, towerElevationSpeed);
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
        Debug.Log("Elevating 2");
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
