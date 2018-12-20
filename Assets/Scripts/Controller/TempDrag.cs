﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempDrag : MonoBehaviour  {
	
    public float dragDelay = 1f;
    public GridManagement gridManagement;
    [HideInInspector] public BlockLink sBlock;
    [HideInInspector] public Vector3 sPosition;
    float timer;
    bool draging;
    private Vector3Int savedPos;

    public SFXManager sfxManager;
    public SystemReferences systemRef;

    public void StartDrag(BlockLink _block)
    {
        if (_block != null)
        {
            sBlock = _block;
            sPosition = sBlock.transform.position;
            savedPos = sBlock.gridCoordinates;
            if (sBlock.transform.Find("Bridge") != null) {
                gridManagement.DestroyBridge(sBlock.transform.Find("Bridge").gameObject);
            }
            sfxManager.PlaySound("BlockDrag");
        }
    }

    public void DuringDrag(Vector3Int _pos)
    {
        if(sBlock != null)
        {
            if(_pos != savedPos)
            {
                if(!draging) 
                {
                    draging = true;
                    sBlock.collider.enabled = false;
                }
                else
                {
                    savedPos = _pos;
                    sfxManager.PlaySoundWithRandomParameters("Tick", 1, 1, 0.8f, 1.2f);
                    sBlock.transform.position = new Vector3
                    (
                        _pos.x * gridManagement.cellSize + gridManagement.cellSize * 0.5f,
                        _pos.y + 0.5f,
                        _pos.z * gridManagement.cellSize + gridManagement.cellSize * 0.5f
                    );
                }
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
                //Play SFX
                sfxManager.PlaySoundLinked("BlockDrop",sBlock.gameObject);
                
                if (systemRef == null)
                {
                    systemRef = FindObjectOfType<SystemReferences>();
                }
                if (systemRef != null)
                {
                    systemRef.UpdateSystem();
                }
                //RESET SOME VALUES OF THE BLOCK THAT ARE RECALCULATED BY THE SYSTEM
                sBlock.currentPower = 0;

                sBlock.CallFlags("BeforeMovingBlock");
                gridManagement.MoveBlock(sBlock.gameObject, _pos);
            } 
            else
            {
                //If the cube is dragged on the stocking bay
                if (gridManagement.checkIfSlotIsBlocked(_pos, false) == GridManagement.blockType.STORAGE &&  sBlock.gameObject.layer != LayerMask.NameToLayer("StoredBlock"))
                {
                    //Update the grid
                    gridManagement.UpdateBlocks(sBlock.gridCoordinates);
                    sBlock.gridCoordinates = new Vector3Int(0, 0, 0);
                    //Stock the cube in the stocking bay
                    FindObjectOfType<StorageBay>().StoreBlock(sBlock.gameObject);
                    GameManager.instance.systemReferences.UpdateSystem();
                    sBlock.collider.enabled = true;
                }
                else
                {
                    CancelDrag();
                }
            }
        }
        if(sBlock != null)
        {            
            sBlock.collider.enabled = true;
            sBlock = null; 
        }
        draging = false;
    }

    public void CancelDrag()
    {
        if(sBlock != null && draging)
        {
            sBlock.transform.position = sPosition;
            sBlock.collider.enabled = true;
            sBlock = null;
            draging = false;
        }
    }
}