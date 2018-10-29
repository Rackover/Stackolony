using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempDrag : MonoBehaviour  {
	
    public GridManagement gridManagement;
    [HideInInspector] public BlockLink sBlock;
    [HideInInspector] public Vector3 sPosition;

    public void StartDrag(BlockLink _block)
    {
        Debug.Log("Start dragging cube");
        sBlock = _block;

        sBlock.collider.enabled = false;
        sBlock.gameObject.layer = 0;
        sPosition = sBlock.transform.position;
    }

    public void DuringDrag(Vector3Int _pos)
    {
        if(sBlock != null)
        {
            //Vector3 newPos = new Vector3(_pos.x*cellsize + cellsize/2, _pos.y + 0.5f, _pos.z*cellsize + cellsize/2);
            Vector3 newPos = new Vector3(_pos.x * gridManagement.cellSize + gridManagement.cellSize * 0.5f, _pos.y + 0.5f, _pos.z * gridManagement.cellSize + gridManagement.cellSize * 0.5f);
            sBlock.transform.position = newPos;
        }  
        //Debug.Log(gridManagement.grid[_pos.x, _pos.y, _pos.z]);
        //gridManagement.grid
    }

    public void EndDrag(Vector3Int _pos)
    {
        if(sBlock != null)
        {
            Debug.Log("End dragging");

            gridManagement.MoveBlock(sBlock.gameObject, _pos);
            sBlock.collider.enabled = true;
            sBlock.gameObject.layer = 10;
            sBlock = null;
        }
    }

    public void CancelDrag()
    {
        if(sBlock != null)
        {
            Debug.Log("Cancel cube dragging");

            sBlock.transform.position = sPosition;
            sBlock.gameObject.layer = 10;
            sBlock = null;
        }
    }
}