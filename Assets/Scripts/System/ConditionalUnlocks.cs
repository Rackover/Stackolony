using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class ConditionalUnlocks {

    class Unlock{
        public int buildingId;
        public List<string> conditions;

        public Unlock(int _buildingId, List<string> _conditions)
        {
            buildingId = _buildingId;
            conditions = _conditions;
        }
    }

    bool isLoaded = false;
    List<Unlock> unlocks = new List<Unlock>();
    ScriptInterpreter interpreter = new ScriptInterpreter();

    public bool CanBeUnlocked(int buildingId)
    {
        Unlock buildingUnlock = GetUnlock(buildingId);

        // No specific unlock conditions
        if (buildingUnlock == null) {
            return true;
        }

        foreach(string condition in buildingUnlock.conditions) {
            if (!interpreter.InterpretComparison(condition)) {
                return false;
            }
        }
        return true;
    }

    public List<string[]> GetFormattedUnlockConditions(int buildingId)
    {
        Unlock buildingUnlock = GetUnlock(buildingId);
        List<string[]> conditions = new List<string[]>();

        // No specific unlock conditions
        if (buildingUnlock == null) {
            return conditions;
        }

        foreach (string condition in buildingUnlock.conditions) {
            conditions.Add(
                interpreter.FormatComparison(condition)
            );
        }
        return conditions;
    }

    Unlock GetUnlock(int buildingId)
    {
        foreach (Unlock unlock in unlocks) {
            if (unlock.buildingId == buildingId) {
                return unlock;
            }
        }
        return null;
    }

    public void LoadConditionalUnlocks()
    {
        string path = Paths.GetUnlocksFile();
        XmlDocument timeFile = new XmlDocument();

        try {
            timeFile.Load(path);
        }
        catch (FileNotFoundException e) {
            Logger.Throw("Could not access timeline file at path " + path + ". Error : " + e.ToString());
            return;
        }

        XmlNodeList nodeList = timeFile.SelectNodes("unlocks")[0].ChildNodes;
        foreach (XmlNode xUnlock in nodeList) {
            // Garbage node
            if (xUnlock.Name != "building") {
                continue;
            }

            int id = System.Convert.ToInt32(xUnlock.Attributes["id"].Value);
            List<string> conditions = new List<string>();

            foreach(XmlNode xCondition in xUnlock.ChildNodes) {
                if (!ScriptInterpreter.CheckSyntax(xCondition.InnerText)) {
                    Logger.Error("Skipped wrong syntax in code " + xCondition.InnerText + " in building unlock " + id);
                    continue;
                }
                conditions.Add(xCondition.InnerText);
            }

            unlocks.Add(new Unlock(id, conditions));

        }
    }

}
