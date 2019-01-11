using UnityEngine;

public class TempBlockSelector : MonoBehaviour
{
	public void ShowBlock(BlockLink _block) 
	{
        if (_block != null)
        {
			GameManager.instance.blockInfobox.LoadBlockValues(_block);
        }
		else 
		{
			GameManager.instance.blockInfobox.Hide();
		}
	}
}
