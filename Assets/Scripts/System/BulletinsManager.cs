using System.IO;
using System.Collections;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BulletinsManager : MonoBehaviour {

    private void Awake()
    {
        LoadBulletins();
    }

    public class Bulletin
    {
        public int id = 1;
        public int eventId = 0;
        public int minCycle = 0;

        public Bulletin(int _id, int _minCycle)
        {
            id = _id;
            minCycle = _minCycle;
        }
    }

    List<Bulletin> bulletinList = new List<Bulletin>();
    Bulletin currentBulletin;

    public void Renew(int currentCycle)
    {
        Bulletin pick = null;
        List<Bulletin> newList = new List<Bulletin>();
        foreach (Bulletin bulletin in bulletinList) {
            if (pick == null && 
                bulletin.minCycle <= currentCycle) {
                pick = bulletin;
                continue;
            }
            newList.Add(bulletin);
        }

        if (pick == null) {
            // No more bulletins, let's reload
            LoadBulletins();
            return;
        }

        currentBulletin = pick;
        bulletinList = newList;
        GameManager.instance.soundManager.Play("BulletinPop");

    }

    void LoadBulletins()
    {

        // XML Loading
        string path = Paths.GetBulletinFile();
        XmlDocument bulletinFile = new XmlDocument();
        try {
            bulletinFile.Load(path);
        }
        catch (FileNotFoundException e) {
            Logger.Throw("Could not access localization file at path " + path + ". Error : " + e.ToString());
            return;
        }

        // XML Parsing & List filling
        XmlNodeList bulletins = bulletinFile.SelectNodes("bulletins")[0].ChildNodes;
        foreach(XmlNode pool in bulletins) {
            foreach (XmlNode xBulletin in pool.ChildNodes) {

                // Most probably a comment - skip
                if (xBulletin.Name != "bulletin") {
                    continue;
                }

                // Bulletin creation
                Bulletin bulletin = new Bulletin(
                    int.Parse(xBulletin.Attributes["id"].Value),
                    int.Parse(pool.Attributes["minCycle"].Value)
                );

                bulletinList.Add(bulletin);
            }
        }
        Logger.Debug("Loaded " + bulletinList.Count.ToString() + " bulletins");

        //Shuffling
        System.Random rng = new System.Random((Int32)DateTime.Now.TimeOfDay.TotalMilliseconds);
        bulletinList = bulletinList.OrderBy((a => rng.Next())).ToList();

        // Loading first cycle of bulletins
        Renew(0);
    }

    public Bulletin GetBulletin()
    {
        return currentBulletin;
    }
}
