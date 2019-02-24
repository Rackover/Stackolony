using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public int blockId;
    public ScrollRect parentScrollRect;


    // STATES
    bool isBeingDragged = false;
    bool concerned = false;
    bool isLocked = false;
    bool clicked = false;
    bool lastState;

    // REFERENCES
    Block draggingBuilding;
    GameObject blockPrefab;
    GameObject padlock;
    RawImage ri;
    Image background;
    Displayer display;
    Texture preview;
    
    private void Start()
    {
        blockPrefab = GameManager.instance.library.GetBlockByID(blockId).model;
        background = transform.parent.GetComponent<Image>();
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
        background.color = Color.black;
        isLocked = true;
    }

    public void Unlock()
    {
        background.color = Color.white;
        transform.parent.SetSiblingIndex(0);
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

    void Update () 
    {
        // LOCK CHECK
        bool currentState = GameManager.instance.cityManager.IsLocked(blockId); 
        if (lastState != currentState)
        {
            lastState = currentState;
            if(lastState) Lock();
            else Unlock();
        }
        else
        {
            if(lastState) Lock();
            else Unlock();  
        }

        // DRAG
        if (clicked || isBeingDragged && !isLocked && !GameManager.instance.cursorManagement.cursorOnUI)
        {
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
            clicked = false;
            draggingBuilding = null;
        }
        else if (!isLocked)
        {
            if (Input.GetButtonDown("Select") && concerned && !GameManager.instance.cursorManagement.isDragging && !isBeingDragged)
            {
                isBeingDragged = true;
                ri.color = new Color(ri.color.r, ri.color.g, ri.color.b, 0f);
            }
            else if (Input.GetButtonUp("Select"))
            {
                if(concerned)
                {
                    isBeingDragged = false;
                    clicked = true;
                }
                else
                {
                    clicked = false;
                    ri.color = new Color(ri.color.r, ri.color.g, ri.color.b, 1f);
                }
                
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
