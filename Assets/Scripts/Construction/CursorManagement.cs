﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CursorManagement : MonoBehaviour
{
    [Header("=== REFERENCIES ===")]
    [Header("Prefabs")]
    public GameObject projectorPrefab;
    public GameObject stackSelectorPrefab;  //Prefab de la petite fléche qui se met au pied de la tour qu'on selectionne
    public GameObject bridgePreview;

    [Space(5)]
    [Header("=== SYSTEM ===")]
    public Color highlightColor;
    public List<GameObject> activeBridgePreviews = new List<GameObject>(); //Liste contenant les preview de pont
    public bool isBridging = false;
    public Block selectedBlock; //Le block selectionné par le joueur
    public int projectorHeight = 10;
    public GameObject myProjector;
    public Projector myProjectorComponent;
    public float dragTreshold = 10; // Amount of pixels the player must move his mouse of to start dragging
    public float blockFallingSpeed = 1;
    public float blockRisingSpeed = 3;
    private Vector2 initialDragPos;
    [HideInInspector] public ScrollRect linkedScrollRect;
    [Space(5)]

    [Header("=== DEBUG ===")]
    public Vector3Int posInGrid; //Position de la souris sur le terrain
    public Vector3 posInWorld;
    public bool isDragging;
    List<Block> highlightedBlocks = new List<Block>();

    // Interface related events & funcs
    public bool couldDrag;
    public Action<string> CursorError;
    public Action<Block, Vector3Int> MovingBlock;
    public bool cursorOnUI = false;
    public bool draggingNewBlock = false;

    float timer;
    Vector3Int savedPos;
    GameObject hoveredBlock;
    GameObject stackSelector; //La petite fléche qui se met au pied de la tour qu'on selectionne

    public void InitializeGameCursor()
    {
        hoveredBlock = null;
        //Instantie un projecteur de selection qui se clamp sur les cellules de la grille
        myProjector = Instantiate(projectorPrefab);
        myProjector.name = "Projector";
        float cellMedianSize = (GameManager.instance.gridManagement.cellSize.x + GameManager.instance.gridManagement.cellSize.z) / 2;
        myProjectorComponent = myProjector.GetComponent<Projector>();
        myProjectorComponent.orthographicSize = 2.5f * cellMedianSize; //On adapte la taille du projecteur (qui projette la grille au sol) à la taille des cellules
        myProjectorComponent.enabled = false;

        //Instantie la fleche qui indique la tour selectionnée par le joueur
        stackSelector = Instantiate(stackSelectorPrefab);
        stackSelector.name = "Stack_Selector";
        stackSelector.SetActive(false);
    }
    public void KillGameCursor()
    {
        Destroy(stackSelector);
        Destroy(myProjector);
    }

    private void Update()
    {
        if (!GameManager.instance.IsInGame()) {
            return;
        }
        
        // Raycast the ground and update the cursor if needed
        UpdateCursor();

        // Detect cursor over UI
        cursorOnUI = false;
        if (EventSystem.current.IsPointerOverGameObject()) { cursorOnUI = true; }
    }

    //Place le curseur correctement sur la grille et sur le terrain
    private void UpdateCursor()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            UpdatePosition(hit);    // Refreshes PosInGrid and PosInTerrain 
            UpdateTool(hit);       // Effects depending on the current tool
            UpdateProjector();      // Visual update

            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Block")) {
                hoveredBlock = hit.collider.gameObject;
                UpdateFeedback(hit);
            }
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain")) {
                UpdateFeedback(hit);
            }
        }
        else // If the mouse is pointing at nothing
        {
            myProjectorComponent.enabled = false;
            stackSelector.SetActive(false);
        }
        transform.position = hit.point;
    }
    
    void UpdatePosition(RaycastHit hit)
    {
        Vector3 tempCoord = hit.point;
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Terrain") || hit.transform.gameObject.layer == LayerMask.NameToLayer("Block")) {
            //On adapte la position de la souris pour qu'elle corresponde à la taille des cellules
            tempCoord += new Vector3(0,
                GameManager.instance.gridManagement.cellSize.y / 2
            ); 
        }

        //Converti la position pour savoir sur quelle case se trouve la souris
        posInGrid = GameManager.instance.gridManagement.WorldPositionToIndex(tempCoord);
        posInWorld = GameManager.instance.gridManagement.IndexToWorldPosition(posInGrid);
    }

    void UpdateProjector()
    {
        if (cursorOnUI)
        {
            myProjectorComponent.enabled = false;
        } else
        {
            myProjectorComponent.enabled = true;
        }
        //Met à jour la position du projecteur
        myProjector.transform.position = GameManager.instance.gridManagement.IndexToWorldPosition(posInGrid) + new Vector3(0, projectorHeight, 0);
    }

    void ClearFeedback()
    {
        if (activeBridgePreviews.Count > 0)
            DestroyBridgePreview();
        ResetHighlightedBlocks();
    }

    void UpdateFeedback(RaycastHit hit)
    {
        if (isDragging)
        {
            if (selectedBlock != null)
            {
                HighlightBlock(selectedBlock.gameObject);
            } else
            {
                ClearFeedback();
            }
        }
        // Highlighting block the cursor currently is on if we're in Bridge mode
        else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block")) 
        {
            GameObject objective = hit.transform.gameObject;
            Vector3Int position = posInGrid;
            // If the player is bridging, we stuck the preview to the block he's dragging the bridge from 
            if (isBridging) {
                objective = selectedBlock.gameObject;
                position = selectedBlock.gridCoordinates;
            }
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

    void UpdateTool(RaycastHit hit)
    {
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block") && hit.transform.gameObject.GetComponent<Block>().scheme.isMovable && !cursorOnUI)
        {
            if (selectedBlock == null)
            {
                couldDrag = true;
            }
        } else
        {
            couldDrag = false;
        }
       
        
        /////////////////////////////
        ///
        ///     DRAG AND DROP
        ///
        if(Input.GetButtonDown("Select"))
        {
            initialDragPos = Input.mousePosition;
        }

        if (Input.GetButton("Select"))
        {
            if (selectedBlock == null && !cursorOnUI)
            {
                Block selectedBlock = hit.transform.gameObject.GetComponent<Block>();
                StartDrag(selectedBlock);
            }
            DuringDrag(posInGrid);
        }

        if (Input.GetButtonUp("Select"))
        {
            EndDrag(posInGrid);
        }
        ///
        /////////////////////////////



        /////////////////////////////
        ///
        ///     BRIDGING
        ///

        // Mouse click hold
        if (Input.GetButton("Bridge")) {
            if (!isBridging && !isDragging) {
                GameObject selectedObj = hit.transform.gameObject;
                if (selectedObj != null) {
                    if (selectedObj.transform.parent != null) {
                        if (selectedObj.transform.parent.GetComponent<BridgeInfo>() != null) {
                            GameManager.instance.gridManagement.DestroyBridge(selectedObj.transform.parent.gameObject);
                        }
                    }
                }
                if (selectedObj.layer == LayerMask.NameToLayer("Block")) {
                    StartPlanningBridge(hit.transform.gameObject);
                }
            }
        }

        // Left Mouse up 
        if (Input.GetButtonUp("Bridge")) {
            if (isBridging) {
                isBridging = false;
                TryToMakeBridge(hit.transform.gameObject);
                CancelPotentialBridge();
            }
        }
        ///
        /////////////////////////////



        /// Cancel actions
        if (Input.GetButton("Trash")) {
            if (isDragging) {
                CancelDrag();
            }
            if (isBridging) {
                CancelPotentialBridge();
            }
        }
    }

    void StartPlanningBridge(GameObject startingObject)
    {
        selectedBlock = startingObject.GetComponent<Block>();
        isBridging = true;
    }

    void CancelPotentialBridge()
    {
        ResetSelection();
        ClearFeedback();
        isBridging = false;
    }

    void ResetSelection()
    {
        selectedBlock = null;
    }

    void HighlightBlock(GameObject block = null)
    {
        Block foundBlock = block.GetComponent<Block>();
        if (foundBlock != null)
        {
            highlightedBlocks.Add(foundBlock);
        }
    }

    void ResetHighlightedBlocks()
    {
        foreach (Block block in highlightedBlocks)
        {
            block.overlayVisuals.Deactivate();
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
            if (GameManager.instance.gridManagement.GetSlotType(new Vector3Int(i, coordinate.y, coordinate.z)) != GridManagement.blockType.FREE)
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
            if (GameManager.instance.gridManagement.GetSlotType(new Vector3Int(i, coordinate.y, coordinate.z)) != GridManagement.blockType.FREE)
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
            if (GameManager.instance.gridManagement.GetSlotType(new Vector3Int(coordinate.x, coordinate.y, i)) != GridManagement.blockType.FREE)
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
            if (GameManager.instance.gridManagement.GetSlotType(new Vector3Int(coordinate.x, coordinate.y, i)) != GridManagement.blockType.FREE)
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
                        bool bridgeAlreadyFound = false;
                        foreach (GameObject bridge in destinationCandidate.bridges)
                        {
                            if (bridge.GetComponent<BridgeInfo>().destination == selectedBlock.gridCoordinates)
                            {
                                bridgeAlreadyFound = true;
                            }
                        }
                        foreach (GameObject bridge in selectedBlock.bridges)
                        {
                            if (bridge.GetComponent<BridgeInfo>().destination == destinationCandidate.gridCoordinates)
                            {
                                bridgeAlreadyFound = true;
                            }
                        }
                        if (!bridgeAlreadyFound)
                        {
                            //Les conditions sont remplies et on peut tracer le pont
                            //Call de la fonction pour tracer un pont
                            GameManager.instance.gridManagement.CreateBridge(selectedBlock, destinationCandidate);
                        } else {
                            CursorError.Invoke("bridgeAlreadyFound");
                        }
                    }
                    else {
                        CursorError.Invoke("misalignedBlocks");
                    }
                }
                else {
                    CursorError.Invoke("differentHeightBlocks");
                }
            }
            else {
                CursorError.Invoke("selfBridging");
            }
            ResetSelection();
        }
        
    }

    public void ClearListeners()
    {
        try {
            foreach (Delegate d in CursorError.GetInvocationList()) {
                CursorError -= (Action<string>)d;
            }
        }
        catch {
            // Nothing to do
        }
    }

#region DragAndDrop

    public void StartDrag(Block _block)
    {
        if (linkedScrollRect != null)
        {
            linkedScrollRect.enabled = false;
        }

        if (_block != null && _block.scheme.isMovable == true && !_block.states.ContainsKey(State.OnFire))
        {
            selectedBlock = _block;
            selectedBlock.StopAllCoroutines();
            savedPos = selectedBlock.gridCoordinates;
            if (selectedBlock.transform.Find("Bridge") != null)
            {
                GameManager.instance.gridManagement.DestroyBridge(selectedBlock.transform.Find("Bridge").gameObject);
            }
            
            if(_block.scheme.sound != null)
            {
                GameManager.instance.soundManager.PlayClip(_block.scheme.sound, 0.4f);
            }
            else
            {
                GameManager.instance.soundManager.Play("BlockDrag");
            }
        }
    }

    public void DuringDrag(Vector3Int _pos)
    {
        if (selectedBlock != null)
        {
            if (_pos != savedPos)
            {
                if (!isDragging)
                {
                    isDragging = true;
                    selectedBlock.GetComponent<Collider>().enabled = false;
                }
                savedPos = _pos;
                GameManager.instance.soundManager.Play("Tick");
            }
            // The player is dragging
            MovingBlock.Invoke(selectedBlock, savedPos);
        }
    }

    public void EndDrag(Vector3Int _pos)
    {
        if (linkedScrollRect != null)
        {
            linkedScrollRect.enabled = true;
        }

        if (selectedBlock != null && isDragging)
        {
            if (cursorOnUI)
            {
                CancelDrag();
                return;
            }
            if (GameManager.instance.gridManagement.IsPlaceable(_pos, true, selectedBlock.scheme))
            {
                //Play SFX
                GameManager.instance.soundManager.Play("BlockDrop");

                // Fait tomber les blocs au dessus de la position initiale du bloc qui vient d'être déplacé
                GameManager.instance.gridManagement.UpdateBlocks(selectedBlock.gridCoordinates);

                // Fait tomber le bloc au sol
                if (GameManager.instance.gridManagement.grid[_pos.x, _pos.y, _pos.z] != null)
                {
                    GameManager.instance.gridManagement.InsertBlockAtPosition(selectedBlock, _pos);
                    GameManager.instance.animationManager.EndElevateTower(new Vector2Int(_pos.x, _pos.z),0);
                }
                else
                {
                    GameManager.instance.gridManagement.LayBlock(selectedBlock, new Vector2Int(_pos.x, _pos.z));
                }
                selectedBlock.LoadFlags();
            }
            else
            {
                CancelDrag();
                return;
            }
        }
        if (selectedBlock != null)
        {        
            selectedBlock.CallFlags("OnBlockUpdate");
            selectedBlock.boxCollider.enabled = true;
            selectedBlock = null; 
        }
        isDragging = false;
        draggingNewBlock = false;
    }

    public void CancelDrag()
    {
        if (selectedBlock != null && isDragging)
        {
            if (draggingNewBlock == true)
            {
                GameManager.instance.gridManagement.DestroyBlock(selectedBlock);
                draggingNewBlock = false;
            }
            else if (selectedBlock != null)
            {
                selectedBlock.transform.position = GameManager.instance.gridManagement.IndexToWorldPosition(selectedBlock.gridCoordinates);
            }
            selectedBlock.boxCollider.enabled = true;
            selectedBlock = null;
            isDragging = false;
        }
    }
    #endregion
}


