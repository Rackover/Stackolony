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

    public SFXManager sfxManager;

    public void StartDrag(BlockLink _block)
    {
        if(_block != null)
        {
            Debug.Log("Start dragging cube");    
            timer = Time.time + dragDelay;
            sBlock = _block;
            sBlock.collider.enabled = false;
            sPosition = sBlock.transform.position;
            if (sBlock.transform.Find("Bridge") != null) {
                gridManagement.DestroyBridge(sBlock.transform.Find("Bridge").gameObject);
            }
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
            }  
        }
    }

    public void EndDrag(Vector3Int _pos)
    {
        if(sBlock != null && draging)
        {
            if (gridManagement.checkIfSlotIsBlocked(_pos,false) == GridManagement.blockType.FREE)
            {
                if (sBlock.gameObject.layer == LayerMask.NameToLayer("StoredBlock"))
                {
                    FindObjectOfType<StorageBay>().DeStoreBlock(sBlock.gameObject);
                }
                Debug.Log("End dragging");

                //Play SFX
                sfxManager.PlaySoundLinked("BlockDrop",sBlock.gameObject);

                gridManagement.MoveBlock(sBlock.gameObject, _pos);
                sBlock.collider.enabled = true;

                sBlock = null;
                draging = false;
            } else
            {
                //If the cube is dragged on the stocking bay
                if (gridManagement.checkIfSlotIsBlocked(_pos, false) == GridManagement.blockType.STORAGE)
                {
                    //Update the grid
                    gridManagement.UpdateBlocks(sBlock.gridCoordinates);
                    sBlock.gridCoordinates = new Vector3Int(0, 0, 0);
                    //Stock the cube in the stocking bay
                    FindObjectOfType<StorageBay>().StoreBlock(sBlock.gameObject);
                    Debug.Log("End dragging");
                    sBlock.collider.enabled = true;

                    sBlock = null;
                    draging = false;
                    return;
                }
                else
                {
                    CancelDrag();
                }
            }
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