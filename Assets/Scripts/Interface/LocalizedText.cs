using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour {

    public int id;
    public string category;
    [HideInInspector] public string lang = "";

    Localization localizer;

    void Start () {
        localizer = GameManager.instance.localization;
        UpdateText(lang);
	}
	
    public void UpdateText(string newLang)
    {
        localizer.SetCategory(category);
        GetComponent<Text>().text = GameManager.instance.localization.GetLine(id);
        lang = newLang;
    }
}
