using UnityEngine;
using UnityEngine.UI;


public class StateTag : MonoBehaviour 
{
	public RectTransform self;
	public Text stateTxt;
	public Image stateBackground;

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
				stateBackground.color = Color.green;
				break;
			case BlockState.OnFire:
				stateBackground.color = Color.red;
				break;
			case BlockState.OnRiot:
				stateBackground.color = Color.blue;
				break;

			default:
				Debug.Log("Unclear state");
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
