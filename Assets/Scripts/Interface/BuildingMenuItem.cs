using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingMenuItem : MonoBehaviour {

    public int blockId;
    bool isBeingDragged = false;
    bool concerned = false;
    GameObject draggingBuilding;

    public void SetConcerned()
    {
        concerned = true;
    }
    public void UnsetConcerned()
    {
        concerned = false;
    }

    void Update () {
		if (isBeingDragged) {
            GameManager.instance.cursorManagement.isDragging = true;
            GetComponent<Image>().enabled = false;
            draggingBuilding.transform.position = GameManager.instance.cursorManagement.posInWorld;

            if (Input.GetButtonUp("Select")) {
                isBeingDragged = false;
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
        else {
            GetComponent<Image>().enabled = true;

            if (Input.GetButton("Select") && concerned && !GameManager.instance.cursorManagement.isDragging) {
                GameManager.instance.cursorManagement.isDragging = true;
                isBeingDragged = true;
                if (draggingBuilding == null) {
                    draggingBuilding = new GameObject("fakeBuildingPreview");
                    Instantiate(GameManager.instance.library.GetBlockByID(blockId).model, draggingBuilding.transform).transform.position = new Vector3();
                }
            }
        }
	}
}
