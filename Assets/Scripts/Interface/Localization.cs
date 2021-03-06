﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;

public class Localization : MonoBehaviour {

    List<Lang> languages = new List<Lang>();
    Lang currentLang;
    string currentCategory;
    Dictionary<KeyValuePair<string, string>, string> locs = new Dictionary<KeyValuePair<string, string>, string>();
    string[] varNames;

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
            return name;
        }
    }
    
    [Serializable]
    public class Line
    {
        public string category { get; }
        public string id { get; }
        public string[] values { get; }


        public Line(Tooltip.Entry tt, params string[] vs)
        {
            category = tt.category;
            id = tt.id;
            values = vs;
        }

        public Line(string c, string i, params string[] vs)
        {
            category = c;
            id = i;
            values = vs;
        }
    }

    private void Start()
    {
        LoadLocalizationFiles(Paths.GetLocFolder());
        varNames = Environment.GetVarNames();

        // Init
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
        Lang lang;
        try {
            lang = languages[index];
        }
        catch (KeyNotFoundException) {
            Logger.Throw("Could not find localization #" + index + " in the languages list. Aborting.");
            return;
        }

        LoadLocalization(lang);
    }

    public void LoadLocalization(Lang lang)
    {

        XmlDocument locFile = new XmlDocument();

        string path = lang.locFilePath;
        try {
            locFile.Load(path);
        }
        catch(FileNotFoundException e) {
            Logger.Throw("Could not access localization file at path "+ path+". Error : "+e.ToString());
            return;
        }

        locs = new Dictionary<KeyValuePair<string, string>, string>();  

        XmlNodeList nodeList = locFile.SelectNodes("lines")[0].ChildNodes;
        foreach (XmlNode node in nodeList) {
            string cat = node.Name;
            foreach (XmlNode subNode in node.ChildNodes) {
                string id = subNode.Name;
                Logger.Debug("Added line [" + cat + ":" + subNode.Name+ "]");
                locs[new KeyValuePair<string, string>(cat, id)] = subNode.InnerText;
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

    public string GetLine(Line line)
    {
        return GetLineFromCategory(line.category, line.id, line.values);
    }

    public string GetLineFromCategory(string category, string id, params string[] values)
    {
        SetCategory(category);
        return GetLine(id, values);
    }
    
    public string GetLine(string id, params string[] values)
    {
        if (GetLanguages().Count <= 0) {
            return "[LOC:NO LANGUAGES LOADED]";
        }
        try {
            string line = string.Format(locs[new KeyValuePair<string, string>(currentCategory, id)], values);

            // Workaround to eliminate whitespaces before ! and ? in english
            if (currentLang.name == "english") {
                line = Regex.Replace(line, @"( \?)", "?");
                line = Regex.Replace(line, @"( \:)", ":");
                line = Regex.Replace(line, @"( \!)", "!");
            }
            // end of

            return Interpret(line);
        }
        catch(KeyNotFoundException) {
            Logger.Error("Could not load line "+ currentCategory+":"+ id);
            return "[LOC:" + currentCategory + ":" + id+"]";
        }
        catch(FormatException)
        {
            Logger.Error("Line : " + id + " in " + currentCategory + " category has too much parameters : "+string.Join(",",values));
            return "[LOC:" + currentCategory + ":" + id+"] TOO MUCH PARAMS";
        }
    }

    public delegate string InterpretedName();

    string Interpret(string line)
    {
        foreach(string word in varNames) {
            line = line.Replace("$"+ word, Environment.GetVariable(word));
        }
        return line;
    }

    public List<string> GetLanguageNames()
    {
        List<string> langs = new List<string>();
        foreach(Lang lang in languages) {
            langs.Add(lang.name);
        }
        return langs;
    }

    public List<Lang> GetLanguages()
    {
        return languages;
    }
}
