using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.IO;
using UnityEngine;

public class Localization : MonoBehaviour {

    List<Lang> languages = new List<Lang>();
    Lang currentLang;
    string currentCategory;
    Dictionary<KeyValuePair<string, int>, string> locs = new Dictionary<KeyValuePair<string, int>, string>();

    public string localizationPath = "LocFiles";

    public class Lang {

        public Sprite logo { get; }
        public string locFilePath { get; }
        public string name { get; }

        public Lang(string path)
        {
            name = Path.GetFileName(path);
            locFilePath = path + "/lang.xml";
            logo = Resources.Load<Sprite>(path + "/flag.png");
        }

        public override string ToString()
        {
            return name + "=>" + locFilePath;
        }
    }

    private void Awake()
    {
        LoadLocalizationFiles(Application.streamingAssetsPath + "/" + localizationPath);
    }

    private void Start()
    {
        LoadLocalization(0);
        Logger.Info("Loaded localization file succesfully");
    }

    void LoadLocalizationFiles(string path)
    {
        string[] dirs = Directory.GetDirectories(path);
        foreach(string dir in dirs) {
            Lang lang = new Lang(dir);
            languages.Add(lang);
        }
    }

    public void LoadLocalization(int index)
    {
        XmlDocument locFile = new XmlDocument();
        Lang lang;
        try {
            lang = languages[index];
        }
        catch (KeyNotFoundException e) {
            Logger.Throw("Could not find localization #" + index + " in the languages list. Aborting.");
            return;
        }

        string path = lang.locFilePath;
        try {
            locFile.Load(path);
        }
        catch(FileNotFoundException e) {
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
                text.UpdateText(currentLang, this);
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
