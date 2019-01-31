﻿using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.IO;
using System;

public class TimelineController : MonoBehaviour {

    class CycleInformation
    {
        public List<int> unlocks = new List<int>();
        public Dictionary<Population, int> settlers = new Dictionary<Population, int>();
    }

    List<CycleInformation> cycles;
    CycleInformation currentCycle;

    public void UpdateCycle(int cycleNumber)
    {
        currentCycle = cycleNumber < cycles.Count ? cycles[cycleNumber] : null;

        if (currentCycle == null) return;

        foreach (KeyValuePair<Population, int> settler in currentCycle.settlers) {
            GameManager.instance.populationManager.SpawnCitizens(settler.Key, settler.Value);
        }

        foreach(int blockId in currentCycle.unlocks) {
            GameManager.instance.cityManager.UnlockBuilding(blockId);
        }

        Logger.Debug("New cycle, spawning " + currentCycle.settlers.Count.ToString() + " citizens and unlocking " + currentCycle.unlocks.Count.ToString() + " buildings");
    }

    public void LoadCycles()
    {
        cycles = new List<CycleInformation>();

        string path = Paths.GetTimelineFile();
        XmlDocument locFile = new XmlDocument();
        
        try {
            locFile.Load(path);
        }
        catch (FileNotFoundException e) {
            Logger.Throw("Could not access timeline file at path " + path + ". Error : " + e.ToString());
            return;
        }

        XmlNodeList nodeList = locFile.SelectNodes("timeline")[0].ChildNodes;
        foreach (XmlNode xCycle in nodeList) {
            // Garbage node
            if (xCycle.Name != "cycle") {
                continue;
            }

            CycleInformation cycle;

            try {
                cycle = ReadXCycle(xCycle);
                cycles.Add(cycle);
            }
            catch(Exception e) {
                Logger.Error("Skipped timeline cycle because of an error while reading : " + e.ToString());
            }
        }
    }

    CycleInformation ReadXCycle(XmlNode xCycle)
    {
        CycleInformation cycle = new CycleInformation();

        foreach (XmlNode xProperty in xCycle.ChildNodes) {
            switch (xProperty.Name) {
                case "unlocks":
                    foreach(XmlNode xUnlock in xProperty.ChildNodes) {
                        // Garbage unlock
                        if (xUnlock.Name != "building") {
                            continue;
                        }

                        int id = Convert.ToInt32(xUnlock.InnerText);

                        // Garbage ID
                        if (!GameManager.instance.library.BlockExists(id)) {
                            continue;
                        }

                        cycle.unlocks.Add(id);
                    }
                    break;

                case "settlers":
                    foreach (XmlNode xSettler in xProperty.ChildNodes) {

                        string codeName = xSettler.Name;

                        Population pop;
                        if (codeName == "_random") {
                            pop = GameManager.instance.populationManager.populationTypeList[Mathf.FloorToInt(GameManager.instance.populationManager.populationTypeList.Length)];
                        }
                        else {
                            pop = GameManager.instance.populationManager.GetPopulationByCodename(codeName);
                        }

                        // Garbage codename
                        if (pop == null) {
                            continue;
                        }

                        int amount = 0;
                        if (xSettler.InnerText.Contains("~")) {
                            string[] values = xSettler.InnerText.Split('~');
                            amount = Mathf.RoundToInt(Convert.ToInt32(values[0]) + Convert.ToInt32(values[1]) * UnityEngine.Random.value);
                        }
                        else {
                            amount = Convert.ToInt32(xSettler.InnerText);
                        }

                        cycle.settlers.Add(pop, amount);
                    }
                    break;
            }

        }

        return cycle;
    }

}
