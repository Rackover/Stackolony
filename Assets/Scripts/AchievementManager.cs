using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class AchievementManager : MonoBehaviour 
{
	public static Achievements achievements = new Achievements();

	void Start()
	{
		LoadProperties();
		achievements.DefineAchievement("Installez-vous", 0, new string[] {"cyclePassed_GREATEROREQUAL_10"});
	}

	void LoadProperties()
	{
		XmlDocument properties = new XmlDocument();
		properties.Load(Application.dataPath + "/Achievements.xml");
		XmlNodeList nodeList = properties.SelectNodes("property")[0].ChildNodes;
		foreach (XmlNode node in nodeList) 
		{
			string[] propertyParams = node.InnerText.Split(new char[]{'_'}, System.StringSplitOptions.RemoveEmptyEntries);
			Debug.Log(propertyParams[1]);
			achievements.DefineProperty(propertyParams[0], node.InnerText, 0, propertyParams[1], int.Parse(propertyParams[2]));
        }
	}
}
