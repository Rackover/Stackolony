using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockingBay : MonoBehaviour {

    [Header("SETTINGS")]
    [Space(1)]
    public int stockSize; //Combien de blocs peut on mettre par colonne
    public int size; //Combien de cellules prend l'objet, on veut savoir la racine carrée (2 = 2x2, 3 = 3x3 etc...)
    public GameObject[,] slots; //Liste des slots
    private int stockedAmount; //Le nombre de blocks stockés actuellement dans la baie de stockage
    public int maxStock; //Combien de blocs au maximum la zone peut elle contenir (-1 = infini)
    public float offset; //A quel point les blocs sont eloignés les uns de les autres quand ils sont dans la zone de stockage
    public GameObject[,,] stockedBlocks;

    [Header("REFERENCES")]
    [Space(1)]
    public CursorManagement cursor;
    public GridManagement gridManager;
    public Interface uiManager;
    public GameObject blockPrefab;

    private bool isPlaced = false;

    private void Start()
    {
        //Recuperation des references non attribuées
        if (cursor == null)
            cursor = FindObjectOfType<CursorManagement>();
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManagement>();
        if (uiManager == null)
            uiManager = FindObjectOfType<Interface>();

        //Initialisation des tableaux
        slots = new GameObject[stockSize, stockSize];
        stockedBlocks = new GameObject[stockSize, 1, stockSize];

        //Empêche de changer d'outil pendant qu'on est supposé placer la stocking bay
        cursor.switchMode(CursorManagement.cursorMode.Default);
        cursor.canSwitchTools = false;

        GenerateSlots();
    }

    private void Update()
    {
        if (!isPlaced)
        {
            // cursor.posInTerrain += new Vector3Int(1, 0, 1);
            transform.position = new Vector3
                (
                    (cursor.posInTerrain.x) * gridManager.cellSize,
                    cursor.posInTerrain.y + 0.2f,
                    (cursor.posInTerrain.z) * gridManager.cellSize
                );
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 cursorPosition = new Vector3
                (
                    (cursor.posInTerrain.x) * gridManager.cellSize + gridManager.cellSize * 0.5f,
                    cursor.posInTerrain.y + 0.2f,
                    (cursor.posInTerrain.z) * gridManager.cellSize + gridManager.cellSize * 0.5f
                );
                if (canBePlaced(cursorPosition))
                {
                    placeBay(cursor.posInTerrain);
                }
                else
                {
                    uiManager.ShowError("You can't place it here");
                }
            }
        } else
        {
            if (Input.GetKeyDown(KeyCode.U)) {
                GenerateBlock();
            }
        }
    }


    void GenerateSlots() //Genere un empty gameobject à chaque emplacement de la stockingBay
    {
        Vector3 topLeftCorner = new Vector3(-0.5f, 0.5f, -0.5f); //Vecteur correspondant à l'emplacement en haut à gauche de la stockingBay
        topLeftCorner += new Vector3(1f / (float)(stockSize*2),0, 1f / (float)(stockSize * 2)); //On ajoute un demi slot pour que les slots crées par la suite soient centrés
        for (int x = 0; x < stockSize; x++)
        {
            for (int y = 0; y < stockSize; y++) {

                //Création des slots
                GameObject newSlot = new GameObject();
                newSlot.name = "Slot[" + x +"," + y + "]";
                slots[x, y] = newSlot;
                newSlot.transform.parent = this.transform;
                newSlot.transform.localPosition = new Vector3
                    (
                        (float)x / (float)stockSize, 
                        0, 
                        (float)y / (float)stockSize
                    ) + topLeftCorner;
            }
        }
    }

    //Place la baie de stockage
    private void placeBay(Vector3 cursorCoordinates)
    {
        cursor.canSwitchTools = true;
        isPlaced = true;
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < gridManager.maxHeight; y++)
                {
                    Vector3Int _pos = new Vector3Int();
                    _pos.x = Mathf.RoundToInt(cursorCoordinates.x - x);
                    _pos.y = y;
                    _pos.z = Mathf.RoundToInt(cursorCoordinates.z - z);
                    gridManager.grid[_pos.x, _pos.y, _pos.z] = this.gameObject;
                }
            }
        }
    }

    private bool canBePlaced(Vector3 positionToCompare) //Compare toutes les positions du bloc avec la position ou se trouve la souris pour savoir si le bloc est plaçable
    {
        float flexibility = 0.4f; //A quel point le système sera flexible et permettra au joueur de placer un bloc sur un sol non adapté
        foreach (GameObject t in slots)
        {
            if (gridManager.myTerrain.SampleHeight(t.transform.position) < (positionToCompare.y - flexibility) || Terrain.activeTerrain.SampleHeight(t.transform.position) > (positionToCompare.y + flexibility))
            {
                return false;
            }
        }
        return true;
    }

    //Genere un block à l'intérieur de la baie de stockage
    public void GenerateBlock()
    {
        GameObject newBlock = Instantiate(blockPrefab);
        StockBlock(newBlock);
    }

    //Place un block à l'intérieur de la baie de stockage
    public void StockBlock(GameObject blockToStock)
    {
        if (stockedAmount < maxStock || maxStock < 0)
        {
            stockedAmount++;

            for (int y = 0; y < stockedBlocks.GetLength(1)-1; y++)
            {
                for (int x = 0; x < stockSize; x++)
                {
                    for (int z = 0; z < stockSize; z++)
                    {
                        if (stockedBlocks[x, y, z] == null)
                        {
                            //Stockage du bloc
                            stockedBlocks[x, y, z] = blockToStock;
                            blockToStock.transform.position = slots[x, z].transform.position;
                            blockToStock.transform.position += new Vector3(0, y + 0.5f, 0);
                            blockToStock.transform.SetParent(slots[x, z].transform);
                            blockToStock.transform.localScale = Vector3.one;
                            blockToStock.name = "StockedBlock[" + x + "," + y + "," + z + "]";
                            blockToStock.layer = LayerMask.NameToLayer("StockedBlock");
                            return;
                        }
                    }
                }
            }
            //Si on a pas réussi à stocker le block, la grille est pleine, dans ce cas on l'agrandi et on reessaye
            AddHeightToGrid();
            StockBlock(blockToStock);
        }
    }

    //Agrandi la taille max de la grille de stockage de blocs
    public void AddHeightToGrid()
    {
        GameObject[,,] newGrid = new GameObject[stockedBlocks.GetLength(0), stockedBlocks.GetLength(1)+1, stockedBlocks.GetLength(2)];
        for (int x = 0; x < stockSize; x++)
        {
            for (int z = 0; z < stockSize; z++)
            {
                for (int y = 0; y < stockedBlocks.GetLength(1); y++)
                {
                    newGrid[x, y, z] = stockedBlocks[x, y, z];
                }
            }
        }
        stockedBlocks = newGrid;
    }

    //Retire un block de la baie de stockage
    public void DestockBlock(GameObject block)
    {
        if (block.layer == LayerMask.NameToLayer("StockedBlock"))
        {
            stockedAmount--;
            string xPos = ""+block.name[13];
            string yPos = ""+block.name[15];
            string zPos = ""+block.name[17];
            Vector3Int blockCoordinates = new Vector3Int(int.Parse(xPos), int.Parse(yPos), int.Parse(zPos));
            stockedBlocks[blockCoordinates.x, blockCoordinates.y, blockCoordinates.z] = null;

            //Fait tomber les blocs qui se trouvaient au dessus de ce bloc
            for (int y = blockCoordinates.y+1; y < stockedBlocks.GetLength(1); y++)
            {
                GameObject foundObject = stockedBlocks[blockCoordinates.x, y, blockCoordinates.z];
                if (foundObject != null)
                {
                    stockedBlocks[blockCoordinates.x, y, blockCoordinates.z] = null;
                    stockedBlocks[blockCoordinates.x, y - 1, blockCoordinates.z] = foundObject;
                    foundObject = stockedBlocks[blockCoordinates.x, y - 1, blockCoordinates.z];
                    foundObject.transform.position = slots[blockCoordinates.x, blockCoordinates.z].transform.position;
                    foundObject.transform.position += new Vector3(0, y - 0.5f, 0);
                    foundObject.transform.localScale = Vector3.one;
                    foundObject.name = "StockedBlock[" + blockCoordinates.x + "," + (y-1) + "," + blockCoordinates.z + "]";
                }
            }
            block.layer = LayerMask.NameToLayer("Block");
            block.transform.SetParent(FindObjectOfType<GridManagement>().transform.Find("Grid"));
        } else
        {
            uiManager.ShowError("Block can't be destocked");
        }
    }
}
