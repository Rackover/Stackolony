using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Achievements
{
	public class Property
	{
		public string concernedVariable;	// The property name
		public int currentValue;			// The current counter value
		public string rule; 				// rule to activate the property ( >, <, =>, == , ect)
		public int activationValue;			// The counter value that should be reached

		public Property(string _concernedVariable, string _rule, int _activationValue)
		{
			concernedVariable = _concernedVariable;
			rule = _rule;
			activationValue = _activationValue;
		}

		public bool IsActive()
		{
			switch(rule)
			{
				case "GREATER_THAN": return currentValue > activationValue;
				case "GREATER_OR_EQUAL_THAN": return currentValue >= activationValue;
				case "LESS_THAN": return currentValue < activationValue;
				case "LESS_OR_EQUAL_THAN": return currentValue <= activationValue;
				case "EQUAL": return currentValue == activationValue;
				default: return false;
			}
		}
	}

	public class Achievement
	{
		public int id;				// The id of the achievement
		public int propertyID;		// Propertie names used to reference in the property dictionary
		public bool unlocked;		// If the achievement is unlocked or not

		public Achievement(int ID, int _propertyID)
		{
			id = ID;
			propertyID = _propertyID;
			unlocked = false;
		}
	}

	public List<Property> properties = new List<Property>();
	public List<Achievement> achievements = new List<Achievement>();

	public void Check()
	{
		foreach(Achievement achievement in achievements)
		{
			if(!achievement.unlocked && properties[achievement.propertyID].IsActive())
			{
				achievement.unlocked = true;

				string achievementName = GameManager.instance.localization.GetLineFromCategory("achievementName", "achievement" + achievement.id);

				Logger.Debug("Achievement_" + achievement.id + " : " + achievementName + " unlocked !");
				GameManager.instance.achievementManager.unlockedAchievements.Add(achievement.id); // Add the achievement ID to the player save
				// g_SteamAchievements->SetAchievement(entry.value.name); // Trigger steam achievement	
			}
		}
	}

	// Define a new Property and returned its ID to the achievement
	public int DefineProperty(string variableName, string rule, int activationValue)
	{
		properties.Add(new Property(variableName, rule, activationValue));
		return properties.Count - 1;
	}

	// Define a new Achievement checker (Dont if the player has already got it)
	public void DefineAchievement(int id, int property)
	{
		achievements.Add(new Achievement(id, property));
	}

	// Add to the currentValue of all Properties concerned by this variable
	public void AddToValue(string variable, int value = 1)
	{
		foreach(Property p in properties)
		{
			if(p.concernedVariable == variable) p.currentValue += value;
		}
		Check();
	}

	// Set the currentValue of all Properties concerned by this variable
	public void SetValue(string variable, int value)
	{
		foreach(Property p in properties)
		{
			if(p.concernedVariable == variable) p.currentValue = value;
		}
		Check();
	}
}
