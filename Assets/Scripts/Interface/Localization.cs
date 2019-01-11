using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using UnityEngine;

public class Localization : MonoBehaviour {

    XmlDocument locFile = new XmlDocument();
    public List<string> languages = new List<string>();

    string currentLang;
    string currentCategory;
    Dictionary<KeyValuePair<string, int>, string> locs = new Dictionary<KeyValuePair<string, int>, string>();

    int test = 0;

    private void Start()
    {
        LoadLocalization(0);
        Logger.Info("Loaded localization file succesfully");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) {
            test++;
            if (test >= languages.Count) {
                test = 0;
            }
            LoadLocalization(test);
        }
    }

    public void LoadLocalization(int index)
    {
        string lang;
        try {
            lang = languages[index];
        }
        catch (KeyNotFoundException e) {
            Logger.Throw("Could not find localization #" + index + " in the languages list. Aborting.");
            return;
        }

        string path = Application.streamingAssetsPath + "/LocFiles/" + lang + ".xml";
        try {
            locFile.Load(path);
        }
        catch(System.IO.FileNotFoundException e) {
            Logger.Throw("Could not access localization file at path "+ path+". Error : "+e.ToString());
            return;
        }
        XmlNodeList nodeList = locFile.SelectNodes("lines")[0].ChildNodes;
        foreach (XmlNode node in nodeList) {
            string cat = node.Name;
            foreach (XmlNode subNode in node.ChildNodes) {
                if (subNode.Attributes == null) {
                    Logger.Debug("Skipping node " + subNode.Name + " (no attributes)");
                    continue;
                }
                XmlAttribute attrId = subNode.Attributes["id"];
                if (attrId == null) {
                    Logger.Debug("Skipping node " + subNode.Name + " (no ID attribute)");
                    continue;
                }
                int id = Int16.Parse(attrId.InnerText);
                Logger.Debug("Added line [" + cat + ":" + id.ToString()+"]");
                locs[new KeyValuePair<string, int>(cat, id)] = subNode.InnerText;
            }
        }

        currentLang = lang;
        UpdateLocalizedTexts();
    }

    public void UpdateLocalizedTexts()
    {
        foreach(LocalizedText text in FindObjectsOfType<LocalizedText>()) {
            if (text.lang != currentLang) {
                text.UpdateText(currentLang);
            }
        }
    }

    public void SetCategory(string category)
    {
        currentCategory = category;
    }

    public string GetLine(int id)
    {
        try { 
            string line = locs[new KeyValuePair<string, int>(currentCategory, id)];
            return line;
        }
        catch(KeyNotFoundException e) {
            Logger.Error("Could not load line ID " + id);
            return "LOC " + currentCategory + ":" + id;
        }
    }
}
