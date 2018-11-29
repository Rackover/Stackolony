using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDebugger : MonoBehaviour {

    [System.Serializable]
    public class DebugGrid
    {
        public GameObject[,,] grid;
        public GameObject cellsParent;
        public string gridName;
        public int gridID;
        public bool isActive;
    }

    [Header("DebugGrid")]
    public GameObject debugCellEmpty;
    public GameObject debugCellBridge;
    public GameObject debugCellCube;
    public GameObject debugCellOther;

    public GameObject debugCellPower;

    List<DebugGrid> debugGrids = new List<DebugGrid>();

    public DebugGrid selectedGrid;

    public GridManagement gridmanager;
    private GameObject debugParentDefault; //Objet parent de tout les visuels
    private bool debugModIsActive;
    //Genere une grille 3D pour pouvoir débug la variable grid[,,] et afficher ce qu'elle contient

    public void InitAllGrids()
    {
        foreach (DebugGrid debugGrid in debugGrids)
        {
            InitGrid(debugGrid);
        }
    }
    public void InitGrid(DebugGrid debugGrid) //Initialise la grille
    {
        debugGrid.grid = new GameObject[gridmanager.grid.GetLength(0), gridmanager.grid.GetLength(1), gridmanager.grid.GetLength(2)];
        debugGrid.cellsParent = new GameObject();
        debugGrid.cellsParent.name = debugGrid.gridName;
        debugGrid.cellsParent.transform.parent = this.transform;
        debugGrid.cellsParent.SetActive(false);
        debugGrid.isActive = false;
        UpdateDebugGrid(debugGrid);
    }

    //Active la grille selectionnée
    public void SelectGrid(int gridID)
    {
        foreach (DebugGrid checkedGrid in debugGrids)
        {
            if (checkedGrid.gridID == gridID)
            {
                selectedGrid = checkedGrid;
                selectedGrid.cellsParent.SetActive(true);
            } else
            {
                checkedGrid.cellsParent.SetActive(false);
            }
        }
    }


    //Nettoie les visuels à toutes les hauteurs
    public void ClearDebugGrid(DebugGrid selectedGrid)
    {
        foreach (GameObject go in selectedGrid.grid)
        {
            Destroy(go);
        }
    }

    //Genère les visuels à toutes les hauteurs
    public void UpdateDebugGrid(DebugGrid selectedGrid)
    {
        for (int i = 0; i < gridmanager.grid.GetLength(1); i++)
        {
            UpdateDebugGridAtHeight(selectedGrid, i);
        }
    }

    //Nettoie les visuels à une hauteur définie
    public void ClearDebugGridAtHeigh(DebugGrid selectedGrid, int height)
    {
        for (int x = 0; x < selectedGrid.grid.GetLength(0); x++)
        {
            for (int z = 0; z < selectedGrid.grid.GetLength(2); z++)
            {
                if (selectedGrid.grid[x, height, z] != null)
                {
                    Destroy(selectedGrid.grid[x, height, z].gameObject);
                }
            }
        }
    }

    //Genère les visuels à une hauteur définie
    public void UpdateDebugGridAtHeight(DebugGrid selectedGrid, int height)
    {
        //Clean les blocs à la hauteur définie
        ClearDebugGridAtHeigh(selectedGrid, height);

        //Genère les blocs à la hauteur définie
        for (int x = 0; x < gridmanager.grid.GetLength(0); x++)
        {
            for (int z = 0; z < gridmanager.grid.GetLength(2); z++)
            {
                GameObject generatedCube = GenerateDebugCell(new Vector3Int(x,height,z),selectedGrid);
             
                if (generatedCube != null)
                {
                    selectedGrid.grid[x, height, z] = generatedCube;
                    generatedCube.transform.parent = selectedGrid.cellsParent.transform;
                    float cellSize = gridmanager.cellSize;
                    generatedCube.transform.position = new Vector3
                    (
                        x * cellSize + (cellSize / 2),
                        height + 0.5f,
                        z * cellSize + (cellSize / 2)
                    );
                }
            }
        }
    }

    //Instantie un bloc de débug à l'endroit souhaité, si les conditions pour l'instantier sont verifiées (Conditions variables selon le mod de debug selectionné)
    public GameObject GenerateDebugCell(Vector3Int position, DebugGrid selectedGrid)
    {
        GameObject foundObject = gridmanager.grid[position.x, position.y, position.z];
        BlockLink foundBlockLink = null;
        if (foundObject != null)
        {
            foundBlockLink = foundObject.GetComponent<BlockLink>();
        }

        switch (selectedGrid.gridID)
        {
            //Grille par défault
            case 0:
                if (foundObject != null)
                {
                    switch (foundObject.tag)
                    {
                        case "Bridge":
                            return Instantiate(debugCellBridge);
                        default:
                            if (foundObject.layer == LayerMask.NameToLayer("Block"))
                            {
                                return Instantiate(debugCellCube);
                            }
                            else
                            {
                                return Instantiate(debugCellOther);
                            }
                    }
                } break;

            //Grille "Power"
            case 1: 
                if (foundBlockLink !=null)
                {
                    GameObject generatedBlock = Instantiate(debugCellPower);
                    generatedBlock.GetComponent<Renderer>().material.color = new Color32(100, 100, 100, 255);
                    return generatedBlock;
                }
                break;
            default: break;
        }
        return null;
    }
}
