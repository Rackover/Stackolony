using UnityEngine;
using UnityEngine.UI;


public class StateTag : MonoBehaviour 
{
	[Header("Referencies")]
	public RectTransform self;
	public Text stateTxt;
	public Image stateBackground;

	[Header("Settings")]
	public Color onFireColor;
	public Color poweredColor;
	public Color onRiotColor;
	public Color damagedColor;

	public bool available;

	public void PrintTag(BlockState state)
	{
		stateTxt.enabled = true;
		stateBackground.enabled = true;

		available = false;
		stateTxt.text = state.ToString();

		switch(state)
		{
			case BlockState.Powered:
				stateBackground.color = poweredColor;
				break;

			case BlockState.OnFire:
				stateBackground.color = onFireColor;
				break;

			case BlockState.OnRiot:
				stateBackground.color = onRiotColor;
				break;

			case BlockState.Damaged:
				stateBackground.color = damagedColor;
				break;

			default:
				Logger.Debug(state.ToString() + " dosn't have any color assigned to it.");
				break;
		}

	}

	public void Hide()
	{
		available = true;
		stateTxt.enabled = false;
		stateBackground.enabled = false;
	}
}
