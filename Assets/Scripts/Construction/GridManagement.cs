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
    public InterfaceManager uiManager;

    //------------VARIABLES PRIVEE------------
    Terrain myTerrain; //Terrain sur lequel la grille doit être generée
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

        //GENERATION DU GAMEOBJECT CONTENANT CHAQUE LAYERS
        gridGameObject = new GameObject(); //Crée le gameobject qui contiendra absolument tout les blocs du jeu (pour trier)
        gridGameObject.name = "Grid";
        gridGameObject.transform.parent = this.transform;
        gridGameObject.transform.localPosition = Vector3.zero;
        Debug.Log("-----Generating grid-----");
    }

    public void DestroyBlock(Vector3Int coordinates)
    {
        if (grid[coordinates.x, coordinates.y,coordinates.z] != null)
        {
            // Removes object from list and destroys the gameObject
            GameObject target = grid[coordinates.x, coordinates.y, coordinates.z];
            buildingsList.RemoveAll(o => o == target);
            Destroy(target);

            for (var i = coordinates.y+1; i<maxHeight; i++) //Fait descendre d'une case les blocs au dessus du bloc supprimé
            {
                if (grid[coordinates.x, i, coordinates.z] == null)
                {
                    grid[coordinates.x, i - 1, coordinates.z] = null;
                    return;
                } 
                else
                {
                    //Change la position du bloc dans la grille contenant chaque bloc
                    grid[coordinates.x, i-1, coordinates.z] = grid[coordinates.x, i, coordinates.z];

                    //Change le nom du bloc pour qu'il corresponde à sa nouvelle position (Ex : Block[1,2,1])
                    grid[coordinates.x, i - 1, coordinates.z].name = "Block[" + coordinates.x + ";" + (i - 1) + ";" + coordinates.z + "]";

                    //Met à jour les coordonnées du block dans son script "BlockLink"
                    grid[coordinates.x, i-1, coordinates.z].GetComponent<BlockLink>().gridCoordinates = new Vector3Int(coordinates.x, i-1, coordinates.z);

                    //Déplace le block vers ses nouvelles coordonnées
                    grid[coordinates.x, i-1, coordinates.z].GetComponent<BlockLink>().MoveToMyPosition();
                }
            }
        }
    }

    public void MoveBlock(GameObject newBlock, Vector3Int coordinates) //Bouge un bloc à une nouvelle coordonnée
    {
        grid[coordinates.x, coordinates.y, coordinates.z] = newBlock; 

        if (grid[coordinates.x, coordinates.y,coordinates.z] != null)
        {
            for (var i = coordinates.y + 1; i < maxHeight; i++) //Fait monter d'une case les blocs au dessus du bloc ajouté
            {
                if (grid[coordinates.x, i, coordinates.z] == null)
                {
                    grid[coordinates.x, i + 1, coordinates.z] = null;
                    return;
                } 
                else
                {
                    
                    //Change la position du bloc dans la grille contenant chaque bloc
                    grid[coordinates.x, i + 1, coordinates.z] = grid[coordinates.x, i, coordinates.z];

                    GameObject actualGridPos = grid[coordinates.x, i + 1, coordinates.z];

                    //Change le nom du bloc pour qu'il corresponde à sa nouvelle position (Ex : Block[1,2,1])
                    actualGridPos.name = "Block[" + coordinates.x + ";" + (i + 1) + ";" + coordinates.z + "]";

                    //Met à jour les coordonnées du block dans son script "BlockLink"
                    actualGridPos.GetComponent<BlockLink>().gridCoordinates = new Vector3Int(coordinates.x, i-1, coordinates.z);

                    //Déplace le block vers ses nouvelles coordonnées
                    actualGridPos.GetComponent<BlockLink>().MoveToMyPosition();
                }
            }
        }
    }

    public void SpawnBlock(GameObject blockPrefab, Vector2Int coordinates) //Genère un bloc à une coordonnée 2D sur la map
    {
        int cursorPosYInTerrain = FindObjectOfType<Cursor>().posInTerrain.y; //Position en Y à laquelle le joueur a cliqué
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
        buildingsList.Add(newBlock);
        newBlock.GetComponent<BlockLink>().gridCoordinates = new Vector3Int(coordinates.x, newBlockHeight, coordinates.y);
        newBlock.name = "Block[" + coordinates.x + ";" + newBlockHeight + ";" + coordinates.y + "]";
    }

    /// <summary>
    /// Bridges two building and returns the parent bridge GameObject
    /// </summary>
    /// <param name="blockA"></param>
    /// <param name="blockB"></param>
    public GameObject CreateBridge(BlockLink blockA, BlockLink blockB)
    {
        //Creation du gameObject contenant le pont
        GameObject parentBridgeGameObject = new GameObject();
        parentBridgeGameObject.name = "Bridge";
        parentBridgeGameObject.transform.parent = blockA.transform;
        parentBridgeGameObject.transform.localPosition = new Vector3(0, 0, 0);
        parentBridgeGameObject.transform.localScale = new Vector3(1, 1, 1);

        // Adding origin/destination info in the parent bridge game object
        BridgeInfo bridgeInfo = parentBridgeGameObject.AddComponent<BridgeInfo>();
        bridgeInfo.origin = blockA.gridCoordinates;
        bridgeInfo.destination = blockB.gridCoordinates;

        //Calcul de la longueur du pont et de l'orientation du pont
        int bridgeLength = 0;

        Vector2Int direction = Vector2Int.zero;

        if (blockA.gridCoordinates.x == blockB.gridCoordinates.x) {
            bridgeLength = Mathf.Abs(blockA.gridCoordinates.z - blockB.gridCoordinates.z) - 1;
            direction.x = 0;
            if (blockA.gridCoordinates.z - blockB.gridCoordinates.z > 0) {
                direction.y = -1;
            }
            else {
                direction.y = 1;
            }
        }
        else {
            bridgeLength = Mathf.Abs(blockA.gridCoordinates.x - blockB.gridCoordinates.x) - 1;
            direction.y = 0;
            if (blockA.gridCoordinates.x - blockB.gridCoordinates.x > 0) {
                direction.x = -1;
            }
            else {
                direction.x = 1;
            }
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

        Destroy(bridgeObject);
    }
}
