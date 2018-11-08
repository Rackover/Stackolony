using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockingBay : MonoBehaviour {

    [Header("SETTINGS")]
    [Space(1)]
    public int stockSize; //Combien de blocs peut on mettre par colonne
    public int size; //Combien de cellules prend l'objet, on veut savoir la racine carrée (2 = 2x2, 3 = 3x3 etc...)
    public GameObject[,] slots; //Liste des slots

    [Header("REFERENCES")]
    [Space(1)]
    public CursorManagement cursor;
    public GridManagement gridManager;
    public Interface uiManager;



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

        //Empêche de changer d'outil pendant qu'on est supposé placer la stocking bay
        cursor.switchMode(CursorManagement.cursorMode.Default);
        cursor.canSwitchTools = false;

        GenerateSlots();
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
                } else
                {
                    uiManager.ShowError("You can't place it here");
                }
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
}
