﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CursorManagement : MonoBehaviour
{
    public enum cursorMode { Default, Build, Delete, Bridge }; //Chaque mode du curseur

    [Header("=== REFERENCIES ===")]
    [Header("Prefabs")]
    public GameObject projectorPrefab;
    public GameObject stackSelectorPrefab;  //Prefab de la petite fléche qui se met au pied de la tour qu'on selectionne
    public GameObject highlighter; // Curseur permettant de surligner / mettre en valeur des blocks (exemple : lors du traçage de pont ou de la délétion)
    public GameObject bridgeHighlighter;
    public GameObject bridgePreview;

    [Space(5)]
    [Header("=== SYSTEM ===")]
    public cursorMode selectedMode = cursorMode.Default; //Mode actuel du curseur
    public List<GameObject> activeBridgePreviews = new List<GameObject>(); //Liste contenant les preview de pont
    public bool isBridging = false;
    public Block selectedBlock; //Le block selectionné par le joueur
    public int projectorHeight = 10;
    public GameObject myProjector;
    [Space(5)]
    
    [Header("=== DEBUG ===")]
    public Vector3Int posInGrid; //Position de la souris sur le terrain
    public Vector3 posInWorld;
    
    [HideInInspector] public bool cursorOnUI = false;
    [HideInInspector] public Vector3 sPosition;float dragDelay = 1f;
    float timer;
    bool draging;
    private Vector3Int savedPos;

    private GameObject[] activeHighlighters; //Liste contenant plusieurs highlighters actifs
    private List<GameObject> permanentHighlighter = new List<GameObject>(); 
    private GameObject hoveredBlock;
    private GameObject stackSelector; //La petite fléche qui se met au pied de la tour qu'on selectionne
    Terrain terr; //Terrain principal sur lequel le joueur pourra placer des blocs
    Vector2Int heightmapSize;
    [System.NonSerialized] public bool canSwitchTools = true;

    public void InitializeGameCursor()
    {
        hoveredBlock = null;
        //Instantie un projecteur de selection qui se clamp sur les cellules de la grille
        myProjector = Instantiate(projectorPrefab);
        myProjector.name = "Projector";
        float cellMedianSize = (GameManager.instance.gridManagement.cellSize.x + GameManager.instance.gridManagement.cellSize.z) / 2;
        myProjector.GetComponent<Projector>().orthographicSize = 2.5f * cellMedianSize; //On adapte la taille du projecteur (qui projette la grille au sol) à la taille des cellules
        myProjector.GetComponent<Projector>().enabled = false;

        //Instantie la fleche qui indique la tour selectionnée par le joueur
        stackSelector = Instantiate(stackSelectorPrefab);
        stackSelector.name = "Stack_Selector";
        stackSelector.SetActive(false);

        //Recupere le terrain et ses dimensions
        terr = Terrain.activeTerrain;
        heightmapSize = new Vector2Int(terr.terrainData.heightmapWidth, terr.terrainData.heightmapHeight);
    }
    public void KillGameCursor()
    {
        Destroy(stackSelector);
        Destroy(myProjector);
    }

    private void Update()
    {
        if (!GameManager.instance.IsInGame()){return;}

        UpdateCursor();

        if(EventSystem.current.IsPointerOverGameObject()){cursorOnUI = true;}

        else{cursorOnUI = false;}
    }

    public void SwitchMode(cursorMode mode)
    {
        if (canSwitchTools)
        {
            selectedMode = mode;
            ClearFeedback();
            ClearPermanentHighlighter();
            selectedBlock = null;
        } 
        else{GameManager.instance.errorDisplay.ShowError("You can't use that tool right now");}
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

            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Block") || hit.collider.gameObject.layer == LayerMask.NameToLayer("StoredBlock") && hoveredBlock != hit.collider.gameObject && !Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
            {
                hoveredBlock = hit.collider.gameObject;
                UpdateFeedback(hit);
            }
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain")) { UpdateFeedback(hit); }
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
        Vector3 tempCoord = hit.point;
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Terrain") ||
            cursorMode.Default == selectedMode) {
            //On adapte la position de la souris pour qu'elle corresponde à la taille des cellules
            tempCoord += new Vector3(0,
                GameManager.instance.gridManagement.cellSize.y / 2
            ); 
        }

        //Converti la position pour savoir sur quelle case se trouve la souris
        posInGrid = GameManager.instance.gridManagement.WorldPositionToIndex(tempCoord);
    }

    void UpdateProjector()
    {
        if (cursorOnUI)
        {
            myProjector.GetComponent<Projector>().enabled = false;
        } else
        {
            myProjector.GetComponent<Projector>().enabled = true;
        }
        //Met à jour la position du projecteur
        myProjector.transform.position = GameManager.instance.gridManagement.IndexToWorldPosition(posInGrid) + new Vector3(0, projectorHeight, 0);
    }

    void ClearFeedback()
    {
        HighlightBlock();
        HighlightMultipleBlocks();
        if (activeBridgePreviews.Count > 0)
            DestroyBridgePreview();
    }

    void UpdateFeedback(RaycastHit hit)
    {
        // Highlighting block the cursor currently is on if we're in Bridge mode
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block")) 
        {
            if (selectedMode == cursorMode.Delete) 
            {
                ClearFeedback();
                HighlightBlock(hit.transform.gameObject);
            }
            else if (selectedMode == cursorMode.Bridge) {

                GameObject objective = hit.transform.gameObject;
                Vector3Int position = posInGrid;
                // If the player is bridging, we stuck the preview to the block he's dragging the bridge from 
                if (isBridging) {
                    objective = selectedBlock.gameObject;
                    position = selectedBlock.gridCoordinates;
                }
                ClearFeedback();
                HighlightBlock(objective);
                Vector3Int[] linkableBlocksList = CheckBridgeableBlocks(position);
                if (linkableBlocksList.Length > 0) {
                    foreach (Vector3Int blockToBridge in linkableBlocksList) {
                        if (blockToBridge.Equals(hit.transform.gameObject.GetComponent<Block>().gridCoordinates)) {
                            HighlightBlock(hit.transform.gameObject);
                            GenerateBridgePreview(objective.GetComponent<Block>().gridCoordinates, blockToBridge);
                            break;
                        }
                    }
                }
            }
            else
            {
                ClearFeedback();
            }

        //Si le joueur a la souris sur un block, alors on recupere le bloc le plus bas dans la tour selectionnée
        
            int minHeight = 0;
            for (var i = posInGrid.y-1; i > 0; i--) //Calcule du pied de la tour selectionnée
            {
                if (GameManager.instance.gridManagement.grid
                    [
                        posInGrid.x,
                        i,
                        posInGrid.z
                    ] == null)
                {
                    minHeight = i+1;
                    break;
                }
            }
            stackSelector.SetActive(true);
            Vector3 stackPosition = GameManager.instance.gridManagement.IndexToWorldPosition(posInGrid);
            stackPosition.y = minHeight;
            stackSelector.transform.position = stackPosition;
        } 
        else
        {
            stackSelector.SetActive(false);
            ClearFeedback();
        }
    }

    void UpdateMouse(RaycastHit hit)
    {
        // Mouse click down
        if(Input.GetButtonDown("Select"))
        {
            switch (selectedMode) {
                case cursorMode.Default:
                    Block selectedBlock = hit.transform.gameObject.GetComponent<Block>();
                    // Debug.LogWarning("The selected cursor mode has no code associated to it! Check Cursor.cs/UseTool");
                    StartDrag(selectedBlock);
                    /*
                    if(!cursorOnUI)
                        selector.ShowBlock(selectedBlock);
                    */

                    break;
                

                /*
                case cursorMode.Build:
                    if (!EventSystem.current.IsPointerOverGameObject()) 
                    {
                        GameManager.instance.gridManagement.SpawnBlock(blockDefaultPrefab, new Vector2Int(posInGrid.x, posInGrid.z));
                    }
                    break;
                    */

                case cursorMode.Delete:
                    GameManager.instance.gridManagement.DestroyBlock(posInGrid);
                    ClearFeedback();
                    break;
            }
        }

        // Mouse click hold
        if(Input.GetButton("Select")) {
            switch (selectedMode) 
            {
                case cursorMode.Default:
                   // Debug.LogWarning("The selected cursor mode has no code associated to it! Check Cursor.cs/UseTool");
                    DuringDrag(posInGrid);
                    break;

                case cursorMode.Bridge:
                    if (!isBridging && hit.transform.gameObject.layer == LayerMask.NameToLayer("Block") ) {
                        StartPlanningBridge(hit.transform.gameObject);
                    }
                    break;
            }
        }  

        // Left Mouse up 
        if (Input.GetButtonUp("Select")) {
            switch (selectedMode) {
                case cursorMode.Default:
                    //   Debug.LogWarning("The selected cursor mode has no code associated to it! Check Cursor.cs/UseTool");
                    EndDrag(posInGrid);
                    break;

                case cursorMode.Bridge:
                    if (isBridging) {
                        isBridging = false;
                        TryToMakeBridge(hit.transform.gameObject);
                        CancelPotentialBridge();
                    }
                    break;
            }
        }

        // Right mouse button
        if(Input.GetButton("RotateCamera"))
        {
            switch (selectedMode) {
                case cursorMode.Default:
                    //  Debug.LogWarning("The selected cursor mode has no code associated to it! Check Cursor.cs/UseTool");
                    CancelDrag();
                    break;

                case cursorMode.Bridge:
                    if (isBridging) {
                        CancelPotentialBridge();
                    }
                    break;
            }
        }
    }

    void StartPlanningBridge(GameObject startingObject)
    {
        selectedBlock = startingObject.GetComponent<Block>();
        GeneratePermanentHighlighter(selectedBlock.gridCoordinates);
        isBridging = true;
    }

    void CancelPotentialBridge()
    {
        ResetSelection();
        ClearFeedback();
        ClearPermanentHighlighter();
        isBridging = false;
    }

    void ResetSelection()
    {
        selectedBlock = null;
    }

    void HighlightBlock(GameObject block = null)
    {
        if (highlighter && block != null) {
            highlighter.SetActive(true);
            Vector3 bounds = block.GetComponent<BoxCollider>().bounds.size;
            highlighter.transform.position = block.transform.position;
            highlighter.transform.SetParent(block.transform);
        }
        else if (highlighter){
            highlighter.SetActive(false);
        }
    }
    void DestroyBridgePreview()
    {
        foreach (GameObject bridgePreviewer in activeBridgePreviews)
        {
            Destroy(bridgePreviewer);
        }
        activeBridgePreviews.Clear();
    }
    void GenerateBridgePreview(Vector3Int firstPosition, Vector3Int secondPosition) { //Genere une preview d'un pont
        GameObject bridgePreviewer = Instantiate(bridgePreview);

        //Crée le pont et le place à equidistance entre les 2 points voulu
        bridgePreviewer.transform.position = GameManager.instance.gridManagement.grid[firstPosition.x,firstPosition.y,firstPosition.z].transform.position + (GameManager.instance.gridManagement.grid[secondPosition.x, secondPosition.y, secondPosition.z].transform.position
                                                                - GameManager.instance.gridManagement.grid[firstPosition.x, firstPosition.y, firstPosition.z].transform.position)/2;

        //Ensuite on cherche à donner au pont la taille qui correspond à la distance entre les deux points du pont
        Vector3 positionDifference = GameManager.instance.gridManagement.grid[secondPosition.x, secondPosition.y, secondPosition.z].transform.position - GameManager.instance.gridManagement.grid[firstPosition.x, firstPosition.y, firstPosition.z].transform.position;

        //La taille sera toujours positive donc on la rend positive
        positionDifference.y = 0.2f; //Hauteur du pont
        positionDifference.x = Mathf.Abs(positionDifference.x);
        positionDifference.z = Mathf.Abs(positionDifference.z);

        //Ensuite on soustrait la taille de la moitié de chaque extrémité du pont (donc 1) à cette valeur
        if (positionDifference.x > 0)
            positionDifference.x -= 1;
        else
            positionDifference.x = 1; //Comme on converti la distance en taille, si la distance est de 0, la taille doit quand même être de 1
        if (positionDifference.z > 0)
            positionDifference.z -= 1;
        else
            positionDifference.z = 1; //Comme on converti la distance en taille, si la distance est de 0, la taille doit quand même être de 1

        //Puis on applique la valeur à la taille du pont
        bridgePreviewer.transform.localScale = positionDifference;

        bridgePreviewer.transform.localPosition -= new Vector3(0, 0.5f, 0); //On abaisse un peu le pont pour qu'il se place en bas des blocs
        activeBridgePreviews.Add(bridgePreviewer);
    }

    Vector3Int[] CheckBridgeableBlocks(Vector3Int coordinate) //Renvoi une liste des coordonnées des blocks reliables au bloc se trouvant aux coordonnées entrées
    {
        List<Vector3Int> coordinatesFound = new List<Vector3Int>();
        //Check les blocs devant la face x
        for (int i = coordinate.x+1; i < GameManager.instance.gridManagement.grid.GetLength(0); i++)
        {
            if (GameManager.instance.gridManagement.GetSlotType(new Vector3Int(i, coordinate.y, coordinate.z),false) != GridManagement.blockType.FREE)
            {
                break;
            }
            if (GameManager.instance.gridManagement.grid[i,coordinate.y,coordinate.z] != null)
            {
                coordinatesFound.Add(new Vector3Int(i, coordinate.y, coordinate.z));
                break;
            }
        }

        //Check les blocs derriere la face x
        for (int i = coordinate.x-1; i >= 0; i--)
        {
            if (GameManager.instance.gridManagement.GetSlotType(new Vector3Int(i, coordinate.y, coordinate.z),false) != GridManagement.blockType.FREE)
            {
                break;
            }
            if (GameManager.instance.gridManagement.grid[i, coordinate.y, coordinate.z] != null)
            {
                coordinatesFound.Add(new Vector3Int(i, coordinate.y, coordinate.z));
                break;
            }
        }

        //Check les blocs devant la face z
        for (int i = coordinate.z+1; i < GameManager.instance.gridManagement.grid.GetLength(2); i++)
        {
            if (GameManager.instance.gridManagement.GetSlotType(new Vector3Int(coordinate.x, coordinate.y, i),false) != GridManagement.blockType.FREE)
            {
                break;
            }
            if (GameManager.instance.gridManagement.grid[coordinate.x, coordinate.y, i] != null)
            {
                coordinatesFound.Add(new Vector3Int(coordinate.x, coordinate.y, i));
                break;
            }
        }

        //Check les blocs derriere la face z
        for (int i = coordinate.z-1; i >= 0; i--)
        {
            if (GameManager.instance.gridManagement.GetSlotType(new Vector3Int(coordinate.x, coordinate.y, i),false) != GridManagement.blockType.FREE)
            {
                break;
            }
            if (GameManager.instance.gridManagement.grid[coordinate.x, coordinate.y, i] != null)
            {
                coordinatesFound.Add(new Vector3Int(coordinate.x, coordinate.y, i));
                break;
            }
        }

        return coordinatesFound.ToArray();
    }

    void GeneratePermanentHighlighter(Vector3Int coordinate)
    {
        GameObject newHighlighter = Instantiate(bridgeHighlighter, highlighter.transform.parent);
        newHighlighter.transform.position = GameManager.instance.gridManagement.grid[coordinate.x, coordinate.y, coordinate.z].transform.position;
        newHighlighter.GetComponent<Highlighter>().SetGreenHighlighter();
        newHighlighter.SetActive(true);
        permanentHighlighter.Add(newHighlighter);
    }

    void ClearPermanentHighlighter()
    {
        foreach (GameObject go in permanentHighlighter)
        {
            Destroy(go);
        }
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
                if (GameManager.instance.gridManagement.grid[coordinate.x, coordinate.y, coordinate.z] != null)
                {
                    GameObject newHighlighter = Instantiate(bridgeHighlighter, highlighter.transform.parent);
                    newHighlighter.transform.parent = GameManager.instance.gridManagement.grid[coordinate.x, coordinate.y, coordinate.z].transform;
                    newHighlighter.transform.localPosition = Vector3.zero;
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
        if (hitGameObject.GetComponent<Block>() != null) 
        {
        //Si le joueur a déjà fait sa premiere selection, on vérifie que le deuxieme bloc selectionné est en face du premier, puis on trace le pont
            
            Block destinationCandidate = hitGameObject.GetComponent<Block>(); //On selectionne temporairement le second block, puis on vérifie s'il remplie les conditions pour tracer un pont
            if (destinationCandidate.gridCoordinates != selectedBlock.gridCoordinates) {
                if (destinationCandidate.gridCoordinates.y == selectedBlock.gridCoordinates.y) {
                    if (destinationCandidate.gridCoordinates.x == selectedBlock.gridCoordinates.x || destinationCandidate.gridCoordinates.z == selectedBlock.gridCoordinates.z) {
                        //Les conditions sont remplies et on peut tracer le pont
                        //Call de la fonction pour tracer un pont
                        GameManager.instance.gridManagement.CreateBridge(selectedBlock, destinationCandidate);
                        ClearPermanentHighlighter();
                    }
                    else {
                        GameManager.instance.errorDisplay.ShowError("You can't link two blocks that aren't aligned");
                        Debug.LogWarning("You can't link two blocks that aren't aligned");
                    }
                }
                else {
                    GameManager.instance.errorDisplay.ShowError("You can't link two blocks of different heights");
                    Debug.LogWarning("You can't link two blocks of different heights");
                }
            }
            else {
                GameManager.instance.errorDisplay.ShowError("You can't select the same point");
                Debug.LogWarning("You can't select the same point");
            }
            ResetSelection();
            ClearPermanentHighlighter();
        }
        
    }


#region DragAndDrop

    public void StartDrag(Block _block)
    {
        if (_block != null && _block.scheme.isMovable == true)
        {
            selectedBlock = _block;
            selectedBlock.StopAllCoroutines();
            sPosition = selectedBlock.transform.position;
            savedPos = selectedBlock.gridCoordinates;
            if (selectedBlock.transform.Find("Bridge") != null) {
                GameManager.instance.gridManagement.DestroyBridge(selectedBlock.transform.Find("Bridge").gameObject);
            }
            GameManager.instance.sfxManager.PlaySound("BlockDrag");
        }
    }

    public void DuringDrag(Vector3Int _pos)
    {
        if(selectedBlock != null)
        {
            if(_pos != savedPos)
            {
                if(!draging) 
                {
                    draging = true;
                    selectedBlock.GetComponent<Collider>().enabled = false;
                }
                else
                {
                    savedPos = _pos;
                    GameManager.instance.sfxManager.PlaySoundWithRandomParameters("Tick", 1, 1, 0.8f, 1.2f);
                    selectedBlock.transform.position = GameManager.instance.gridManagement.IndexToWorldPosition(_pos);
                }
            }
        }
    }

    public void EndDrag(Vector3Int _pos)
    {
        if(selectedBlock != null && draging)
        {
            if (_pos.y <= GameManager.instance.gridManagement.minHeight)
            {
                CancelDrag();
                return;
            }
            if (GameManager.instance.gridManagement.GetSlotType(_pos,false) == GridManagement.blockType.FREE)
            {
                if (selectedBlock.gameObject.layer == LayerMask.NameToLayer("StoredBlock"))
                {
                    FindObjectOfType<StorageBay>().DeStoreBlock(selectedBlock.gameObject);
                }

                //Play SFX
                GameManager.instance.sfxManager.PlaySoundLinked("BlockDrop",selectedBlock.gameObject);

                // Fait tomber les blocs au dessus de la position initiale du bloc qui vient d'être déplacé
                GameManager.instance.gridManagement.UpdateBlocks(selectedBlock.gridCoordinates);

                // Fait tomber le bloc au sol
                GameManager.instance.gridManagement.LayBlock(selectedBlock, new Vector2Int(_pos.x,_pos.z));
            } 
            else
            {
                //If the cube is dragged on the stocking bay
                if(GameManager.instance.gridManagement.GetSlotType(_pos, false) == GridManagement.blockType.STORAGE &&  selectedBlock.gameObject.layer != LayerMask.NameToLayer("StoredBlock") && selectedBlock.scheme.isStorable == true)
                {
                    //Update the grid
                    GameManager.instance.gridManagement.UpdateBlocks(selectedBlock.gridCoordinates);
                    selectedBlock.gridCoordinates = new Vector3Int(0, 0, 0);
                    //Stock the cube in the stocking bay
                    FindObjectOfType<StorageBay>().StoreBlock(selectedBlock.gameObject);
                    selectedBlock.GetComponent<Collider>().enabled = true;
                }
                else
                {
                    CancelDrag();
                    return;
                }
            }
        }
        if(selectedBlock != null)
        {            
            selectedBlock.GetComponent<Collider>().enabled = true;
            selectedBlock = null; 
        }
        draging = false;
    }

    public void CancelDrag()
    {
        if(selectedBlock != null && draging)
        {
            selectedBlock.transform.position = sPosition;
            selectedBlock.GetComponent<Collider>().enabled = true;
            selectedBlock = null;
            draging = false;
        }
    }
    #endregion
}


