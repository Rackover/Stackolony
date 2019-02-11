using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class AchievementManager : MonoBehaviour 
{
	public static Achievements achiever = new Achievements();
	public static List<int> unlockedAchievements; // TO SAVE

	void Start()
	{
		LoadProperties();
	}

	void LoadProperties()
	{
		XmlDocument achievementsData = new XmlDocument();
		achievementsData.Load(Application.dataPath + "/StreamingAssets/Achievements.xml");

		XmlNodeList achievementList = achievementsData.SelectNodes("achievements")[0].ChildNodes;

		for(int i = 0; i < achievementList.Count; i++)
		{	
			if(!unlockedAchievements.Contains(i))
			{
				string aName = achievementList[i].SelectNodes("name")[0].InnerText;
				string aDescription = achievementList[i].SelectNodes("description")[0].InnerText;

				string concernedVariable = achievementList[i].SelectNodes("property")[0].SelectNodes("concernedVariable")[0].InnerText;
				string rule = achievementList[i].SelectNodes("property")[0].SelectNodes("rule")[0].InnerText;
				int activationValue = int.Parse(achievementList[i].SelectNodes("property")[0].SelectNodes("activationValue")[0].InnerText);

				achiever.DefineAchievement(
					aName,
					int.Parse(achievementList[i].Name.Replace("achievement", "")),
					achiever.DefineProperty(concernedVariable, rule, activationValue)
				);
			}
		}
	}
}
