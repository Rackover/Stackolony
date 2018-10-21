using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockLink : MonoBehaviour {
    public Block linkedBlock;
    public Vector3Int gridCoordinates = new Vector3Int(0, 0, 0);
    public int positionInTower; //0 = tout en bas
    public GameObject[] blocks;

    public void Start()
    {
        // TEMP
        Transform newBlockMesh = Instantiate(blocks[Random.Range(0, blocks.Length)], transform.position, Quaternion.identity, transform).transform;
        newBlockMesh.rotation = Quaternion.Euler(-90f, 0f, 0f);
    }

    public void MoveToMyPosition() //Deplace le bloc à sa position supposée
    {
        StartCoroutine(MoveToPosition(0.3f));
    }

    public IEnumerator MoveToPosition(float time) //Coroutine pour déplacer le cube vers sa position
    {
        float elapsedTime = 0;
        Vector3 startingPos = transform.position;
        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(startingPos, new Vector3(gridCoordinates.x * GridManagement.cellSizeStatic + 0.5f * GridManagement.cellSizeStatic, 0.5f + gridCoordinates.y, gridCoordinates.z * GridManagement.cellSizeStatic + 0.5f * GridManagement.cellSizeStatic), (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
}