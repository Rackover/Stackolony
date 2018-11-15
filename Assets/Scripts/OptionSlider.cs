using UnityEngine;
using UnityEngine.UI;

public class OptionSlider : MonoBehaviour {

	public Slider slider;
	public Text valueText;

	public void Awake()
	{
		if(slider == null){slider = GetComponent<Slider>();}
		if(slider == null){Debug.Log("Slider is not set properly, contact devs.");}

		valueText.text = System.Math.Round(slider.value,2).ToString();
	}

	public void ChangeValueText()
	{
		valueText.text = System.Math.Round(slider.value,2).ToString();
	}
}
