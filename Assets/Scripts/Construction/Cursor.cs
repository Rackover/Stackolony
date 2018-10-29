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
    
    public cursorMode selectedMode = cursorMode.Drag; //Mode actuel du curseur

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
    int hmWidth; //Heightmap du terrain
    int hmHeight; //Heightmap du terrain

    private void Start()
    {
        switchMode(cursorMode.Build);

        //Instantie un projecteur de selection qui se clamp sur les cellules de la grille
        myProjector = Instantiate(projectorPrefab);
        myProjector.name = "Projector";
        myProjector.GetComponent<Projector>().orthographicSize = 2.5f * GridManagement.cellSizeStatic; //On adapte la taille du projecteur (qui projette la grille au sol) à la taille des cellules
        myProjector.GetComponent<Projector>().enabled = false;

        //Instantie la fleche qui indique la tour selectionnée par le joueur
        stackSelector = Instantiate(stackSelectorPrefab);
        stackSelector.name = "Stack_Selector";
        stackSelector.SetActive(false);

        //Recupere le terrain et ses dimensions
        terr = Terrain.activeTerrain;
        hmWidth = terr.terrainData.heightmapWidth;
        hmHeight = terr.terrainData.heightmapHeight;

        gridManager = FindObjectOfType<GridManagement>();
    }
    private void Update()
    {
        UpdateCursor();
    }

    public void switchMode(cursorMode mode)
    {
        selectedMode = mode;
        uiManager.txtMode.text = mode.ToString();
    }


    //Place le curseur correctement sur la grille et sur le terrain
    private void UpdateCursor()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            UpdatePosition(hit);
            UpdateFeedback(hit);
            UpdateMouse(hit);
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
            tempCoord.x / GridManagement.cellSizeStatic,
            tempCoord.y, //La taille en y n'est pas dépendante de la taille des cellules, elle vaudra toujours 1
            tempCoord.z / GridManagement.cellSizeStatic
        );

        //Converti la position pour savoir sur quelle case se trouve la souris
        posInTerrain = new Vector3Int(
            Mathf.FloorToInt(coord.x),
            Mathf.FloorToInt(coord.y + 0.01f),
            Mathf.FloorToInt(coord.z)
        );
    }

    void UpdateFeedback(RaycastHit hit)
    {
        //Met à jour la position du projecteur
        myProjector.GetComponent<Projector>().enabled = true;
        myProjector.transform.position = new Vector3(
            posInTerrain.x* GridManagement.cellSizeStatic + (GridManagement.cellSizeStatic/2),
            posInTerrain.y + 10,
            posInTerrain.z * GridManagement.cellSizeStatic + (GridManagement.cellSizeStatic / 2)
        );

        // Highlighting block the cursor currently is on if we're in Bridge mode
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block")) 
        {
            if (selectedMode == cursorMode.Bridge ||
                selectedMode == cursorMode.Delete) 
            {
                HighlightBlock(hit.transform.gameObject);
            }
            else 
            {
                HighlightBlock();
            }
        }

        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block")) //Si le joueur a la souris sur un block, alors on recupere le bloc le plus bas dans la tour selectionnée
        {
            int minHeight = 0;
            for (var i = posInTerrain.y-1; i > 0; i--) //Calcule du pied de la tour selectionnée
            {
                if (GridManagement.grid
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
                posInTerrain.x * GridManagement.cellSizeStatic + (GridManagement.cellSizeStatic / 2), 
                minHeight+0.1f, 
                posInTerrain.z * GridManagement.cellSizeStatic + (GridManagement.cellSizeStatic / 2));
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
                        gridManager.GenerateBlock(blockDefaultPrefab, new Vector2Int(posInTerrain.x, posInTerrain.z));
                    }
                    break;
                case cursorMode.Delete:
                    gridManager.DestroyBlock(posInTerrain);
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
                    drag.DuringDrag(posInTerrain);
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
            highlighter.SetActive(true);
            Vector3 bounds = block.GetComponent<BoxCollider>().bounds.size;
            highlighter.transform.position = block.transform.position;
        }
        else {
            highlighter.SetActive(false);
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


