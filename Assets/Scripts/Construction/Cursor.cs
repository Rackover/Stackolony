using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Cursor : MonoBehaviour {

    public enum cursorMode { Drag, Build, Delete, Bridge }; //Chaque mode du curseur

    [Header("=== REFERENCIES ===")][Space(1)]
    [Header("Prefabs")]
    public GameObject projectorPrefab;
    public GameObject blockDefaultPrefab;
    public GameObject stackSelectorPrefab;  //Prefab de la petite fléche qui se met au pied de la tour qu'on selectionne
    public GameObject highlighter; // Curseur permettant de surligner / mettre en valeur des blocks (exemple : lors du traçage de pont ou de la délétion)
    public GameObject bridgeHighlighter;
    
    public cursorMode selectedMode = cursorMode.Drag; //Mode actuel du curseur
    private GameObject[] activeHighlighters; //Pool contenant plusieurs highlighters actifs
    private GameObject hoveredBlock;

    [Header("Scripts")]
    public TempDrag drag;
    public GridManagement gridManager; // Référence au gridManagement.cs instancié
    public InterfaceManager uiManager;

    [Space(5)][Header("=== DEBUG ===")][Space(1)]
    public Vector3Int posInTerrain; //Position de la souris sur le terrain
    public BlockLink selectedBlock; //Le block selectionné par le joueur
    private GameObject stackSelector; //La petite fléche qui se met au pied de la tour qu'on selectionne
    private GameObject myProjector;
    Terrain terr; //Terrain principal sur lequel le joueur pourra placer des blocs

    Vector2Int heightmapSize;

    private void Start()
    {
        hoveredBlock = null;
        switchMode(cursorMode.Build);
        gridManager = FindObjectOfType<GridManagement>();

        //Instantie un projecteur de selection qui se clamp sur les cellules de la grille
        myProjector = Instantiate(projectorPrefab);
        myProjector.name = "Projector";
        myProjector.GetComponent<Projector>().orthographicSize = 2.5f * gridManager.cellSize; //On adapte la taille du projecteur (qui projette la grille au sol) à la taille des cellules
        myProjector.GetComponent<Projector>().enabled = false;

        //Instantie la fleche qui indique la tour selectionnée par le joueur
        stackSelector = Instantiate(stackSelectorPrefab);
        stackSelector.name = "Stack_Selector";
        stackSelector.SetActive(false);

        //Recupere le terrain et ses dimensions
        terr = Terrain.activeTerrain;
        heightmapSize = new Vector2Int(terr.terrainData.heightmapWidth, terr.terrainData.heightmapHeight);

    }
    private void Update()
    {
        UpdateCursor();
    }

    public void switchMode(cursorMode mode)
    {
        selectedMode = mode;
        uiManager.txtMode.text = mode.ToString();
        ClearFeedback();
    }


    //Place le curseur correctement sur la grille et sur le terrain
    private void UpdateCursor()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            UpdatePosition(hit);
            UpdateMouse(hit);
            UpdateProjector();

            if ((hit.collider.gameObject.layer == LayerMask.NameToLayer("Block")) && hoveredBlock != hit.collider.gameObject && (!Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)))
            {
                hoveredBlock = hit.collider.gameObject;

                UpdateFeedback(hit);
            }

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                UpdateFeedback(hit);
            }

        }
        else // If the mouse is pointing at nothing
        {
            myProjector.GetComponent<Projector>().enabled = false;
            stackSelector.SetActive(false);
        }
        transform.position = hit.point;
    }

    void UpdatePosition(RaycastHit hit)
    {
        Vector3 tempCoord = hit.point; //On adapte la position de la souris pour qu'elle corresponde à la taille des cellules
        Vector3 coord = new Vector3(
            tempCoord.x / gridManager.cellSize,
            tempCoord.y, //La taille en y n'est pas dépendante de la taille des cellules, elle vaudra toujours 1
            tempCoord.z / gridManager.cellSize
        );

        //Converti la position pour savoir sur quelle case se trouve la souris
        posInTerrain = new Vector3Int(
            Mathf.FloorToInt(coord.x),
            Mathf.FloorToInt(coord.y + 0.01f),
            Mathf.FloorToInt(coord.z)
        );
    }

    void UpdateProjector()
    {
        //Met à jour la position du projecteur
        myProjector.GetComponent<Projector>().enabled = true;
        myProjector.transform.position = new Vector3(
            posInTerrain.x * gridManager.cellSize + (gridManager.cellSize / 2),
            posInTerrain.y + 10,
            posInTerrain.z * gridManager.cellSize + (gridManager.cellSize / 2)
        );
    }

    void ClearFeedback()
    {
        HighlightBlock();
        HighlightMultipleBlocks();
    }

    void UpdateFeedback(RaycastHit hit)
    {
        // Highlighting block the cursor currently is on if we're in Bridge mode
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block")) 
        {
            if (selectedMode == cursorMode.Delete) 
            {
                HighlightBlock(hit.transform.gameObject);
                HighlightMultipleBlocks();
            }
            else  if (selectedMode == cursorMode.Bridge)
            {
                HighlightMultipleBlocks(CheckBridgeableBlocks(posInTerrain));
                HighlightBlock(hit.transform.gameObject);
            } else
            {
                ClearFeedback();
            }
        }

        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block")) //Si le joueur a la souris sur un block, alors on recupere le bloc le plus bas dans la tour selectionnée
        {
            int minHeight = 0;
            for (var i = posInTerrain.y-1; i > 0; i--) //Calcule du pied de la tour selectionnée
            {
                if (gridManager.grid
                    [
                        posInTerrain.x,
                        i,
                        posInTerrain.z
                    ] == null)
                {
                    minHeight = i+1;
                    break;
                }
            }
            stackSelector.SetActive(true);
            stackSelector.transform.position = new Vector3(
                posInTerrain.x * gridManager.cellSize + (gridManager.cellSize / 2), 
                minHeight+0.1f, 
                posInTerrain.z * gridManager.cellSize + (gridManager.cellSize / 2));
        } 
        else
        {
            stackSelector.SetActive(false);
        }
    }

    void UpdateMouse(RaycastHit hit)
    {
        // Mouse click down
        if(Input.GetButtonDown("MouseLeft"))
        {
            switch (selectedMode) {
                default:
                    Debug.LogWarning("The selected cursor mode has no code associated to it! Check Cursor.cs/UseTool");
                    break;

                case cursorMode.Build:
                    if (!EventSystem.current.IsPointerOverGameObject()) 
                    {
                        gridManager.SpawnBlock(blockDefaultPrefab, new Vector2Int(posInTerrain.x, posInTerrain.z));
                    }
                    break;
                case cursorMode.Delete:
                    gridManager.DestroyBlock(posInTerrain);
                    ClearFeedback();
                    break;

                case cursorMode.Bridge:
                    TryToMakeBridge(hit.transform.gameObject);
                    break;

                case cursorMode.Drag:
                    BlockLink sBlock = hit.transform.gameObject.GetComponent<BlockLink>(); 
                    if(sBlock != null)
                    {
                        drag.StartDrag(sBlock);
                    }
                    break;
            }
        }
        if(Input.GetButtonDown("MouseRight"))
        {
            if (selectedMode == cursorMode.Bridge)
            {
                ResetSelection();
            }
        }

        // Mouse click hold
        if(Input.GetButton("MouseLeft"))
        {
            switch (selectedMode) 
            {
                default:
                    Debug.LogWarning("The selected cursor mode has no code associated to it! Check Cursor.cs/UseTool");
                    break;
                case cursorMode.Drag:
                    drag.DuringDrag(hit.point);
                    break;
            }
        }  
        if(Input.GetButton("MouseRight"))
        {
            switch (selectedMode) {
                default:
                    Debug.LogWarning("The selected cursor mode has no code associated to it! Check Cursor.cs/UseTool");
                    break;
                 case cursorMode.Drag:
                    drag.CancelDrag();
                    break;
            }
        }
    
        // Mouse click up
        if(Input.GetButtonUp("MouseLeft"))
        {
            switch (selectedMode)
            {
                default:
                    Debug.LogWarning("The selected cursor mode has no code associated to it! Check Cursor.cs/UseTool");
                    break;
                case cursorMode.Drag:
                    drag.EndDrag(hit.point);
                    break;
            }
        }
    }

    void ResetSelection()
    {
        selectedBlock = null;
    }

    void HighlightBlock(GameObject block = null)
    {
        if (block != null) {
            Debug.Log("IsntNull");
            highlighter.SetActive(true);
            Vector3 bounds = block.GetComponent<BoxCollider>().bounds.size;
            highlighter.transform.position = block.transform.position;
        }
        else {
            highlighter.SetActive(false);
        }
    }

    Vector3Int[] CheckBridgeableBlocks(Vector3Int coordinate) //Renvoi une liste des coordonnées des blocks reliables au bloc se trouvant aux coordonnées entrées
    {
        List<Vector3Int> coordinatesFound = new List<Vector3Int>();
        //Check les blocs devant la face x
        for (int i = coordinate.x+1; i < gridManager.grid.GetLength(0); i++)
        {
            if (gridManager.grid[i,coordinate.y,coordinate.z] != null)
            {
                coordinatesFound.Add(new Vector3Int(i, coordinate.y, coordinate.z));
                break;
            }
        }

        //Check les blocs derriere la face x
        for (int i = coordinate.x-1; i >= 0; i--)
        {
            if (gridManager.grid[i, coordinate.y, coordinate.z] != null)
            {
                coordinatesFound.Add(new Vector3Int(i, coordinate.y, coordinate.z));
                break;
            }
        }

        //Check les blocs devant la face z
        for (int i = coordinate.z+1; i < gridManager.grid.GetLength(2); i++)
        {
            if (gridManager.grid[coordinate.x, coordinate.y, i] != null)
            {
                coordinatesFound.Add(new Vector3Int(coordinate.x, coordinate.y, i));
                break;
            }
        }

        //Check les blocs derriere la face z
        for (int i = coordinate.z-1; i >= 0; i--)
        {
            if (gridManager.grid[coordinate.x, coordinate.y, i] != null)
            {
                coordinatesFound.Add(new Vector3Int(coordinate.x, coordinate.y, i));
                break;
            }
        }

        return coordinatesFound.ToArray();
    }
    
    void HighlightMultipleBlocks(Vector3Int[] blocksCoordinates = null) //Instantie un highlighter pour plusieurs blocks, si aucun argument n'est spécifié alors il clean tout
    {
        // Clean les highlighters
        if (activeHighlighters != null && activeHighlighters.Length > 0)
        {
            foreach (GameObject activeHighlighter in activeHighlighters)
            {
                Destroy(activeHighlighter);
            }
            activeHighlighters = new GameObject[0]; //Clean de l'array contenant les highlighters
        }

        if (blocksCoordinates != null)
        {
            //Genere les nouveaux highlighters
            activeHighlighters = new GameObject[blocksCoordinates.Length];
            int i = 0;
            foreach (Vector3Int coordinate in blocksCoordinates)
            {
                if (gridManager.grid[coordinate.x, coordinate.y, coordinate.z] != null)
                {
                    GameObject newHighlighter = Instantiate(highlighter, highlighter.transform.parent);
                    newHighlighter.transform.localPosition = gridManager.grid[coordinate.x, coordinate.y, coordinate.z].transform.position;
                    newHighlighter.SetActive(true);
                    activeHighlighters[i] = newHighlighter;
                    i++;
                }
                else
                {
                    Debug.LogWarning("Coordinate is out of grid, can't draw the highlighter");
                }
            }
        }
    }

    /// <summary>
    /// If the user had a blocklink in selectedBlock, will try to create a bridge between the previous selection and the current selection
    /// Else, will select the block as a candidate for bridging
    /// </summary>
    /// <param name="hitGameObject"></param>

    void TryToMakeBridge(GameObject hitGameObject)
    {
        if (hitGameObject.GetComponent<BlockLink>() != null) 
        {
            if (selectedBlock == null) //Si le joueur n'a pas fait sa premiere selection, alors il la fait
            {
                selectedBlock = hitGameObject.GetComponent<BlockLink>();
            }
            else //Si le joueur a déjà fait sa premiere selection, on vérifie que le deuxieme bloc selectionné est en face du premier, puis on trace le pont
            {
                BlockLink destinationCandidate = hitGameObject.GetComponent<BlockLink>(); //On selectionne temporairement le second block, puis on vérifie s'il remplie les conditions pour tracer un pont
                if (destinationCandidate.gridCoordinates != selectedBlock.gridCoordinates) {
                    if (destinationCandidate.gridCoordinates.y == selectedBlock.gridCoordinates.y) {
                        if (destinationCandidate.gridCoordinates.x == selectedBlock.gridCoordinates.x || destinationCandidate.gridCoordinates.z == selectedBlock.gridCoordinates.z) {
                            //Les conditions sont remplies et on peut tracer le pont
                            //Call de la fonction pour tracer un pont
                            gridManager.CreateBridge(selectedBlock, destinationCandidate);
                        }
                        else {
                            uiManager.ShowError("You can't link two blocks that aren't aligned");
                            Debug.LogWarning("You can't link two blocks that aren't aligned");
                        }
                    }
                    else {
                        uiManager.ShowError("You can't link two blocks of different heights");
                        Debug.LogWarning("You can't link two blocks of different heights");
                    }
                }
                else {
                    uiManager.ShowError("You can't select the same point");
                    Debug.LogWarning("You can't select the same point");
                }
                ResetSelection();
            }
        }
    }

    /// UI FUNCTIONS
    /// To be fired ONLY from interface buttons
    /// do NOT use in code
    public void switchToDragModeFromButton()
    {
        switchMode(cursorMode.Drag);
    }
    public void switchToBuildModeFromButton()
    {
        switchMode(cursorMode.Build);
    }
    public void switchToDeleteModeFromButton()
    {
        switchMode(cursorMode.Delete);
    }
    public void switchToBridgeModeFromButton()
    {
        switchMode(cursorMode.Bridge);
    }
}


