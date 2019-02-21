using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class AchievementManager : MonoBehaviour 
{
    public System.Action<int> AchievementUnlocked;
	public Achievements achiever;
	public List<int> unlockedAchievements; // TO SAVE
    public Stats stats = new Stats();

    [System.Serializable]
    public class Stats
    {
        public int stoppedRiots = 0;
        public int stoppedFires = 0;
        public int maxTowerHeight = 0;
        public int gamesPlayed = 0;
        public int maxBridgeLength = 0;
    }

	void Start()
	{
        achiever = new Achievements(AchievementUnlocked);
        LoadProperties();
    }

    // Should only be used from the interpreter
    public void UnlockAchievement(int id)
    {
        achiever.UnlockAchievement(id);
    }

    void LoadProperties()
	{
		XmlDocument achievementsData = new XmlDocument();
		achievementsData.Load(Paths.GetAchievementsFile());

		XmlNodeList achievementList = achievementsData.SelectNodes("achievements")[0].ChildNodes;

        foreach (XmlNode xAchievement in achievementList) {

            if (xAchievement.Name != "achievement") {
                // Garbage node, skipping
                continue;
            }

            int id;
            try {
                id = System.Convert.ToInt32(xAchievement.Attributes["id"].Value);
            }
            catch (System.Exception e){
                Logger.Error("Could not load id-less event [" + xAchievement.Attributes["id"].Value + "]" + xAchievement.InnerText+"\n"+e.ToString());
                continue;
            }

            string script;
            try {
                script = xAchievement["condition"].InnerText;
            }
            catch {
                Logger.Error("Could not load script-less event " + id);
                continue;
            }

            achiever.DefineAchievement(id, script);
        }
	}


    public void ClearListeners()
    {
        try {
            foreach (System.Delegate d in AchievementUnlocked.GetInvocationList()) {
                AchievementUnlocked -= (System.Action<int>)d;
            }
        }
        catch {
            // Nothing to do
        }
    }
}
