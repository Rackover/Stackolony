﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingMenuItem : MonoBehaviour {

    public int blockId;
    bool isBeingDragged = false;
    bool concerned = false;
    bool isLocked = false;
    GameObject draggingBuilding;
    GameObject blockPrefab;
    GameObject padlock;

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
		if (isBeingDragged && !isLocked) {
            GameManager.instance.cursorManagement.isDragging = true;
            ri.enabled = false;
            draggingBuilding.transform.position = GameManager.instance.cursorManagement.posInWorld;

            if (Input.GetButtonUp("Select")) {
                isBeingDragged = false;
                GameManager.instance.cursorManagement.isDragging = false;
                Destroy(draggingBuilding);

                if (GameManager.instance.cursorManagement.cursorOnUI) {
                    return;
                }

                GameManager.instance.gridManagement.LayBlock(
                    blockId, 
                    new Vector2Int(
                        GameManager.instance.cursorManagement.posInGrid.x,
                        GameManager.instance.cursorManagement.posInGrid.z
                    )
                );
            }

        }
        else if (!isLocked){
            ri.enabled = true;

            if (Input.GetButton("Select") && concerned && !GameManager.instance.cursorManagement.isDragging) {
                GameManager.instance.cursorManagement.isDragging = true;
                isBeingDragged = true;
                if (draggingBuilding == null) {
                    draggingBuilding = new GameObject("fakeBuildingPreview");
                    Instantiate(blockPrefab, draggingBuilding.transform).transform.position = new Vector3();
                }
            }
        }
	}
}
