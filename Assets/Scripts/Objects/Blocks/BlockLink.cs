using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockLink : MonoBehaviour {

    [Header("Referencies")]
    [Tooltip("Linked ScriptableObject")] public Block linkedBlock;
    public BoxCollider collider;

    [HideInInspector] public Vector3Int gridCoordinates = new Vector3Int(0, 0, 0);
    [HideInInspector] public int positionInTower; //0 = tout en bas

    // TEMPORARY ASSET GENERATION
    public GameObject[] blocks;
    GridManagement gridManager;

    void Awake()
    {
        if(collider == null) collider = gameObject.GetComponent<BoxCollider>();
    }

    void Start()
    {
        // TEMP
        gridManager = FindObjectOfType<GridManagement>();
        Transform newBlockMesh = Instantiate(blocks[Random.Range(0, blocks.Length)], transform.position, Quaternion.identity, transform).transform;
        newBlockMesh.rotation = Quaternion.Euler(-90f, 0f, 0f);
    }

    public void MoveToMyPosition() //Deplace le bloc à sa position supposée
    {
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
        yield return null;
    }
}