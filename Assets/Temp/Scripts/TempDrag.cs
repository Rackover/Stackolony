using UnityEngine;

public class TempDrag : MonoBehaviour  {
	
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

    public void DuringDrag(Vector3 _pos)
    {
        if(sBlock != null)
        {
            //Vector3 newPos = new Vector3(_pos.x*cellsize + cellsize/2, _pos.y + 0.5f, _pos.z*cellsize + cellsize/2);
            Vector3 newPos = new Vector3(_pos.x*2.5f + 1.25f, _pos.y + 0.5f, _pos.z*2.5f + 1.25f);
            sBlock.transform.position = newPos;
        }
    }

    public void EndDrag(Vector3 _pos)
    {
        if(sBlock != null)
        {
            Debug.Log("End dragging");

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