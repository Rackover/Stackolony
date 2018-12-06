using UnityEngine;

public class TempBlockSelector : MonoBehaviour
{
	public BlockInfobox blockInfoBox;

	public void ShowBlock(BlockLink _block) 
	{
        if (_block != null)
        {
			blockInfoBox.LoadBlockValues(_block);
        }
		else 
		{
			blockInfoBox.Hide();
		}
	}
}
