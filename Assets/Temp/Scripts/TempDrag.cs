using UnityEngine;

public class TempDrag : MonoBehaviour  {
	
    [HideInInspector] public BlockLink sBlock;
    [HideInInspector] public Vector3 sPosition;

    public void StartDrag(BlockLink _block)
    {
        Debug.Log("Start dragging cube");
        sBlock = _block;
        sBlock.gameObject.layer = 0;
        sPosition = sBlock.transform.position;
    }

    public void DuringDrag(Vector3 _pos)
    {
        if(sBlock != null)
        {
            sBlock.transform.position = _pos;
        }
    }

    public void EndDrag(Vector3 _pos)
    {
        if(sBlock != null)
        {
            Debug.Log("End dragging");

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