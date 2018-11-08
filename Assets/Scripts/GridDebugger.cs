using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDebugger : MonoBehaviour {
    [Header("DebugGrid")]
    public GameObject debugCellEmpty;
    public GameObject debugCellBridge;
    public GameObject debugCellCube;
    public GameObject debugCellOther;

    public GameObject[,,] debugGrid;

    public GridManagement gridmanager;
    private GameObject debugParent; //Objet parent de tout les visuels
    private bool debugModIsActive;
    //Genere une grille 3D pour pouvoir débug la variable grid[,,] et afficher ce qu'elle contient

    public void InitGrid() //Initialise la grille
    {
        debugGrid = new GameObject[gridmanager.grid.GetLength(0), gridmanager.grid.GetLength(1), gridmanager.grid.GetLength(2)];
        debugParent = new GameObject();
        debugParent.name = "DebugParent";
        debugParent.transform.parent = this.transform;
        debugParent.SetActive(false);
        debugModIsActive = false;
        GenerateDebugGrid();
    }

    //Active ou désactive le mod débug
    public void ToggleDebugMod()
    {
        if (debugModIsActive)
        {
            debugModIsActive = false;
            debugParent.SetActive(false);
        } else
        {
            debugModIsActive = true;
            debugParent.SetActive(true);
        }
    }

    //Nettoie les visuels à toutes les hauteurs
    public void ClearDebugGrid()
    {
        foreach (GameObject go in debugGrid)
        {
            Destroy(go);
        }
    }

    //Nettoie les visuels à une hauteur définie
    public void ClearDebugGridAtHeigh(int height)
    {
        for (int x = 0; x < debugGrid.GetLength(0); x++)
        {
            for (int z = 0; z < debugGrid.GetLength(2); z++)
            {
                if (debugGrid[x, height, z] != null)
                {
                    Destroy(debugGrid[x, height, z].gameObject);
                }
            }
        }
    }

    //Genère les visuels à toutes les hauteurs
    public void UpdateDebugGrid()
    {
        ClearDebugGrid();
        GenerateDebugGrid();
    }

    //Genère les visuels à une hauteur définie
    public void UpdateDebugGridAtHeight(int height)
    {
        //Clean les blocs à la hauteur définie
        ClearDebugGridAtHeigh(height);

        //Genère les blocs à la hauteur définie
        for (int x = 0; x < gridmanager.grid.GetLength(0); x++)
        {
            for (int z = 0; z < gridmanager.grid.GetLength(2); z++)
            {
                GameObject cubeFound = gridmanager.grid[x, height, z];
                GameObject generatedCube = null;
                if (cubeFound == null)
                {
                    generatedCube = null;
                }
                else
                {
                    switch (cubeFound.tag)
                    {
                        case "Bridge":
                            generatedCube = Instantiate(debugCellBridge);
                            break;
                        default:
                            if (cubeFound.layer == LayerMask.NameToLayer("Block"))
                            {
                                generatedCube = Instantiate(debugCellCube);
                            }
                            else
                            {
                                generatedCube = Instantiate(debugCellOther);
                            }
                            break;
                    }
                }
                if (generatedCube != null)
                {
                    debugGrid[x, height, z] = generatedCube;
                    generatedCube.transform.parent = debugParent.transform;
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

    public void GenerateDebugGrid()
    {
        for (int x = 0; x < gridmanager.grid.GetLength(0); x++)
        {
            for (int z = 0; z < gridmanager.grid.GetLength(2); z++)
            {
                for (int y = 0; y < gridmanager.grid.GetLength(1); y++)
                {
                    GameObject cubeFound = gridmanager.grid[x, y, z];
                    GameObject generatedCube = null;
                    if (cubeFound == null)
                    {
                        generatedCube = null;
                    }
                    else
                    {
                        switch (cubeFound.tag)
                        {
                            case "Bridge":
                                generatedCube = Instantiate(debugCellBridge);
                                break;
                            default:
                                if (cubeFound.layer == LayerMask.NameToLayer("Block"))
                                {
                                    generatedCube = Instantiate(debugCellCube);
                                }
                                else
                                {
                                    generatedCube = Instantiate(debugCellOther);
                                }
                                break;
                        }
                    }
                    if (generatedCube != null)
                    {
                        if (debugParent != null)
                        {
                            generatedCube.transform.parent = debugParent.transform;
                        }
                        else
                        {
                            Destroy(generatedCube);
                            Debug.LogWarning("Error ! No debugParent found");
                        }
                        debugGrid[x,y,z] = generatedCube;
                        float cellSize = gridmanager.cellSize;
                        generatedCube.transform.position = new Vector3
                        (
                            x * cellSize + (cellSize / 2),
                            y + 0.5f,
                            z * cellSize + (cellSize / 2)
                        );
                    }
                }
            }
        }
    }
}
