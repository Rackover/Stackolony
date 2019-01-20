using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashButton : MonoBehaviour {


    public void TrashCurrentBuilding()
    {
        CursorManagement cursorMan = GameManager.instance.cursorManagement;
        print(cursorMan.selectedBlock);

        if (cursorMan.isDragging && cursorMan.selectedBlock != null) {
            GameManager.instance.gridManagement.DestroyBlock(cursorMan.selectedBlock.gridCoordinates);
        }
    }
	
}
