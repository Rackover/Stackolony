using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CitizenIcon 
{
    public GameObject self;
    public Image image;
}

public class HousePanel : FlagPanel 
{
	public Text slotText;
	public CitizenIcon[] citizenIcons;

	public void ShowFlag(House h)
	{
		slotText.text = h.affectedCitizen.Count.ToString() + " / " + h.slotAmount.ToString();
		
		for(int i = 0; i < citizenIcons.Length; i++)
		{
			citizenIcons[i].self.SetActive(false);
		}

		for(int i = 0; i < h.acceptedPop.Length; i++)
		{
			citizenIcons[i].self.SetActive(true);
			citizenIcons[i].image.sprite = h.acceptedPop[i].humorSprite;
		}
	}
}