using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour {

    public int id;
    public string category;
    [HideInInspector] public Localization.Lang lang;

    Localization localizer;

    void Start () {
        localizer = GameManager.instance.localization;
        UpdateText(lang);
	}
	
    public void UpdateText(Localization.Lang newLang, Localization localizationScript=null)
    {
        if (localizationScript == null) {
            localizationScript = localizer;
        }
        localizationScript.SetCategory(category);
        GetComponent<Text>().text = GameManager.instance.localization.GetLine(id);
        lang = newLang;
    }
}
