using UnityEngine;
using UnityEngine.UI;

public class FiremanStationPanel : FlagPanel 
{
	[Header("Referenciens")]
	public Button button;

	public override void ShowFlag(Flag flag)
	{
		button.onClick.AddListener(flag.Use);
	}
}
