using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManagement : MonoBehaviour {

    //LAST CLEAN : Suppression du raycast, suppression des variables statiques, suppression des variables de type "double"


    //------------VARIABLES PUBLIQUES------------
    [Header("=== MAP SETTINGS ===")][Space(1)]
    [Tooltip("Quelle taille font les cellules en X et Y (la hauteur sera toujours de 1")]
    public float cellSize;
    [Tooltip("Quelle hauteur max pour les tours")]
    public int maxHeight;
    [Tooltip("Quelle taille fait un bloc en hauteur")]
    public int cellHeight;

    [Header("=== PREFABS ===")][Space(1)]
    [Header("Bridge")]
    [Tooltip("Prefab du pont, de la taille (cellSize)")]
    public GameObject bridgePrefab;
    [Tooltip("Prefab de la fin du pont, de la taille (cellSize-1)/2")]
    public GameObject bridgeEndPrefab;
    [Tooltip("Prefab du départ du pont, de la taille (cellSize-1)/2")]
    public GameObject bridgeStartPrefab;

    [Header("=== LISTS ===")][Space(1)]
    [Header("Liste de ponts")]
    public List<GameObject> bridgesList = new List<GameObject>();
    [Header("Liste des batiments")]
    public List<GameObject> buildingsList = new List<GameObject>();
    [Header("Grille de blocs")]
    public Vector3[,,] bridgesGrid;
    public GameObject[,,] grid;

    [Header("=== REFERENCES ===")][Space(1)]
    public Interface uiManager;
    public GridDebugger gridDebugger;

    //------------VARIABLES PRIVEE------------
    [System.NonSerialized]
    public Terrain myTerrain; //Terrain sur lequel la grille doit être generée
    public Vector3Int gridSize; //Nombre de cases sur le terrain
    private GameObject gridGameObject; //GameObject contenant la grille

    private void Start()
    {
        //Recuperation du terrain
        myTerrain = Terrain.activeTerrain;

        //Initialisation des variables statiques
        gridSize.x = Mathf.RoundToInt(myTerrain.terrainData.size.x / cellSize);
        gridSize.z = Mathf.RoundToInt(myTerrain.terrainData.size.z / cellSize);
        gridSize.y = maxHeight;

        GenerateGrid();
    }

    private void GenerateGrid() //Fonction pour générer la grille sur le terrain
    {
        //Initialisation de la variable grille contenant chaque bloc
        grid = new GameObject[gridSize.x, gridSize.y, gridSize.z];
        bridgesGrid = new Vector3[gridSize.x, gridSize.y, gridSize.z];
        gridDebugger.InitGrid(); //Initialise la grille pour gérer le debugger

        //GENERATION DU GAMEOBJECT CONTENANT CHAQUE LAYERS
        gridGameObject = new GameObject(); //Crée le gameobject qui contiendra absolument tout les blocs du jeu (pour trier)
        gridGameObject.name = "Grid";
        gridGameObject.transform.parent = this.transform;
        gridGameObject.transform.localPosition = Vector3.zero;
        Debug.Log("-----Generating grid-----");
    }

    public void DestroyBlock(Vector3Int coordinates)
    {
        if (grid[coordinates.x, coordinates.y, coordinates.z] != null)
        {
            // Removes object from list and destroys the gameObject
            GameObject target = grid[coordinates.x, coordinates.y, coordinates.z];
            buildingsList.RemoveAll(o => o == target);
            Destroy(target);
        }
        UpdateBlocks(coordinates);
    }

    //Update les blocs de toute la tour pour les remettre à leur bonne position
    public void UpdateBlocks(Vector3Int coordinates)
    {
        if (grid[coordinates.x, coordinates.y, coordinates.z] != null)
        {
            // Removes object from list and destroys the gameObject
            GameObject target = grid[coordinates.x, coordinates.y, coordinates.z];

            for (var i = coordinates.y + 1; i < maxHeight; i++) //Fait descendre d'une case les blocs au dessus du bloc supprimé
            {
                if (grid[coordinates.x, i, coordinates.z] == null)
                {
                    grid[coordinates.x, i - 1, coordinates.z] = null;
                    return;
                }
                else
                {
                    if (checkIfSlotIsBlocked(new Vector3Int(coordinates.x, i, coordinates.z),false) == 0)
                    {
                        //Change la position du bloc dans la grille contenant chaque bloc
                        grid[coordinates.x, i - 1, coordinates.z] = grid[coordinates.x, i, coordinates.z];

                        //Change le nom du bloc pour qu'il corresponde à sa nouvelle position (Ex : Block[1,2,1])
                        grid[coordinates.x, i - 1, coordinates.z].name = "Block[" + coordinates.x + ";" + (i - 1) + ";" + coordinates.z + "]";

                        //Met à jour les coordonnées du block dans son script "BlockLink"
                        grid[coordinates.x, i - 1, coordinates.z].GetComponent<BlockLink>().gridCoordinates = new Vector3Int(coordinates.x, i - 1, coordinates.z);

                        //Déplace le block vers ses nouvelles coordonnées
                        grid[coordinates.x, i - 1, coordinates.z].GetComponent<BlockLink>().MoveToMyPosition();

                        //Update le débugguer
                        gridDebugger.UpdateDebugGridAtHeight(i);
                    } else
                    {
                        grid[coordinates.x, i - 1, coordinates.z] = null;
                        uiManager.ShowError("Error at update");
                        Debug.Log("ERror at update");
                        return;
                    }
                }
            }
        }
    }


    public void MoveBlock(GameObject newBlock, Vector3Int coordinates) //Bouge un bloc à une nouvelle coordonnée
    {
        // If the block come from somewhere
        BlockLink _blocklink = newBlock.GetComponent<BlockLink>();
        if (_blocklink != null)
            UpdateBlocks(_blocklink.gridCoordinates);
        grid[_blocklink.gridCoordinates.x, _blocklink.gridCoordinates.x, _blocklink.gridCoordinates.x] = null;

        if (grid[coordinates.x, coordinates.y, coordinates.z] != null) // If there is a block where the block should be dropped
        {
            for (int i = grid.GetLength(1) - 1; i > coordinates.y - 1; i--) //Fait monter d'une case les blocs au dessus du bloc ajouté
            {
                if (grid[coordinates.x, i, coordinates.z] != null)
                {
                    if (checkIfSlotIsBlocked(new Vector3Int(coordinates.x, i, coordinates.z), false) ==0)
                    {
                        grid[coordinates.x, i + 1, coordinates.z] = grid[coordinates.x, i, coordinates.z];
                        GameObject actualGridPos = grid[coordinates.x, i + 1, coordinates.z];
                        //Change le nom du bloc pour qu'il corresponde à sa nouvelle position (Ex : Block[1,2,1])
                        actualGridPos.name = "Block[" + coordinates.x + ";" + (i + 1) + ";" + coordinates.z + "]";
                        //Met à jour les coordonnées du block dans son script "BlockLink"
                        actualGridPos.GetComponent<BlockLink>().gridCoordinates = new Vector3Int(coordinates.x, i + 1, coordinates.z);
                        //Déplace le block vers ses nouvelles coordonnées
                        actualGridPos.GetComponent<BlockLink>().MoveToMyPosition();
                        //Update le débugguer
                        gridDebugger.UpdateDebugGridAtHeight(i);
                    }
                }
            }
        }

        grid[coordinates.x, coordinates.y, coordinates.z] = newBlock;
        newBlock.name = "Block[" + coordinates.x + ";" + coordinates.y + ";" + coordinates.z + "]";
        if (_blocklink != null)
            _blocklink.gridCoordinates = new Vector3Int(coordinates.x, coordinates.y, coordinates.z);

    }

    public void SpawnBlock(GameObject blockPrefab, Vector2Int coordinates) //Genère un bloc à une coordonnée 2D sur la map
    {
        int cursorPosYInTerrain = FindObjectOfType<CursorManagement>().posInTerrain.y; //Position en Y à laquelle le joueur a cliqué

        if (checkIfSlotIsBlocked(new Vector3Int(coordinates.x,cursorPosYInTerrain,coordinates.y),true) != 0)
        {
            return;
        }
        GameObject newBlock = Instantiate(blockPrefab, gridGameObject.transform);

        //Obtention de la hauteur à laquelle le bloc doit être posé
        int newBlockHeight = 0;

        if (grid[coordinates.x,cursorPosYInTerrain,coordinates.y] == null)
        {
            //Le joueur a cliqué sur le sol
            for (var i = cursorPosYInTerrain; i < gridSize.z - 1; i++)
            {
                if (grid[coordinates.x, i, coordinates.y] == null)
                {
                    newBlockHeight = i; //On récupère la hauteur de l'endroit ou le joueur a cliqué, s'il y a déjà une tour, alors on obtient la hauteur de cette tour
                    break;
                }
            }
            newBlock.transform.position = new Vector3(
                coordinates.x * cellSize + (cellSize / 2),
                0.5f + newBlockHeight,
                coordinates.y * cellSize + (cellSize / 2)
            );
        } else
        {
            //Le joueur a cliqué sur un bloc
            for (var i = cursorPosYInTerrain; i < gridSize.y - 1; i++)
            {
                if (checkIfSlotIsBlocked(new Vector3Int(coordinates.x, i, coordinates.y), true) != 0)
                {
                    Destroy(newBlock);
                    return;
                }
                if (grid[coordinates.x, i, coordinates.y] == null)
                {
                    newBlockHeight = i;
                    break;
                }
            }
            if (newBlockHeight == 0)
            {
                uiManager.ShowError("You have reached max height");
                Debug.LogWarning("Max height reached");
                Destroy(newBlock);
            }
            else
            {
                newBlock.transform.position = grid[coordinates.x, newBlockHeight - 1, coordinates.y].gameObject.transform.position;
                newBlock.transform.position += new Vector3(0, 1, 0);
            }
        }
        grid[coordinates.x, newBlockHeight, coordinates.y] = newBlock;

        //Update le débugguer
        gridDebugger.UpdateDebugGridAtHeight(newBlockHeight);

        buildingsList.Add(newBlock);
        newBlock.GetComponent<BlockLink>().gridCoordinates = new Vector3Int(coordinates.x, newBlockHeight, coordinates.y);
        newBlock.name = "Block[" + coordinates.x + ";" + newBlockHeight + ";" + coordinates.y + "]";
    }

    public int checkIfSlotIsBlocked(Vector3Int coordinates, bool displayErrorMessages)
    {
        GameObject objectFound = grid[coordinates.x, coordinates.y, coordinates.z];
        if (objectFound != null)
        {
            switch (objectFound.tag)
            {
                case "StockingBay":
                    if (displayErrorMessages)
                        uiManager.ShowError("You can't build over the stocking bay");
                    return 1;
                case "Bridge":
                    if (displayErrorMessages)
                        uiManager.ShowError("You can't build over a bridge");
                    return 2;
                default:
                    break;
            }
        }
        return 0;
    }

    //Fonction a appelé pour déplacer un pont à une nouvelle position Y
    public void updateBridgePosition(BridgeInfo bridgeInfo, int newYPosition)
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
                    else if (blockToCheck.GetComponent<BlockLink>() != null)
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
    }

    /// <summary>
    /// Bridges two building and returns the parent bridge GameObject
    /// </summary>
    /// <param name="blockA"></param>
    /// <param name="blockB"></param>
    public GameObject CreateBridge(BlockLink blockA, BlockLink blockB)
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

            if (checkIfSlotIsBlocked(new Vector3Int(_posToCheck.x, _posToCheck.y, _posToCheck.z), true) != 0)
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
                    (((cellSize - 1) / 2) * direction.x) + (cellSize * (i - 2)) * direction.x + (0.5f * direction.x), 
                    0, 
                    (((cellSize - 1) / 2) * direction.y) + (cellSize * (i - 2)) * direction.y + (0.5f * direction.y)
                );
                newBridgePart.transform.localRotation = firstBridgePart.transform.localRotation;
            }

            //Création de la derniere partie du pont (de taille (cellsize-1)/2)
            else {
                newBridgePart = Instantiate(bridgePrefab, parentBridgeGameObject.transform);
                newBridgePart.transform.localPosition = new Vector3(
                    (((cellSize - 1) / 2) * direction.x) + (cellSize * (i - 2)) * direction.x + (0.5f * direction.x), 
                    0, 
                    (((cellSize - 1) / 2) * direction.y) + (cellSize * (i - 2)) * direction.y + (0.5f * direction.y)
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

        //Update le débugguer
        gridDebugger.UpdateDebugGridAtHeight(blockA.gridCoordinates.y);

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

        Destroy(bridgeObject);
    }
}
