using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockLink : MonoBehaviour {

    [Header("Referencies")]
    public BoxCollider collider;
    public Container myContainer;
    private Transform newBlockMesh;

    public Vector3Int gridCoordinates = new Vector3Int(0, 0, 0);
    [HideInInspector] public int positionInTower; //0 = tout en bas

    // TEMPORARY ASSET GENERATION
    public Block[] blocks;
    [HideInInspector] public GridManagement gridManager;

    public void Initialize()
    {
        if (collider == null) collider = gameObject.GetComponent<BoxCollider>();
        gameObject.GetComponent<BlockBehaviour>().myBlock = Instantiate(blocks[Random.Range(0, blocks.Length)]);
    }

    public void ToggleVisuals()
    {
        if (newBlockMesh.gameObject.activeSelf == true)
            newBlockMesh.gameObject.SetActive(false);
        else
            newBlockMesh.gameObject.SetActive(true);
    }

    void Start()
    {
        gridManager = FindObjectOfType<GridManagement>();
    }

    public void MoveToSpecificPosition()
    {

    }

    public void MoveToMyPosition() //Deplace le bloc à sa position supposée
    {
        //Déplace aussi le pont qui lui est attaché s'il y en a un
        if (transform.Find("Bridge") != null)
        {
            BridgeInfo bridgeInfo = transform.Find("Bridge").GetComponent<BridgeInfo>();
            if (bridgeInfo != null)
            {
                gridManager.updateBridgePosition(bridgeInfo, gridCoordinates.y);
            }
        }
        StartCoroutine(MoveToPosition(0.1f));
    }

    public IEnumerator MoveToPosition(float time) //Coroutine pour déplacer le cube vers sa position
    {
        float elapsedTime = 0;
        Vector3 startingPos = transform.position;
        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(
                startingPos, 
                new Vector3(
                    gridCoordinates.x * gridManager.cellSize + 0.5f * gridManager.cellSize, 
                    0.5f + gridCoordinates.y, 
                    gridCoordinates.z * gridManager.cellSize + 0.5f * gridManager.cellSize),
                elapsedTime / time
            );
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.position = new Vector3(
        gridCoordinates.x * gridManager.cellSize + 0.5f * gridManager.cellSize,
        0.5f + gridCoordinates.y,
        gridCoordinates.z * gridManager.cellSize + 0.5f * gridManager.cellSize);
        yield return null;
    }
}