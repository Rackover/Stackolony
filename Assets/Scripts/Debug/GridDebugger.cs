using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridDebugger : MonoBehaviour {

    [System.Serializable]
    public class OverlayMode
    {
        [System.NonSerialized]
        public GameObject cellsParent;
        public Button linkedButton;
        public string gridName;
        public int gridID;
        [System.NonSerialized]
        public bool isActive;
        [System.NonSerialized]
        public GameObject[,,] grid;
    }

    [Header("OverlayMode")]
    public GameObject debugCellEmpty;
    public GameObject debugCellBridge;
    public GameObject debugCellCube;
    public GameObject debugCellOther;
    public GameObject debugCellUnpowered;
    public GameObject debugCellPower;
    public GameObject debugCellGenerator;

    public List<OverlayMode> overlayModes = new List<OverlayMode>();

    [System.NonSerialized]
    public OverlayMode selectedGrid;

    public GridManagement gridManager;

    //Genere une grille 3D pour pouvoir débug la variable grid[,,] et afficher ce qu'elle contient
    public void InitAllGrids()
    {
        foreach (OverlayMode overlayMode in overlayModes) {
            Logger.Debug("Initializing debug grid [" + overlayMode.ToString() + "]");
            InitGrid(overlayMode);
        }
    }

    public void InitButtons()
    {
        foreach (OverlayMode overlayMode in overlayModes)
        {
            overlayMode.linkedButton.onClick.AddListener(delegate { SelectGrid(overlayMode.gridID); });
        }
        SelectGrid(0);
    }

    public void InitGrid(OverlayMode overlayMode) //Initialise la grille
    {
        overlayMode.grid = new GameObject[gridManager.grid.GetLength(0), gridManager.grid.GetLength(1), gridManager.grid.GetLength(2)];
        overlayMode.cellsParent = new GameObject();
        overlayMode.cellsParent.name = overlayMode.gridName;
        overlayMode.cellsParent.transform.parent = this.transform;
        overlayMode.cellsParent.SetActive(false);
        overlayMode.isActive = false;
        UpdateOverlayMode(overlayMode);
    }

    //Active la grille selectionnée
    public void SelectGrid(int gridID)
    {
        foreach (OverlayMode checkedGrid in overlayModes)
        {
            if (checkedGrid.gridID == gridID)
            {
                selectedGrid = checkedGrid;
                selectedGrid.cellsParent.SetActive(true);
                selectedGrid.linkedButton.GetComponent<Animator>().SetBool("isActive", true);
            } else
            {
                checkedGrid.cellsParent.SetActive(false);
                checkedGrid.linkedButton.GetComponent<Animator>().SetBool("isActive", false);
            }
        }
    }

    //Utilisé par le bouton "Update grid"
    public void UpdateSelectedGrid()
    {
        UpdateOverlayMode(selectedGrid);
    }


    //Nettoie les visuels à toutes les hauteurs
    public void ClearOverlayMode(OverlayMode selectedGrid)
    {
        foreach (GameObject go in selectedGrid.grid)
        {
            Destroy(go);
        }
    }

    //Genère les visuels à toutes les hauteurs
    public void UpdateOverlayMode(OverlayMode selectedGrid)
    {
        for (int i = 0; i < gridManager.grid.GetLength(1); i++)
        {
            UpdateOverlayModeAtHeight(selectedGrid, i);
        }
    }

    //Nettoie les visuels à une hauteur définie
    public void ClearOverlayModeAtHeigh(OverlayMode selectedGrid, int height)
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
    public void UpdateOverlayModeAtHeight(OverlayMode selectedGrid, int height)
    {
        //Clean les blocs à la hauteur définie
        ClearOverlayModeAtHeigh(selectedGrid, height);

        //Genère les blocs à la hauteur définie
        for (int x = 0; x < gridManager.grid.GetLength(0); x++)
        {
            for (int z = 0; z < gridManager.grid.GetLength(2); z++)
            {
                GameObject generatedCube = GenerateDebugCell(new Vector3Int(x,height,z),selectedGrid);
             
                if (generatedCube != null)
                {
                    selectedGrid.grid[x, height, z] = generatedCube;
                    generatedCube.transform.parent = selectedGrid.cellsParent.transform;
                    float cellSize = gridManager.cellSize;
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
    public GameObject GenerateDebugCell(Vector3Int position, OverlayMode selectedGrid)
    {
        GameObject foundObject = gridManager.grid[position.x, position.y, position.z];
        BlockLink foundBlockLink = null;
        if (foundObject != null)
        {
            foundBlockLink = foundObject.GetComponent<BlockLink>();
        }

        switch (selectedGrid.gridID)
        {
            //Grille "Default"
            case 0:
                return null;
            //Grille "Debug"
            case 1:
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
            case 2: 
                if (foundBlockLink !=null)
                {
                    if (foundBlockLink.gameObject.GetComponent<Generator>() != null)
                    {
                        GameObject generatedBlock = Instantiate(debugCellGenerator);
                        return generatedBlock;
                    }
                    else if (foundBlockLink.currentPower > 0)
                    {
                        GameObject generatedBlock = Instantiate(debugCellPower);
                        generatedBlock.GetComponent<Renderer>().material.color = new Color32(100, 100, 100, 255);
                        return generatedBlock;
                    } else if (foundBlockLink.block.consumption > 0)
                    {
                        GameObject generatedBlock = Instantiate(debugCellUnpowered);
                        return generatedBlock;
                    }
                }
                break;
            default: break;
        }
        return null;
    }
}
