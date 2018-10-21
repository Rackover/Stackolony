using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManagement : MonoBehaviour {

    //------------VARIABLES PUBLIQUES------------
    [Header("=== MAP SETTINGS ===")][Space(1)]
    [Tooltip("Quelle taille font les cellules en X et Y (la hauteur sera toujours de 1")]
    public float cellSize;
    [Tooltip("Quelle hauteur max pour les tours")]
    public int maxHeight;

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
    public static GameObject[,,] grid;

    [Header("=== REFERENCES ===")][Space(1)]
    public InterfaceManager uiManager;

    //------------VARIABLES PRIVEE------------
    Terrain myTerrain; //Terrain sur lequel la grille doit être generée
    public Vector3Int gridSize; //Nombre de cases sur le terrain
    public static float cellSizeStatic; //La taille des cellules, on la passe en statique pour y accéder rapidement, elle n'est pas supposée changer au cours de la partie
    private GameObject gridGameObject; //GameObject contenant la grille


    private void ResizeTerrain() //Change la taille du terrain en fonction de la taille des cellules choisie pour obtenir un nombre de cases rond
    {
        Vector3 newSize = new Vector3(0, 0, 0);
        double floatX = myTerrain.terrainData.size.x;
        double floatY = myTerrain.terrainData.size.z;

        double floatB = cellSize;

        double xComp = floatX % floatB; //Les modulos entre deux floats posent des problèmes, donc on converti en "double" pour avoir une meilleure précision
        double yComp = floatY % floatB;

        if (xComp*xComp > 0.005f) //Un modulos entre deux float peut donner une valeur extrêmement basse, on utilise donc ce if pour éviter de modifier le terrain si cette valeur est trop basse
        {
            newSize.x = (float)xComp;
        }
        if (yComp*yComp > 0.005)
        {
            newSize.z = (float)yComp;
        }
        myTerrain.terrainData.size -= newSize; //On applique notre nouvelle taille au terrain

        myTerrain.terrainData.size += new Vector3(0, maxHeight-myTerrain.terrainData.size.y, 0); //Regle la hauteur du terrain a la hauteur max définie par le joueur

        //Calcule le nombre de cases du terrain en fonction de la taille de la cellule grâce à la nouvelle taille du terrain
        gridSize.x = Mathf.RoundToInt(myTerrain.terrainData.size.x / cellSize);
        gridSize.z = Mathf.RoundToInt(myTerrain.terrainData.size.z / cellSize);

        //Initialisation de la variable grille contenant chaque bloc
        grid = new GameObject[gridSize.x, gridSize.y, gridSize.z];
        bridgesGrid = new Vector3[gridSize.x, gridSize.y, gridSize.z];
}
    private void Start()
    {
        //Recuperation du terrain
        myTerrain = Terrain.activeTerrain;

        //Initialisation des variables statiques
        gridSize.x = Mathf.RoundToInt(myTerrain.terrainData.size.x / cellSize);
        gridSize.z = Mathf.RoundToInt(myTerrain.terrainData.size.z / cellSize);
        gridSize.y = maxHeight;
        cellSizeStatic = cellSize;

        //Redimension du terrain pour avoir un nombre de cellules rond
        ResizeTerrain();

        GenerateGrid();
    }

    private void GenerateGrid() //Fonction pour générer la grille sur le terrain
    {
        //GENERATION DU GAMEOBJECT CONTENANT CHAQUE LAYERS
        gridGameObject = new GameObject(); //Crée le gameobject qui contiendra absolument tout les blocs du jeu (pour trier)
        gridGameObject.name = "Grid";
        gridGameObject.transform.parent = this.transform;
        gridGameObject.transform.localPosition = new Vector3(0, 0, 0);
        Debug.Log("Generating grid");
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

    public void AddBlock(GameObject newBlock, Vector3Int coordinates)
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

                    //Change le nom du bloc pour qu'il corresponde à sa nouvelle position (Ex : Block[1,2,1])
                    grid[coordinates.x, i + 1, coordinates.z].name = "Block[" + coordinates.x + ";" + (i + 1) + ";" + coordinates.z + "]";

                    //Met à jour les coordonnées du block dans son script "BlockLink"
                    grid[coordinates.x, i + 1, coordinates.z].GetComponent<BlockLink>().gridCoordinates = new Vector3Int(coordinates.x, i-1, coordinates.z);

                    //Déplace le block vers ses nouvelles coordonnées
                    grid[coordinates.x, i + 1, coordinates.z].GetComponent<BlockLink>().MoveToMyPosition();
                }
            }
        }
    }

    public void GenerateBlock(GameObject blockPrefab, Vector2Int coordinates)
    {
        int cursorPosYInTerrain = FindObjectOfType<Cursor>().posInTerrain.y; //Position en Y à laquelle le joueur a cliqué
        GameObject newBlock = Instantiate(blockPrefab, gridGameObject.transform);

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Obtention de la hauteur à laquelle le bloc doit être posé
        int newBlockHeight = 0;

        if (Physics.Raycast(ray, out hit))
        {

            //Si le joueur clique sur un terrain, on récupére la position ou il a cliqué, puis s'il y a déjà une tour, on récupère la hauteur de cette tour.
            //Ensuite on génère le bloc à cette position
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
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
            }
            //Si le joueur clique sur un block, on récupére la position ou il a cliqué, puis s'il y a déjà une tour, on récupère la hauteur de cette tour.
            //Ensuite on génère le bloc à cette position
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block"))
            {
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
                    Debug.Log("Max height reached");
                    Destroy(newBlock);
                }
                else
                {
                    newBlock.transform.position = grid[coordinates.x, newBlockHeight - 1, coordinates.y].gameObject.transform.position;
                    newBlock.transform.position += new Vector3(0, 1, 0);
                }
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
        int directionX = 0;
        int directionZ = 0;

        if (blockA.gridCoordinates.x == blockB.gridCoordinates.x) {
            bridgeLength = Mathf.Abs(blockA.gridCoordinates.z - blockB.gridCoordinates.z) - 1;
            directionX = 0;
            if (blockA.gridCoordinates.z - blockB.gridCoordinates.z > 0) {
                directionZ = -1;
            }
            else {
                directionZ = 1;
            }
        }
        else {
            bridgeLength = Mathf.Abs(blockA.gridCoordinates.x - blockB.gridCoordinates.x) - 1;
            directionZ = 0;
            if (blockA.gridCoordinates.x - blockB.gridCoordinates.x > 0) {
                directionX = -1;
            }
            else {
                directionX = 1;
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
                    (((cellSize - 1) / 2) * directionX) + (cellSize * (i - 2)) * directionX + (0.5f * directionX), 
                    0, 
                    (((cellSize - 1) / 2) * directionZ) + (cellSize * (i - 2)) * directionZ + (0.5f * directionZ)
                );
                newBridgePart.transform.localRotation = firstBridgePart.transform.localRotation;
                Debug.Log("Direction X = " + directionX + "Direction Z = " + directionZ);
            }

            //Création de la derniere partie du pont (de taille (cellsize-1)/2)
            else {
                newBridgePart = Instantiate(bridgePrefab, parentBridgeGameObject.transform);
                newBridgePart.transform.localPosition = new Vector3(
                    (((cellSize - 1) / 2) * directionX) + (cellSize * (i - 2)) * directionX + (0.5f * directionX), 
                    0, 
                    (((cellSize - 1) / 2) * directionZ) + (cellSize * (i - 2)) * directionZ + (0.5f * directionZ)
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
