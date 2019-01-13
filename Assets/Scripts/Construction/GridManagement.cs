using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManagement : MonoBehaviour 
{
    public GameManager gameManager;

    //------------VARIABLES PUBLIQUES------------
    [Header("=== MAP SETTINGS ===")][Space(1)]
    [Tooltip("Quelle taille font les cellules en X Y et Z")] public Vector3 cellSize;
    [Tooltip("Quelle hauteur max pour les tours")] public int maxHeight;
    [Tooltip("Hauteur de la lave")] public GameObject lavaPlane;
    [Header("=== PREFABS ===")][Space(1)]
    [Header("Bridge")]
    [Tooltip("Prefab du pont, de la taille (cellSize)")] public GameObject bridgePrefab;
    [Tooltip("Prefab de la fin du pont, de la taille (cellSize-1)/2")] public GameObject bridgeEndPrefab;
    [Tooltip("Prefab du départ du pont, de la taille (cellSize-1)/2")] public GameObject bridgeStartPrefab;
    [Header("=== LISTS ===")][Space(1)]
    [Header("Liste de ponts")]
    public List<GameObject> bridgesList = new List<GameObject>();
    [Header("Liste des batiments")]
    public List<GameObject> buildingsList = new List<GameObject>();
    [Header("Grille de blocs")]
    public Vector3[,,] bridgesGrid;
    public GameObject[,,] grid;

    public Terrain myTerrain; //Terrain sur lequel la grille doit être generée
    public Vector3Int gridSize; //Nombre de cases sur le terrain
    private GameObject gridGameObject; //GameObject contenant la grille

    [HideInInspector]
    public int lavaHeight; //Hauteur de la lave
    
    public enum blockType{ FREE = 0, STORAGE = 1, BRIDGE = 2}

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (!gameManager.IsInGame()) {
           // return;
        }
        //Recuperation du terrain
        if (myTerrain == null) {
            myTerrain = FindObjectOfType<Terrain>();
        }

        //Initialisation des variables statiques
        gridSize.x = Mathf.RoundToInt(myTerrain.terrainData.size.x / cellSize.x);
        gridSize.z = Mathf.RoundToInt(myTerrain.terrainData.size.z / cellSize.z);
        gridSize.y = maxHeight;

        GenerateGrid();

        CalculateLavaHeight();
    }

    private void CalculateLavaHeight()
    {
        lavaHeight = WorldPositionToIndex(lavaPlane.transform.position).y;
    }

    private void GenerateGrid() //Fonction pour générer la grille sur le terrain
    {
        //Initialisation de la variable grille contenant chaque bloc
        grid = new GameObject[gridSize.x, gridSize.y, gridSize.z];
        bridgesGrid = new Vector3[gridSize.x, gridSize.y, gridSize.z];

        //GENERATION DU GAMEOBJECT CONTENANT CHAQUE LAYERS
        gridGameObject = new GameObject(); //Crée le gameobject qui contiendra absolument tout les blocs du jeu (pour trier)
        gridGameObject.name = "Grid";
        gridGameObject.transform.parent = this.transform;
        gridGameObject.transform.localPosition = Vector3.zero;
        Logger.Debug("Generated grid of size "+gridSize.ToString());

        //GENERATION DES GRILLES DE DEBUG
        if (GameManager.instance.DEBUG_MODE) {
            gameManager.gridDebugger.gridManager = this;
            gameManager.gridDebugger.InitAllGrids();
            gameManager.gridDebugger.InitButtons();
        }
    }

    public void DestroyBlock(Vector3Int coordinates)
    {
        if (grid[coordinates.x, coordinates.y, coordinates.z] != null)
        {
            grid[coordinates.x, coordinates.y, coordinates.z].GetComponent<Block>().CallFlags("OnBlockDestroy");
            // Removes object from list and destroys the gameObject
            GameObject target = grid[coordinates.x, coordinates.y, coordinates.z];
            buildingsList.RemoveAll(o => o == target);
            gameManager.sfxManager.PlaySoundLinked("DestroyBlock", target);
            Destroy(target);
        }
        UpdateBlocks(coordinates);
    }

    public float GetDistanceFromGround(Vector3Int coordinates)
    {
        float distanceFromGround = grid[coordinates.x, coordinates.y, coordinates.z].transform.position.y - myTerrain.SampleHeight(grid[coordinates.x, coordinates.y, coordinates.z].transform.position);
        return distanceFromGround;
    }

    //Fait baisser d'un bloc tout les blocs au dessus de la coordonnée actuelle
    public void UpdateBlocks(Vector3Int coordinates)
    {
        if (grid[coordinates.x, coordinates.y, coordinates.z] != null)
        {
            GameObject target = grid[coordinates.x, coordinates.y, coordinates.z];

            for (int i = coordinates.y + 1; i < maxHeight; i++) //Fait descendre d'une case les blocs
            {
                if (grid[coordinates.x, i, coordinates.z] == null)
                {
                    grid[coordinates.x, i - 1, coordinates.z] = null;
                    return;
                }
                else
                {
                    if (GetSlotType(new Vector3Int(coordinates.x, i, coordinates.z),false) == GridManagement.blockType.FREE)
                    {
                        MoveBlock(grid[coordinates.x, i, coordinates.z], new Vector3Int(coordinates.x, i - 1, coordinates.z));
                    } else
                    {
                        grid[coordinates.x, i - 1, coordinates.z] = null;
                        gameManager.errorDisplay.ShowError("Error at update");
                        return;
                    }
                }
            }
        }
    }


    /// <summary>
    /// Safe function to move a block around.
    /// </summary>
    /// <param name="block"></param>
    /// <param name="coordinates"></param>
    public void MoveBlock(GameObject block, Vector3Int coordinates)
    {
        Block link = block.GetComponent<Block>();
        grid[coordinates.x, coordinates.y, coordinates.z] = block;
        link.MoveTo(coordinates);
    }
    
    public Vector3 IndexToWorldPosition(Vector3Int index)
    {
        return new Vector3(
                index.x * cellSize.x + (cellSize.x / 2),
                index.y * cellSize.y + (cellSize.y / 2),
                index.z * cellSize.z + (cellSize.z / 2)
        ) + myTerrain.transform.position;
    }

    public Vector3Int WorldPositionToIndex(Vector3 position)
    {
        position -= myTerrain.transform.position;
        return new Vector3Int(
                (int)Mathf.Round((position.x  - cellSize.x / 2) / cellSize.x),
                (int)Mathf.Round(position.y / cellSize.y),
                (int)Mathf.Round((position.z - cellSize.z / 2)/cellSize.z)
        );
    }

    //Update a block so he touch the ground or the first block encountered (Like if gravity was applied to it)
    public void LayBlock(Block block, Vector2Int coordinates)
    {
        //Position en Y des coordonnées au sol données
        float worldY =
            myTerrain.SampleHeight(
                IndexToWorldPosition(
                    new Vector3Int(coordinates.x, 0, coordinates.y)
                )
            );
        // Index de Y
        int y = WorldPositionToIndex(new Vector3(coordinates.x, worldY, coordinates.y)).y;

        // Candidate coordinates
        Vector3Int coordinates3 = new Vector3Int(coordinates.x, y, coordinates.y);

        if (GetSlotType(coordinates3, true) != GridManagement.blockType.FREE)
        {
            return;
        }


        if (grid[coordinates3.x, coordinates3.y, coordinates3.z] != null)
        {
            // The slot is occupied - let's see if we can lay our block ontop
            for (int i = coordinates3.y; i < gridSize.y - 1; i++)
            {
                if (GetSlotType(new Vector3Int(coordinates3.x, i, coordinates3.z), true) != GridManagement.blockType.FREE)
                {
                    continue;
                }
                else if (grid[coordinates3.x, i, coordinates3.z] == null)
                {
                    coordinates3.y = i;
                    break;
                }
            }
        }
        MoveBlock(block.gameObject, coordinates3);
    }

    /// <summary>
    /// Lays a block at X Y coordinates on the grid at the highest possible point 
    /// (ontop of a building or on the ground)
    /// </summary>
    /// <param name="blockId"></param>
    /// <param name="coordinates"></param>
    public void LayBlock(int blockId, Vector2Int coordinates)
    {
        GameObject newBlock = SpawnBlock(blockId, new Vector3Int(coordinates.x,0,coordinates.y));
        LayBlock(newBlock.GetComponent<Block>(), coordinates);
    }

    public GameObject SpawnBlock(int blockId, Vector3Int coordinates)
    {
        GameObject newBlock = CreateBlockFromId(blockId);
        MoveBlock(newBlock, coordinates);
        Logger.Debug("Spawned block : " + newBlock.GetComponent<Block>().block.name + " with prefab " + GameManager.instance.library.blockPrefab.name + " at position "+coordinates.ToString());
        return newBlock;
    }

    public GameObject CreateBlockFromId(int blockId)
    {
        BlockScheme block = GameManager.instance.library.GetBlockByID(blockId);
        if (block == null) { Logger.Warn("BlockScheme index not found - index : " + blockId); return null; }

        GameObject newBlock = Instantiate(GameManager.instance.library.blockPrefab);
        Block newBlockLink = newBlock.GetComponent<Block>();
        newBlockLink.block = block;
        newBlockLink.LoadBlock();
        newBlockLink.container.OpenContainer();

        return newBlock;
    }

    public blockType GetSlotType(Vector3Int coordinates, bool displayErrorMessages)
    {
        GameObject objectFound = grid[coordinates.x, coordinates.y, coordinates.z];
        if (objectFound != null)
        {
            switch (objectFound.tag)
            {
                case "StorageBay":
                    if (displayErrorMessages)
                        gameManager.errorDisplay.ShowError("You can't build over the storage bay");
                    return blockType.STORAGE;
                case "Bridge":
                    if (displayErrorMessages)
                        gameManager.errorDisplay.ShowError("You can't build over a bridge");
                    return blockType.BRIDGE;
                default:
                    break;
            }
        }
        return blockType.FREE;
    }

    //Fonction a appelé pour déplacer un pont à une nouvelle position Y
    public void UpdateBridgePosition(BridgeInfo bridgeInfo, int newYPosition)
    {
        List<Vector3Int> newBridgePositions = new List<Vector3Int>();
        foreach (Vector3Int partPos in bridgeInfo.allBridgePositions)
        {
            //Pour chaque partie du pont on vérifie si leur destination est disponible
            if (grid[partPos.x,partPos.y,partPos.z].tag == "Bridge")
            {
                GameObject blockToCheck = grid[partPos.x, newYPosition, partPos.z];

                //S'il n' a rien sous le pont, on le fait descendre
                if (blockToCheck == null)
                {
                    newBridgePositions.Add(new Vector3Int(partPos.x, newYPosition, partPos.z));
                    grid[partPos.x, newYPosition, partPos.z] = grid[partPos.x, partPos.y, partPos.z];
                    grid[partPos.x, partPos.y, partPos.z] = null;
                }
                else
                {
                    //Si le bloc tombe sur un autre pont, on détruit le pont du bas, puis on met à jour ses positions
                    if (blockToCheck.tag == "Bridge")
                    {
                        DestroyBridge(blockToCheck);

                        newBridgePositions.Add(new Vector3Int(partPos.x, newYPosition, partPos.z));
                        grid[partPos.x, newYPosition, partPos.z] = grid[partPos.x, partPos.y, partPos.z];
                        grid[partPos.x, partPos.y, partPos.z] = null;

                    }
                    else if (blockToCheck.GetComponent<Block>() != null)
                    //Si le pont tombe sur un bloc, on détruit le pont et on reforme 2 ponts |||||||| ATTENTION POUR L'INSTANT LE SCRIPT N'A PAS CETTE FEATURE IMPLANTéE (A la place le pont se détruit)
                    {
                       DestroyBridge(grid[partPos.x, partPos.y, partPos.z]);
                        return;
                    }
                }
            }
        }
        //S'il le pont n'a pas été détruit ou modifié, on retransmet la nouvelle liste des positions de chaque partie du pont à l'objet bridgeInfo
        bridgeInfo.allBridgePositions = newBridgePositions.ToArray();
        bridgeInfo.destination.y = newYPosition;
        bridgeInfo.origin.y = newYPosition;
    }

    /// <summary>
    /// Bridges two building and returns the parent bridge GameObject
    /// </summary>
    /// <param name="blockA"></param>
    /// <param name="blockB"></param>
    public GameObject CreateBridge(Block blockA, Block blockB)
    {
        //Calcul de la longueur du pont et de l'orientation du pont
        int bridgeLength = 0;

        Vector2Int direction = Vector2Int.zero;

        if (blockA.gridCoordinates.x == blockB.gridCoordinates.x)
        {
            bridgeLength = Mathf.Abs(blockA.gridCoordinates.z - blockB.gridCoordinates.z) - 1;
            direction.x = 0;
            if (blockA.gridCoordinates.z - blockB.gridCoordinates.z > 0)
            {
                direction.y = -1;
            }
            else
            {
                direction.y = 1;
            }
        }
        else
        {
            bridgeLength = Mathf.Abs(blockA.gridCoordinates.x - blockB.gridCoordinates.x) - 1;
            direction.y = 0;
            if (blockA.gridCoordinates.x - blockB.gridCoordinates.x > 0)
            {
                direction.x = -1;
            }
            else
            {
                direction.x = 1;
            }
        }

        //VERIFICATION DE SI ON PEUT CREER OU NON LE PONT
        for (int i = 1; i <= bridgeLength; i++)
        {
            Vector3Int _posToCheck = new Vector3Int();
                _posToCheck.x = blockA.gridCoordinates.x + (i * direction.x);
                _posToCheck.y = blockA.gridCoordinates.y;
                _posToCheck.z = blockA.gridCoordinates.z + (i * direction.y);

            if (GetSlotType(new Vector3Int(_posToCheck.x, _posToCheck.y, _posToCheck.z), true) != GridManagement.blockType.FREE)
            {
                return null;
            }

            if (grid[_posToCheck.x,_posToCheck.y,_posToCheck.z] != null)
            {
                return null;
            }
        }

        //Creation du gameObject contenant le pont
        GameObject parentBridgeGameObject = new GameObject();
        parentBridgeGameObject.name = "Bridge";
        parentBridgeGameObject.tag = "Bridge";
        parentBridgeGameObject.transform.parent = blockA.transform;
        parentBridgeGameObject.transform.localPosition = new Vector3(0, 0, 0);
        parentBridgeGameObject.transform.localScale = new Vector3(1, 1, 1);

        // Adding origin/destination info in the parent bridge game object
        BridgeInfo bridgeInfo = parentBridgeGameObject.AddComponent<BridgeInfo>();
        bridgeInfo.origin = blockA.gridCoordinates;
        bridgeInfo.destination = blockB.gridCoordinates;
        blockA.bridge = parentBridgeGameObject;

        //Ajout de chaque partie du pont dans la grille grid[] et dans le component bridgeInfo
        bridgeInfo.allBridgePositions = new Vector3Int[bridgeLength];
        for (int i = 1; i <= bridgeLength; i++)
        {
            Vector3Int _posToCheck = new Vector3Int();
                _posToCheck.x = blockA.gridCoordinates.x + (i * direction.x);
                _posToCheck.y = blockA.gridCoordinates.y;
                _posToCheck.z = blockA.gridCoordinates.z + (i * direction.y);

            grid[_posToCheck.x, _posToCheck.y, _posToCheck.z] = parentBridgeGameObject;

            bridgeInfo.allBridgePositions[i-1] = _posToCheck;
        }




        //Création de chaque parties du pont
        GameObject firstBridgePart = null;

        for (int i = 1; i <= bridgeLength + 2; i++) {
            GameObject newBridgePart = null;

            //Création de la premiere partie du pont (de taille (cellsize-1)/2)
            if (i == 1) {
                newBridgePart = Instantiate(bridgeStartPrefab, parentBridgeGameObject.transform);
                newBridgePart.transform.localPosition = new Vector3(0, 0, 0);
                newBridgePart.transform.LookAt(blockB.transform);
                firstBridgePart = newBridgePart;
            }

            //Création de toutes les parties intérieures du pont (de la taille d'une cellule)
            else if (i == bridgeLength + 2) {
                newBridgePart = Instantiate(bridgeEndPrefab, parentBridgeGameObject.transform);
                newBridgePart.transform.localPosition = new Vector3(
                    (((cellSize.x - 1) / 2) * direction.x) + (cellSize.x * (i - 2)) * direction.x + (0.5f * direction.x), 
                    0, 
                    (((cellSize.z - 1) / 2) * direction.y) + (cellSize.z * (i - 2)) * direction.y + (0.5f * direction.y)
                );
                newBridgePart.transform.localRotation = firstBridgePart.transform.localRotation;
            }

            //Création de la derniere partie du pont (de taille (cellsize-1)/2)
            else {
                newBridgePart = Instantiate(bridgePrefab, parentBridgeGameObject.transform);
                newBridgePart.transform.localPosition = new Vector3(
                    (((cellSize.x - 1) / 2) * direction.x) + (cellSize.x * (i - 2)) * direction.x + (0.5f * direction.x), 
                    0, 
                    (((cellSize.z - 1) / 2) * direction.y) + (cellSize.z * (i - 2)) * direction.y + (0.5f * direction.y)
                );
                newBridgePart.transform.localRotation = firstBridgePart.transform.localRotation;
            }
            newBridgePart.name = "Bridge part " + i;
            newBridgePart.transform.parent = parentBridgeGameObject.transform;
        }
        bridgesList.Add(parentBridgeGameObject);
        bridgesGrid[
            blockA.gridCoordinates.x,
            blockA.gridCoordinates.x,
            blockA.gridCoordinates.x
        ] = blockB.gridCoordinates;

        //Joue le son
        gameManager.sfxManager.PlaySoundLinked("CreateBridge", parentBridgeGameObject);

        //Update the system
        gameManager.systemManager.UpdateSystem();

        return parentBridgeGameObject;
    }

    /// <summary>
    /// Destroy bridges (give the parent bridge object as argument)
    /// </summary>
    /// <param name="bridgeObject"></param>
    public void DestroyBridge(GameObject bridgeObject)
    {
        int subParts = bridgeObject.transform.childCount;

        for (int i = 0; i < subParts; i++) {
            Transform subPart = bridgeObject.transform.GetChild(i);
            Destroy(subPart.gameObject);
        }

        BridgeInfo bridgeInfo = bridgeObject.GetComponent<BridgeInfo>();

        // Removes bridge from the BridgesList
        bridgesList.RemoveAll(o=>o == bridgeObject);
        bridgesGrid[bridgeInfo.origin.x, bridgeInfo.origin.y, bridgeInfo.origin.z] = new Vector3();

        // Remove the bridge from the grid
        foreach (Vector3Int subpartPos in bridgeInfo.allBridgePositions)
        {
            grid[subpartPos.x, subpartPos.y, subpartPos.z] = null;
        }
        gameManager.sfxManager.PlaySoundLinked("DestroyBlock", bridgeObject);
        Destroy(bridgeObject);
        //Update the system
        gameManager.systemManager.UpdateSystem();
    }
}
