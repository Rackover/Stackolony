using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashButton : MonoBehaviour {

    bool hovered = false;

    public void Hover()
    {
        hovered = true;
    }

    public void UnHover()
    {
        hovered = false;
    }

    public void TrashCurrentBuilding()
    {
        CursorManagement cursorMan = GameManager.instance.cursorManagement;

        if (cursorMan.isDragging && cursorMan.selectedBlock != null) {
            GameManager.instance.gridManagement.DestroyBlock(cursorMan.selectedBlock.gridCoordinates);
        }
    }

    private void Update()
    {
        if (hovered && Input.GetButtonUp("Select")) {
            TrashCurrentBuilding();
        }
    }
}
