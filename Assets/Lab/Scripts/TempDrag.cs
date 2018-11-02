using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempDrag : MonoBehaviour  {
	
    public float dragDelay = 1f;
    public GridManagement gridManagement;
    [HideInInspector] public BlockLink sBlock;
    [HideInInspector] public Vector3 sPosition;
    float timer;
    bool draging;

    public void StartDrag(BlockLink _block)
    {
        if(_block != null)
        {
            Debug.Log("Start dragging cube");    
            timer = Time.time + dragDelay;
            sBlock = _block;
            sBlock.collider.enabled = false;
            sPosition = sBlock.transform.position;
        }
    }

    public void DuringDrag(Vector3Int _pos)
    {
        if(sBlock != null)
        {
            if(!draging)
            {
                if(Time.time > timer)
                {
                    Debug.Log("Start dragging cube");
                    sBlock.collider.enabled = false;
                    sPosition = sBlock.transform.position;
                    draging = true;
                }
            }
            else
            {
                
                sBlock.transform.position = new Vector3
                (
                    _pos.x * gridManagement.cellSize + gridManagement.cellSize * 0.5f,
                    _pos.y + 0.5f,
                    _pos.z * gridManagement.cellSize + gridManagement.cellSize * 0.5f
                );
                /*
                sBlock.gridCoordinates = new Vector3Int
                (
                    (int)(_pos.x * gridManagement.cellSize + gridManagement.cellSize * 0.5f),
                    (int)(_pos.y + 0.5f),
                    (int)(_pos.z * gridManagement.cellSize + gridManagement.cellSize * 0.5f)
                );
                //Déplace le block vers ses nouvelles coordonnées
                sBlock.MoveToMyPosition();
                */
            }  
        }
    }

    public void EndDrag(Vector3Int _pos)
    {
        if(sBlock != null && draging)
        {
            Debug.Log("End dragging");
            gridManagement.MoveBlock(sBlock.gameObject, _pos);
            sBlock.collider.enabled = true;

            sBlock = null;
            draging = false;
        }
    }

    public void CancelDrag()
    {
        if(sBlock != null && draging)
        {
            Debug.Log("Cancel cube dragging");
            sBlock.transform.position = sPosition;

            sBlock = null;
            draging = false;
        }
    }
}