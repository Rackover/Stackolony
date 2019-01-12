using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OptionsWindow : MonoBehaviour {

    public GameObject sliderExample;
    public GameObject checkboxExample;
    Options options;

    public float optionListStart = -200f;
    public float optionsSpacing = 70f;

	// Use this for initialization
	void Start () {
        options = GameManager.instance.player.options;
        Localization loc = GameManager.instance.localization;

        loc.SetCategory("options");
        int count = 0;
        foreach(KeyValuePair<string, Options.IOption> option in options.Get()) {

            GameObject inst = new GameObject(); // Dummy game object - can't avoid creating it

            //////////////
            ///
            /// SLIDER option
            /// 
            if (option.Value is Options.SliderOption) {
                Options.SliderOption opt = (Options.SliderOption)option.Value;

                Destroy(inst);
                inst = Instantiate(sliderExample, transform);
                Text[] tagAndVal = inst.GetComponentsInChildren<Text>();
                tagAndVal[0].text = loc.GetLine(option.Key);
                tagAndVal[1].text = opt.value.ToString("n2");

                Slider slider = inst.GetComponentInChildren<Slider>();
                slider.minValue = opt.minValue;
                slider.maxValue = opt.maxValue;
                slider.value = opt.value;
                slider.onValueChanged.AddListener(delegate {
                    tagAndVal[1].text = slider.value.ToString("n2");
                    opt.Set(slider.value);
                });
            }
            ///
            //////////////


            //////////////
            ///
            /// Checkbox option
            /// 
            else if (option.Value is Options.CheckboxOption) {
                Options.CheckboxOption opt = (Options.CheckboxOption)option.Value;

                Destroy(inst);
                inst = Instantiate(checkboxExample, transform);
                Text tag = inst.GetComponentInChildren<Text>();
                tag.text = loc.GetLine(option.Key);

                Toggle checkbox = inst.GetComponentInChildren<Toggle>();
                checkbox.isOn = opt.value;
                checkbox.onValueChanged.AddListener(delegate {
                    opt.Set(checkbox.isOn);
                });
            }
            ///
            //////////////

            Vector3 position = inst.GetComponent<RectTransform>().position;
            inst.GetComponent<RectTransform>().position = new Vector3(position.x, position.y - optionsSpacing * count, position.z);

            count++;
        }

        Destroy(sliderExample);
        Destroy(checkboxExample);
    }

    public void SaveAndExit()
    {
        options.WriteAndLoadFromDisk(Paths.GetOptionsFile());
        Destroy(this.gameObject);
    }

}