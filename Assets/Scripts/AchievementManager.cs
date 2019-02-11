using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class AchievementManager : MonoBehaviour 
{
	public Achievements achiever = new Achievements();
	public List<int> unlockedAchievements; // TO SAVE

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

				XmlNode property = achievementList[i].SelectNodes("property")[0];

				string concernedVariable = property.SelectNodes("concernedVariable")[0].InnerText;
				string rule = property.SelectNodes("rule")[0].InnerText;
				int activationValue = int.Parse(property.SelectNodes("activationValue")[0].InnerText);

				achiever.DefineAchievement(
					aName,
					int.Parse(achievementList[i].Name.Replace("achievement", "")),
					achiever.DefineProperty(concernedVariable, rule, activationValue)
				);
			}
		}
	}
}
