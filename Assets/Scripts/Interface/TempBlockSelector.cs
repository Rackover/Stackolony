using UnityEngine;

public class TempBlockSelector : MonoBehaviour
{
	public BlockInfobox blockInfoBox;

	void Update () 
	{
		if(Input.GetButtonDown("MouseLeft"))
		{
			RaycastHit hit;
        	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			BlockLink block;

			if (Physics.Raycast(ray, out hit))
			{
				block = hit.transform.gameObject.GetComponent<BlockLink>();
				if(block != null){blockInfoBox.LoadBlockValues(block);}
				else{blockInfoBox.Hide();}			
			}
		}
	}
}
