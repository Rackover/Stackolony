using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageBay : MonoBehaviour {

    public GameManager gameManager;
    [Header("SETTINGS")]
    [Space(1)]
    public int stockSize; //Combien de blocs peut on mettre par colonne
    public int size; //Combien de cellules prend l'objet, on veut savoir la racine carrée (2 = 2x2, 3 = 3x3 etc...)
    private int storedAmount; //Le nombre de blocks stockés actuellement dans la baie de stockage
    public int maxStock; //Combien de blocs au maximum la zone peut elle contenir (-1 = infini)
    public float offset; //A quel point les blocs sont eloignés les uns de les autres quand ils sont dans la zone de stockage
    public float storageBayHeight; //Hauteur de la zone de stockage
    public float flexibility; //A quel point le systeme sera flexible et permettra au joueur de placer la zone de stockage sur le terrain
    
    public GameObject[,,] storedBlocks;
    public GameObject[,] slots; //Liste des slots
    
    bool isPlaced = false;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        //Initialisation des tableaux
        slots = new GameObject[stockSize, stockSize];
        storedBlocks = new GameObject[stockSize, 1, stockSize];

        //Empêche de changer d'outil pendant qu'on est supposé placer la stocking bay
        gameManager.cursorManagement.switchMode(CursorManagement.cursorMode.Default);
        gameManager.cursorManagement.canSwitchTools = false;

        GenerateSlots();
    }

    private void Update()
    {
        if (!isPlaced)
        {
            // cursor.posInTerrain += new Vector3Int(1, 0, 1);
            transform.position = new Vector3
                (
                    (gameManager.cursorManagement.posInTerrain.x) * gameManager.gridManagement.cellSize,
                    gameManager.cursorManagement.posInTerrain.y + storageBayHeight,
                    (gameManager.cursorManagement.posInTerrain.z) * gameManager.gridManagement.cellSize
                );
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 cursorPosition = new Vector3
                (
                    (gameManager.cursorManagement.posInTerrain.x) * gameManager.gridManagement.cellSize + gameManager.gridManagement.cellSize * 0.5f,
                    gameManager.cursorManagement.posInTerrain.y + storageBayHeight,
                    (gameManager.cursorManagement.posInTerrain.z) * gameManager.gridManagement.cellSize + gameManager.gridManagement.cellSize * 0.5f
                );
                if (CanBePlaced(cursorPosition))
                {
                    PlaceBay(gameManager.cursorManagement.posInTerrain);
                }
                else
                {
                    gameManager.errorDisplay.ShowError("You can't place it here");
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
        gameManager.sfxManager.PlaySoundLinked("BlockDropScientific", this.gameObject,0.1f,1,false);
        gameManager.cursorManagement.canSwitchTools = true;
        isPlaced = true;
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < gameManager.gridManagement.maxHeight; y++)
                {
                    Vector3Int _pos = new Vector3Int();
                    _pos.x = Mathf.RoundToInt(cursorCoordinates.x - x);
                    _pos.y = y;
                    _pos.z = Mathf.RoundToInt(cursorCoordinates.z - z);
                    gameManager.gridManagement.grid[_pos.x, _pos.y, _pos.z] = this.gameObject;
                }
            }
        }
    }

    private bool CanBePlaced(Vector3 positionToCompare) //Compare toutes les positions du bloc avec la position ou se trouve la souris pour savoir si le bloc est plaçable
    {
        foreach (GameObject t in slots)
        {
            if(gameManager.gridManagement.myTerrain.SampleHeight(t.transform.position) < (positionToCompare.y - flexibility) || Terrain.activeTerrain.SampleHeight(t.transform.position) > (positionToCompare.y + flexibility))
            {
                return false;
            }
        }
        return true;
    }

    //Genere un block à l'intérieur de la baie de stockage
    public void GenerateBlock()
    {
        GameObject newBlock = Instantiate(gameManager.library.blockPrefab);
        newBlock.GetComponent<BlockLink>().container.DropBlock();
        StoreBlock(newBlock);
    }

    //Livre un block à l'intérieur de la baie de stockage
    public void DeliverBlock(Block blockInfo)
    {
        GameObject newBlock = Instantiate(gameManager.library.blockPrefab);
        BlockLink newBlockLink = newBlock.GetComponent<BlockLink>();
        newBlockLink.block = blockInfo;
        newBlockLink.container.DropBlock();
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
                            StoreAtPosition(blockToStore, new Vector3Int(x, y, z));
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

    public void StoreAtPosition(GameObject blockToStore, Vector3Int position)
    {
        //Stockage du bloc
        storedBlocks[position.x, position.y, position.z] = blockToStore;
        blockToStore.transform.position = slots[position.x, position.z].transform.position;
        blockToStore.transform.position += new Vector3(0, position.y + 0.5f, 0);
        blockToStore.transform.SetParent(slots[position.x, position.z].transform);
        blockToStore.transform.localScale = Vector3.one;
        blockToStore.name = "StoredBlock[" + position + "]";
        BlockLink blockInfo = blockToStore.GetComponent<BlockLink>();
        blockInfo.gridCoordinates = position;
        blockInfo.container.CloseContainer();
        blockToStore.layer = LayerMask.NameToLayer("StoredBlock");
        if (blockInfo.container.isFalling == false) {
            gameManager.sfxManager.PlaySoundLinked("BlockDrop", blockToStore);
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

            block.GetComponent<BlockLink>().container.OpenContainer();
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
            gameManager.errorDisplay.ShowError("Block can't be destocked");
        }
    }
}
