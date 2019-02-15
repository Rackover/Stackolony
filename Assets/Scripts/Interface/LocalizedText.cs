using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour {

    public string id;
    public string category;
    public bool isAllCaps = false;
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
        string txt = GameManager.instance.localization.GetLine(id);
            
        GetComponent<Text>().text = isAllCaps ? txt.ToUpper() : txt;

        lang = newLang;
    }
}
