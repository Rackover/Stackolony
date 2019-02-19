using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public int blockId;
    bool isBeingDragged = false;
    bool concerned = false;
    bool isLocked = false;
    Block draggingBuilding;
    GameObject blockPrefab;
    GameObject padlock;
    public ScrollRect parentScrollRect;

    RawImage ri;
    Displayer display;
    Texture preview;
    
    private void Start()
    {
        blockPrefab = GameManager.instance.library.GetBlockByID(blockId).model;
        ri = GetComponent<RawImage>();
        display = CreateDisplay();
        padlock = transform.GetChild(0).gameObject;

        Invoke("Freeze", 0.1f);
        Invoke("SavePreview", 0.1f);
    }

    public void SetConcerned()
    {
        if (isLocked) {
            return;
        }
        concerned = true;
        display = CreateDisplay();
    }
    
    public void Lock()
    {
        padlock.SetActive(true);
        isLocked = true;
    }

    public void Unlock()
    {
        padlock.SetActive(false);
        isLocked = false;
    }

    Displayer CreateDisplay()
    {
        return GameManager.instance.displayerManager.SetRotationFeed(blockPrefab, ri, 45f, 2, 5, 20, 128);
    }

    public void UnsetConcerned()
    {
        concerned = false;
        ri.texture = preview;
        if(display != null) display.Unstage();
    }

    void SavePreview()
    {
        Texture saved = new RenderTexture(ri.texture.height, ri.texture.width, 1);
        Graphics.CopyTexture(ri.texture, saved);
        preview = saved;
    }

    void Freeze()
    {
        Texture saved = new RenderTexture(ri.texture.height, ri.texture.width, 1);
        Graphics.CopyTexture(ri.texture, saved);
        display.Unstage();
        ri.texture = saved;
    }

    void Update () {
        // Lock check
        Unlock();
        if (GameManager.instance.cityManager.IsLocked(blockId)) {
            Lock();
        }

        // Drag
        if (isBeingDragged && !isLocked && !GameManager.instance.cursorManagement.cursorOnUI) {
            if (draggingBuilding == null)
            {
                draggingBuilding = GameManager.instance.gridManagement.CreateBlockFromId(blockId).GetComponent<Block>();
                draggingBuilding.Pack();
                draggingBuilding.transform.position = GameManager.instance.cursorManagement.posInWorld;
            }
            GameManager.instance.cursorManagement.selectedBlock = draggingBuilding;
            GameManager.instance.cursorManagement.draggingNewBlock = true;
            GameManager.instance.cursorManagement.linkedScrollRect = parentScrollRect;
            GameManager.instance.cursorManagement.StartDrag(draggingBuilding);

            isBeingDragged = false;
            draggingBuilding = null;
        }
        else if (!isLocked){
            if (Input.GetButton("Select") && concerned && !GameManager.instance.cursorManagement.isDragging) {
                isBeingDragged = true;
                ri.color = new Color(ri.color.r, ri.color.g, ri.color.b, 0f);
            }
            if (Input.GetButtonUp("Select")) {
                ri.color = new Color(ri.color.r, ri.color.g, ri.color.b, 1f);
                isBeingDragged = false;
            }
        }
	}

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetConcerned();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UnsetConcerned();
    }
}
