using UnityEngine;
using UnityEngine.UI;

public class BlockInfobox : MonoBehaviour 
{
	public GameObject box;
	public Text nameText;
	public Text descriptionText;
	public RectTransform sliderLife;


	public void LoadBlockValues(BlockLink block)
	{
		box.SetActive(true);
		nameText.text = block.myBlock.title;
		descriptionText.text = block.myBlock.description;
	}

	public void Hide()
	{
		box.SetActive(false);
	}
}
