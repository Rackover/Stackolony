using UnityEngine;

public class TempBlockSelector : MonoBehaviour
{
	public void ShowBlock(Block _block) 
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
