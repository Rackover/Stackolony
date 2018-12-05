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
    private Vector3Int _savedPos;

    public SFXManager sfxManager;
    public SystemReferences systemRef;

    public void StartDrag(BlockLink _block)
    {
        if (_block != null)
        {
            timer = Time.time + dragDelay;
            sBlock = _block;
            sBlock.collider.enabled = false;
            sPosition = sBlock.transform.position;
            _savedPos = sBlock.gridCoordinates;
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
            if(!draging)
            {
                if(Time.time > timer)
                {
                    sBlock.collider.enabled = false;
                    sPosition = sBlock.transform.position;
                    draging = true;
                }
            }
            else
            {
                if (_pos != _savedPos)
                {
                    _savedPos = _pos;
                    sfxManager.PlaySoundWithRandomParameters("Tick", 1, 1, 0.8f, 1.2f);
                }
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

                //Play SFX
                sfxManager.PlaySoundLinked("BlockDrop",sBlock.gameObject);
                
                if (systemRef == null)
                {
                    systemRef = FindObjectOfType<SystemReferences>();
                }
                if (systemRef != null)
                {
                    if (sBlock.GetComponent<Generator>() == null)
                    {
                        systemRef.UpdateSystem();
                    }
                }
                //RESET SOME VALUES OF THE BLOCK THAT ARE RECALCULATED BY THE SYSTEM
                sBlock.currentPower = 0;

                sBlock.CallFlags("BeforeMovingBlock");
                gridManagement.MoveBlock(sBlock.gameObject, _pos);
                sBlock.collider.enabled = true;

                sBlock = null;
                draging = false;
            } else
            {
                //If the cube is dragged on the stocking bay
                if (gridManagement.checkIfSlotIsBlocked(_pos, false) == GridManagement.blockType.STORAGE &&  sBlock.gameObject.layer != LayerMask.NameToLayer("StoredBlock"))
                {
                    //Update the grid
                    gridManagement.UpdateBlocks(sBlock.gridCoordinates);
                    sBlock.gridCoordinates = new Vector3Int(0, 0, 0);
                    //Stock the cube in the stocking bay
                    FindObjectOfType<StorageBay>().StoreBlock(sBlock.gameObject);
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
            sBlock.transform.position = sPosition;
            sBlock.collider.enabled = true;

            sBlock = null;
            draging = false;
        }
    }
}