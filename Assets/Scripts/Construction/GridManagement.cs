using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridManagement : MonoBehaviour 
{
    public GameManager gameManager;

    //------------VARIABLES PUBLIQUES------------
    [Header("=== MAP SETTINGS ===")][Space(1)]
    [Tooltip("Quelle taille font les cellules en X Y et Z")] public Vector3 cellSize;
    [Tooltip("Quelle hauteur max pour les tours")] public int maxHeight;
    [Tooltip("Hauteur minimale pour construire")] public int minHeight;
    [Header("=== PREFABS ===")][Space(1)]
    [Header("Bridge")]
    [Tooltip("Prefab du pont, de la taille (cellSize)")] public GameObject bridgePrefab;
    [Tooltip("Prefab de la fin du pont, de la taille (cellSize-1)/2")] public GameObject bridgeEndPrefab;
    [Tooltip("Prefab du départ du pont, de la taille (cellSize-1)/2")] public GameObject bridgeStartPrefab;
    [Header("=== LISTS ===")][Space(1)]
    [Header("Liste de ponts")]
    public List<GameObject> bridgesList = new List<GameObject>();
    [Header("Grille de blocs")]
    public Vector3[,,] bridgesGrid;
    public GameObject[,,] grid;

    [HideInInspector] public Vector3Int spawnPoint;
    public Terrain myTerrain; //Terrain sur lequel la grille doit être generée
    public Vector3Int gridSize; //Nombre de cases sur le terrain
    private GameObject gridGameObject; //GameObject contenant la grille

    public List<Vector2Int> buildablePositions = new List<Vector2Int>(); // List des positions disponible pour poser des block par rapport au terrain
    
    public enum blockType{ FREE = 0, BRIDGE = 1}


    public void InitializeGridManager()
    {
        gameManager = FindObjectOfType<GameManager>();
        myTerrain = FindObjectOfType<Terrain>();


        //Initialisation des variables statiques
        gridSize.x = Mathf.RoundToInt(myTerrain.terrainData.size.x / cellSize.x);
        gridSize.z = Mathf.RoundToInt(myTerrain.terrainData.size.z / cellSize.z);
        gridSize.y = maxHeight+1;
        GenerateGrid();
        GenerateBuildablePositions();


        // Find or Set the Spawn of the Spatioport
        Spawn spawn = FindObjectOfType<Spawn>();
        if(spawn == null)
        {
            Vector2Int randPos = GetRandomCoordinates();
            spawnPoint = new Vector3Int(randPos.x, 0, randPos.y);
        }
        else
        {
            GameObject spawnObject = spawn.gameObject;
            spawnPoint = WorldPositionToIndex(spawnObject.transform.position);
            Destroy(spawnObject);
        }

        // Init camera to spawn for cinematic purposes
        CameraController camera = FindObjectOfType<CameraController>();
        if (camera != null) {
            Vector3 spawnPosition = IndexToWorldPosition(spawnPoint);
            camera.transform.position = new Vector3(spawnPosition.x, camera.transform.position.y, spawnPosition.z);
            //camera.SetCameraPositionAndRotation(camera.editorOnlyDummy.transform.position, camera.editorOnlyDummy.transform.rotation);
        }

        buildablePositions.Remove(new Vector2Int(spawnPoint.x, spawnPoint.z));
    }

    private void GenerateBuildablePositions()
    {
        for(int x = 0; x < grid.GetLength(0); x++)
        {
            for(int z = 0; z < grid.GetLength(2); z++)
            {
                if(myTerrain.SampleHeight(IndexToWorldPosition(new Vector3Int(x, 0, z))) > minHeight)
                {
                    buildablePositions.Add(new Vector2Int(x, z));
                }
            }
        }
    }

    public Vector2Int GetRandomCoordinates()
    {
        return buildablePositions[Mathf.FloorToInt(buildablePositions.Count*UnityEngine.Random.value)];
    }

    public bool IsPositionFree(Vector2Int pos)
    {
        for(int i = 0; i < maxHeight; i++)
        {
            if(grid[pos.x, i, pos.y] != null) return false;
        }
        return true;
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

    }

    public void UpdateGridSystems()
    {
        Logger.Debug("Updating system - Grid Update");
        StartCoroutine(GameManager.instance.systemManager.OnGridUpdate());
    }

    /// <summary>
    /// Correct function to destroy a building and remove all of its references
    /// </summary>
    /// <param name="coordinates"></param>
    public void DestroyBlock(Vector3Int coordinates)
    {
        if (grid[coordinates.x, coordinates.y, coordinates.z] != null)
        {
            if (grid[coordinates.x, coordinates.y, coordinates.z].GetComponent<Block>() == null) {
                return;
            }
            if (!grid[coordinates.x, coordinates.y, coordinates.z].GetComponent<Block>().scheme.isDestroyable) {
                return;
            }
            grid[coordinates.x, coordinates.y, coordinates.z].GetComponent<Block>().CallFlags("OnBlockDestroy");

            // Removes object from list and destroys the gameObject
            GameObject target = grid[coordinates.x, coordinates.y, coordinates.z];
            SystemManager systemManager = GameManager.instance.systemManager;
            systemManager.RemoveBuilding(target);
            target.GetComponent<Block>().UnloadBlock();
            GameManager.instance.soundManager.Play("DestroyBlock");
            Destroy(target);
        }
        UpdateBlocks(coordinates);
        UpdateGridSystems();
    }

    public void DestroyBlock(Block block)
    {
        grid[block.gridCoordinates.x, block.gridCoordinates.y, block.gridCoordinates.z] = null;
        block.CallFlags("OnBlockDestroy");

        // Removes object from list and destroys the gameObject
        GameObject target = grid[block.gridCoordinates.x, block.gridCoordinates.y, block.gridCoordinates.z];
        SystemManager systemManager = GameManager.instance.systemManager;
        systemManager.RemoveBuilding(target);
        block.UnloadBlock();
        GameManager.instance.soundManager.Play("DestroyBlock");
        Destroy(block.gameObject);
        UpdateBlocks(block.gridCoordinates);
        UpdateGridSystems();
    }

    //Return true if a block is placable here, false if it isn't
    public bool IsPlaceable(Vector3Int coordinates, bool shouldDisplayInformation, BlockScheme scheme) {
        
        if (coordinates.x < 0 || coordinates.y < 0 || coordinates.z < 0)
        {
            if (shouldDisplayInformation) { GameManager.instance.cursorManagement.CursorError("cannotBuildOutOfMap"); }
            return false;
        }
        if (coordinates.y > maxHeight)
        {
            if (shouldDisplayInformation) { GameManager.instance.cursorManagement.CursorError("maxHeightReached"); }
            return false;
        }
        Vector3Int groundPosition = GetLowestFreeSlot(new Vector2Int(coordinates.x, coordinates.z));
        if (groundPosition.y < minHeight)
        {
            if (shouldDisplayInformation) { GameManager.instance.cursorManagement.CursorError.Invoke("cannotBuildOnWater"); }
            return false;
        }
        if (groundPosition.y >= maxHeight)
        {
            if (shouldDisplayInformation) { GameManager.instance.cursorManagement.CursorError("maxHeightReached"); }
            return false;
        }
        if (GetSlotType(groundPosition) != blockType.FREE)
        {
            if (shouldDisplayInformation) { GameManager.instance.cursorManagement.CursorError.Invoke("cannotBuildHere"); }
            return false;
        }
        GameObject gameObjectUnderPos = grid[groundPosition.x, groundPosition.y - 1, groundPosition.z];
        if (gameObjectUnderPos != null)
        {
            if (gameObjectUnderPos.GetComponent<BridgeInfo>() != null)
            {
                if (shouldDisplayInformation) { GameManager.instance.cursorManagement.CursorError.Invoke("cannotBuildOverBridge"); }
                return false;
            }
            Block blockUnderPos = gameObjectUnderPos.GetComponent<Block>();
            if (blockUnderPos != null)
            {
                if (!blockUnderPos.scheme.canBuildAbove && (coordinates.y != groundPosition.y-1))
                {
                    if (shouldDisplayInformation) { GameManager.instance.cursorManagement.CursorError.Invoke("cannotBuildAboveThis"); }
                    return false;
                }
                if (!blockUnderPos.scheme.isMovable && coordinates.y == groundPosition.y-1)
                {
                    if (shouldDisplayInformation) { GameManager.instance.cursorManagement.CursorError.Invoke("cannotBuildHere"); }
                    return false;
                }
            }
        }
        GameObject gameObjectOnPos = grid[coordinates.x, coordinates.y, coordinates.z];
        if (gameObjectOnPos != null)
        {
            if (gameObjectOnPos.GetComponent<BridgeInfo>() != null)
            {
                if (shouldDisplayInformation) { GameManager.instance.cursorManagement.CursorError.Invoke("cannotBuildOverBridge"); }
                return false;
            }
            Block blockOnPos = gameObjectOnPos.GetComponent<Block>();
            if (blockOnPos != null && !scheme.canBuildAbove)
            {
                if (shouldDisplayInformation) { GameManager.instance.cursorManagement.CursorError.Invoke("cannotPlaceThisBlockHere"); }
                return false;
            }
        }
        return true;
    }


    //Fait baisser d'un bloc tout les blocs au dessus de la coordonnée actuelle
    public void UpdateBlocks(Vector3Int coordinates)
    {
        if (coordinates.x < 0 || coordinates.y < 0 || coordinates.z < 0) { return; }
        if (grid[coordinates.x, coordinates.y, coordinates.z] != null)
        {
            for (int i = coordinates.y + 1; i <= maxHeight; i++) //Fait descendre d'une case les blocs
            {
                if (grid[coordinates.x, i, coordinates.z] == null)
                {
                    grid[coordinates.x, i - 1, coordinates.z] = null;
                    return;
                }
                else
                {
                    if (GetSlotType(new Vector3Int(coordinates.x, i, coordinates.z)) == GridManagement.blockType.FREE)
                    {
                        MoveBlock(grid[coordinates.x, i, coordinates.z], new Vector3Int(coordinates.x, i - 1, coordinates.z));
                    } else
                    {
                        grid[coordinates.x, i - 1, coordinates.z] = null;
                        Logger.Error("An error happened while moving blocks down the grid from " + coordinates + " to " + (new Vector3Int(coordinates.x, i - 1, coordinates.z)));
                        return;
                    }
                }
            }
            UpdateGridSystems();
        }
    }

    public void InsertBlockAtPosition(Block block, Vector3Int coordinates)
    {
        if (grid[coordinates.x, coordinates.y, coordinates.z] != null)
        {
            for (int i = maxHeight; i >= coordinates.y; i--)
            {
                if (grid[coordinates.x, i, coordinates.z] != null)
                {
                    if (GetSlotType(new Vector3Int(coordinates.x, i+1, coordinates.z)) == GridManagement.blockType.FREE)
                    {
                        MoveBlock(grid[coordinates.x, i, coordinates.z], new Vector3Int(coordinates.x, i + 1, coordinates.z));
                    }
                    else
                    {
                        grid[coordinates.x, i +1, coordinates.z] = null;
                        Logger.Error("An error happened while moving blocks down the grid from " + coordinates + " to " + (new Vector3Int(coordinates.x, i + 1, coordinates.z)));
                        return;
                    }
                }
            }
        }
        MoveBlock(block.gameObject, coordinates);
        UpdateGridSystems();
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
        Vector3Int output = new Vector3Int(
                (int)Mathf.Round((position.x - cellSize.x / 2) / cellSize.x),
                (int)Mathf.Round((position.y - cellSize.y / 2) / cellSize.y),
                (int)Mathf.Round((position.z - cellSize.z / 2) / cellSize.z)
        );
        return output;
    }


    //Returns the lowest block on the terrain
    public Vector3Int GetLowestFreeSlot(Vector2Int coordinates)
    {
        // Position en Y des coordonnées au sol données
        float worldY =
            myTerrain.SampleHeight(
                IndexToWorldPosition(
                    new Vector3Int(coordinates.x, 0, coordinates.y)
                )
            );
        // Index de Y
        int y = WorldPositionToIndex(new Vector3(coordinates.x, worldY + cellSize.y / 2, coordinates.y)).y;

        // Candidate coordinates
        Vector3Int coordinates3 = new Vector3Int(coordinates.x, y, coordinates.y);

        while (GetSlotType(coordinates3) != GridManagement.blockType.FREE)
        {
            coordinates3.y++;
            if (coordinates3.y > GameManager.instance.gridManagement.gridSize.y)
            {
                return new Vector3Int(-1,-1,-1);
            }
        }


        if (grid[coordinates3.x, coordinates3.y, coordinates3.z] != null)
        {
            // The slot is occupied - let's see if we can lay our block ontop
            for (int i = coordinates3.y; i <= maxHeight; i++)
            {
                if (GetSlotType(new Vector3Int(coordinates3.x, i, coordinates3.z)) != GridManagement.blockType.FREE)
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
        return coordinates3;
    }

    //Update a block so he touch the ground or the first block encountered (Like if gravity was applied to it)
    public void LayBlock(Block block, Vector2Int coordinates)
    {
        Vector3Int destinationCoordinates = GetLowestFreeSlot(coordinates);
        if (destinationCoordinates.y >= 0)
        {
            MoveBlock(block.gameObject, destinationCoordinates);
            buildablePositions.Remove(coordinates);
            UpdateGridSystems();
        } else
        {
            Logger.Warn("Error while trying to place block " + block.name + " at position " + coordinates);
        }
    }

    /// <summary>
    /// Lays a block at X Y coordinates on the grid at the highest possible point 
    /// (ontop of a building or on the ground)
    /// </summary>
    /// <param name="blockId"></param>
    /// <param name="coordinates"></param>
    public Block LayBlock(int blockId, Vector2Int coordinates)
    {
        GameObject newBlock = SpawnBlock(blockId, new Vector3Int(coordinates.x,gridSize.y-1,coordinates.y));
        Block b = newBlock.GetComponent<Block>();
        LayBlock(b, coordinates);
        return b;
    }

    public GameObject SpawnBlock(int blockId, Vector3Int coordinates)
    {
        GameObject newBlock = CreateBlockFromId(blockId);
        MoveBlock(newBlock, coordinates);
        Logger.Debug("Spawned block : " + newBlock.GetComponent<Block>().scheme.name + " with prefab " + GameManager.instance.library.blockPrefab.name + " at position "+ coordinates.ToString());
        return newBlock;
    }

    public GameObject CreateBlockFromId(int blockId)
    {
        BlockScheme scheme = GameManager.instance.library.GetBlockByID(blockId);
        if (scheme == null) { Logger.Warn("BlockScheme index not found - index : " + blockId); return null; }

        GameObject newBlockGO = Instantiate(GameManager.instance.library.blockPrefab);
        Block newBlock = newBlockGO.GetComponent<Block>();
        newBlock.scheme = scheme;
        LoadBlock(newBlock);
        return newBlockGO;
    }

    public void LoadBlock(Block block)
    {
        block.LoadBlock();
    }

    public blockType GetSlotType(Vector3Int coordinates)
    {
        GameObject objectFound = grid[coordinates.x, coordinates.y, coordinates.z];
        if (objectFound != null && objectFound.tag == "Bridge") {
                return blockType.BRIDGE;
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
        UpdateGridSystems();
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

            if (GetSlotType(new Vector3Int(_posToCheck.x, _posToCheck.y, _posToCheck.z)) != GridManagement.blockType.FREE)
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
        blockA.bridges.Add(parentBridgeGameObject);

        //Adding animator to the bridge
        parentBridgeGameObject.AddComponent<BridgeAnimator>();

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
                    (((cellSize.x - 1) / 2) * direction.x) + (cellSize.x * (i - 2)) * direction.x, 
                    0, 
                    (((cellSize.z - 1) / 2) * direction.y) + (cellSize.z * (i - 2)) * direction.y
                );
                newBridgePart.transform.localRotation = firstBridgePart.transform.localRotation;
            }

            //Création de la derniere partie du pont (de taille (cellsize-1)/2)
            else {
                newBridgePart = Instantiate(bridgePrefab, parentBridgeGameObject.transform);
                newBridgePart.transform.localPosition = new Vector3(
                    (((cellSize.x - 1) / 2) * direction.x) + (cellSize.x * (i - 2)) * direction.x, 
                    0, 
                    (((cellSize.z - 1) / 2) * direction.y) + (cellSize.z * (i - 2)) * direction.y
                );
                newBridgePart.transform.localRotation = firstBridgePart.transform.localRotation;
            }
            newBridgePart.name = "Bridge part " + i;
            newBridgePart.transform.parent = parentBridgeGameObject.transform;
        }
        bridgesList.Add(parentBridgeGameObject);
        bridgesGrid[
            blockA.gridCoordinates.x,
            blockA.gridCoordinates.y,
            blockA.gridCoordinates.z
        ] = blockB.gridCoordinates;

        // Update stats
        if (gameManager.achievementManager.stats.maxBridgeLength < bridgeLength) {
            gameManager.achievementManager.stats.maxBridgeLength = bridgeLength;
        }

        //Update the system
        UpdateGridSystems();

        return parentBridgeGameObject;
    }

    /// <summary>
    /// Destroy bridges (give the parent bridge object as argument)
    /// </summary>
    /// <param name="bridgeObject"></param>
    public void DestroyBridge(GameObject bridgeObject)
    {
        //Removes the bridge from the list of bridges of his parent
        bridgeObject.transform.parent.GetComponent<Block>().bridges.Remove(bridgeObject);

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
        GameManager.instance.soundManager.Play("DestroyBlock");
        Destroy(bridgeObject);
        
        //Update the system
        UpdateGridSystems();
    }
}
