using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class OptionsWindow : MonoBehaviour {

    public GameObject sliderExample;
    public GameObject checkboxExample;
    public GameObject selectExample;
    public GameObject container;
    public GameObject parentGameObject;
    Options options;

	// Use this for initialization
	void Start () {
        options = GameManager.instance.player.options;
        Localization loc = GameManager.instance.localization;

        Canvas canvas = GetComponentInParent<Canvas>();
        float factor = canvas.scaleFactor;

        loc.SetCategory("options");
        int count = 0;
        foreach(KeyValuePair<Options.Option, Options.IOption> option in options.Get()) {

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
                tagAndVal[0].text = loc.GetLine(option.Key.ToString());
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
                tag.text = loc.GetLine(option.Key.ToString());

                Toggle checkbox = inst.GetComponentInChildren<Toggle>();
                checkbox.isOn = opt.value;
                checkbox.onValueChanged.AddListener(delegate {
                    opt.Set(checkbox.isOn);
                });
            }
            ///
            //////////////

            /////////////
            ///
            /// Select option <string>
            ///

            else if (option.Value.GetType().IsGenericType && option.Value.GetType().GetGenericTypeDefinition() == typeof(Options.SelectOption<>)) {

                Options.IGenericTypeOption opt = option.Value as Options.IGenericTypeOption;

                Destroy(inst);
                inst = Instantiate(selectExample, transform);
                Text tag = inst.GetComponentInChildren<Text>();
                tag.text = loc.GetLine(option.Key.ToString());

                Dropdown dd = inst.GetComponentInChildren<Dropdown>();
                foreach(object obj in opt.GetRange()) {
                    string name = obj.ToString();
                    Dropdown.OptionData optionData = new Dropdown.OptionData(name);
                    dd.options.Add(optionData);
                }
                dd.value = Convert.ToInt32(opt.ToString());

                dd.onValueChanged.AddListener(delegate {
                    option.Value.SetFromString(dd.value.ToString());
                });
            }


            ///
            ////////////
            
            inst.transform.SetParent(container.transform);

            count++;
        }

        Destroy(sliderExample);
        Destroy(checkboxExample);
        Destroy(selectExample);
    }

    public void SaveAndExit()
    {
        options.WriteAndLoadFromDisk(Paths.GetOptionsFile());
        Destroy(parentGameObject);
    }

}