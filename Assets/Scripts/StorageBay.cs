using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageBay : MonoBehaviour {

    [Header("SETTINGS")]
    [Space(1)]
    public int stockSize; //Combien de blocs peut on mettre par colonne
    public int size; //Combien de cellules prend l'objet, on veut savoir la racine carrée (2 = 2x2, 3 = 3x3 etc...)
    public GameObject[,] slots; //Liste des slots
    private int storedAmount; //Le nombre de blocks stockés actuellement dans la baie de stockage
    public int maxStock; //Combien de blocs au maximum la zone peut elle contenir (-1 = infini)
    public float offset; //A quel point les blocs sont eloignés les uns de les autres quand ils sont dans la zone de stockage
    public float storageBayHeight; //Hauteur de la zone de stockage
    public GameObject[,,] storedBlocks;
    public float flexibility; //A quel point le systeme sera flexible et permettra au joueur de placer la zone de stockage sur le terrain

    [Header("REFERENCES")]
    [Space(1)]
    public CursorManagement cursor;
    public GridManagement gridManager;
    public Interface uiManager;
    public GameObject blockPrefab;
    public SFXManager sfxManager;

    private bool isPlaced = false;

    private void Start()
    {
        //Initialisation des tableaux
        slots = new GameObject[stockSize, stockSize];
        storedBlocks = new GameObject[stockSize, 1, stockSize];

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
                    cursor.posInTerrain.y + storageBayHeight,
                    (cursor.posInTerrain.z) * gridManager.cellSize
                );
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 cursorPosition = new Vector3
                (
                    (cursor.posInTerrain.x) * gridManager.cellSize + gridManager.cellSize * 0.5f,
                    cursor.posInTerrain.y + storageBayHeight,
                    (cursor.posInTerrain.z) * gridManager.cellSize + gridManager.cellSize * 0.5f
                );
                if (CanBePlaced(cursorPosition))
                {
                    PlaceBay(cursor.posInTerrain);
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
    private void PlaceBay(Vector3 cursorCoordinates)
    {
        sfxManager.PlaySoundLinked("BlockDropScientific", this.gameObject,0.1f,1,false);
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

    private bool CanBePlaced(Vector3 positionToCompare) //Compare toutes les positions du bloc avec la position ou se trouve la souris pour savoir si le bloc est plaçable
    {
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
        newBlock.GetComponent<BlockLink>().myContainer.DropBlock();
        StoreBlock(newBlock);
    }

    //Place un block à l'intérieur de la baie de stockage
    public void StoreBlock(GameObject blockToStore)
    {
        if (storedAmount < maxStock || maxStock < 0)
        {
            storedAmount++;

            for (int y = 0; y < storedBlocks.GetLength(1)-1; y++)
            {
                for (int x = 0; x < stockSize; x++)
                {
                    for (int z = 0; z < stockSize; z++)
                    {
                        if (storedBlocks[x, y, z] == null)
                        {
                            //Stockage du bloc
                            storedBlocks[x, y, z] = blockToStore;
                            blockToStore.transform.position = slots[x, z].transform.position;
                            blockToStore.transform.position += new Vector3(0, y + 0.55f, 0);
                            blockToStore.transform.SetParent(slots[x, z].transform);
                            blockToStore.transform.localScale = Vector3.one;
                            blockToStore.name = "StoredBlock[" + x + "," + y + "," + z + "]";
                            BlockLink blockInfo = blockToStore.GetComponent<BlockLink>();
                            blockInfo.gridCoordinates = new Vector3Int(x, y, z);
                            blockInfo.myContainer.CloseContainer();
                            blockToStore.layer = LayerMask.NameToLayer("StoredBlock");
                            if (blockInfo.myContainer.isFalling == false)
                            {
                                sfxManager.PlaySoundLinked("BlockDrop", blockToStore);
                            }
                            return;
                        }
                    }
                }
            }
            //Si on a pas réussi à stocker le block, la grille est pleine, dans ce cas on l'agrandi et on reessaye
            AddHeightToGrid();
            StoreBlock(blockToStore);
        }
    }

    //Agrandi la taille max de la grille de stockage de blocs
    public void AddHeightToGrid()
    {
        GameObject[,,] newGrid = new GameObject[storedBlocks.GetLength(0), storedBlocks.GetLength(1)+1, storedBlocks.GetLength(2)];
        for (int x = 0; x < stockSize; x++)
        {
            for (int z = 0; z < stockSize; z++)
            {
                for (int y = 0; y < storedBlocks.GetLength(1); y++)
                {
                    newGrid[x, y, z] = storedBlocks[x, y, z];
                }
            }
        }
        storedBlocks = newGrid;
    }

    //Retire un block de la baie de stockage
    public void DeStoreBlock(GameObject block)
    {
        if (block.layer == LayerMask.NameToLayer("StoredBlock"))
        {
            storedAmount--;

            block.GetComponent<BlockLink>().myContainer.OpenContainer();
            Vector3Int blockCoordinates = block.GetComponent<BlockLink>().gridCoordinates;
            storedBlocks[blockCoordinates.x, blockCoordinates.y, blockCoordinates.z] = null;

            //Fait tomber les blocs qui se trouvaient au dessus de ce bloc
            for (int y = blockCoordinates.y+1; y < storedBlocks.GetLength(1); y++)
            {
                GameObject foundObject = storedBlocks[blockCoordinates.x, y, blockCoordinates.z];
                if (foundObject != null)
                {
                    storedBlocks[blockCoordinates.x, y, blockCoordinates.z] = null;
                    storedBlocks[blockCoordinates.x, y - 1, blockCoordinates.z] = foundObject;
                    foundObject = storedBlocks[blockCoordinates.x, y - 1, blockCoordinates.z];
                    foundObject.transform.position = slots[blockCoordinates.x, blockCoordinates.z].transform.position;
                    foundObject.transform.position += new Vector3(0, y - 0.5f, 0);
                    foundObject.transform.localScale = Vector3.one;
                    foundObject.name = "StoredBlock[" + blockCoordinates.x + "," + (y-1) + "," + blockCoordinates.z + "]";
                    BlockLink blockInfo = foundObject.GetComponent<BlockLink>();
                    blockInfo.gridCoordinates = new Vector3Int(blockCoordinates.x, y-1, blockCoordinates.z);
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
