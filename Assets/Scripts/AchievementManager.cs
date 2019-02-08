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
	}

	void LoadProperties()
	{
		XmlDocument properties = new XmlDocument();
		properties.Load(Application.dataPath + "/StreamingAssets/Achievements.xml");
		XmlNodeList nodeList = properties.SelectNodes("property")[0].ChildNodes;
		for(int i = 0; i < nodeList.Count; i++)
		{
			string[] propertyParams = nodeList[i].InnerText.Split(new char[]{'_'}, System.StringSplitOptions.RemoveEmptyEntries);
			achievements.DefineProperty(propertyParams[0], nodeList[i].InnerText, 0, propertyParams[1], int.Parse(propertyParams[2]));
			achievements.DefineAchievement(nodeList[i].InnerText, int.Parse(nodeList[i].Name.Replace("property", "")), new string[] {nodeList[i].InnerText});
		}
	}
}
