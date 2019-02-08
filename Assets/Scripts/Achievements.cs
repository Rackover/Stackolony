using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Achievements
{
	public class Property
	{
		public string mName;			// The property name
		public int mValue;				// The current counter value
		public int mActivationValue;	// The counter value that should be reached
		public string mActivationRule; 	// rule to activate the property ( >, <, =>, == , ect)
		public int mInitialValue;		// The value when the achievement was created

		public Property(string name, int initialValue, string activationRule, int activationValue)
		{
			mName = name;
			mInitialValue = initialValue;
			mActivationRule = activationRule;
			mActivationValue = activationValue;
		}

		public bool IsActive()
		{
			switch(mActivationRule)
			{
				case GREATER: return mValue > mActivationValue;
				case GREATEROREQUAL: return mValue >= mActivationValue;
				case LESS: return mValue < mActivationValue;
				case LESSOREQUAL: return mValue <= mActivationValue;
				case EQUAL: return mValue == mActivationValue;
				default: return false;
			}
		}
	}

	public class Achievement
	{
		public string mName;			// The name of the achievement
		public int id;					// The id of the achievement
		public string[] mProperties;	// Propertie names used to reference in the property dictionary
		public bool mUnlock;			// If the achievement is unlocked or not

		public Achievement(string name, int ID, string[] properties)
		{
			id = ID;
			mName = name;
			mProperties = properties;
			mUnlock = false;
		}
	}

	public const string GREATER = "GREATER";
	public const string LESS = "LESS";
	public const string EQUAL = "EQUAL";
	public const string GREATEROREQUAL = "GREATEROREQUAL";
	public const string LESSOREQUAL = "LESSOREQUAL";

	public Dictionary<string, List<Property>> aProperties = new Dictionary<string, List<Property>>();
	public Dictionary<string, Achievement> aAchievements = new Dictionary<string, Achievement>();

	public void Check()
	{
		foreach(KeyValuePair<string, Achievement> entry in aAchievements)
		{
			if(!entry.Value.mUnlock)
			{
				int aProps = 0;
				for( int i = 0; i < entry.Value.mProperties.Length; i++)
				{
					if(GetProperty(entry.Value.mProperties[i]).IsActive()) 
					{
						aProps++;
					}
				}

				if(aProps == entry.Value.mProperties.Length)
				{
					// THE ACHIEVEMENT IS UNLOCKED
					entry.Value.mUnlock = true;

					string name = GameManager.instance.localization.GetLineFromCategory("achievementName", "achievement" + (entry.Value.id).ToString());
					string description = GameManager.instance.localization.GetLineFromCategory("achievementDescription", "achievement" + (entry.Value.id).ToString());
					//Debug.Log(entry.Value.id + " " + name + " unlocked! " + description);
					// g_SteamAchievements->SetAchievement(entry.value.name); // Trigger steam achievement
				}
			}
		}
	}

	public Property GetProperty(string propertyName)
	{
		foreach(KeyValuePair<string, List<Property>> entry in aProperties)
		{
			foreach(Property p in entry.Value)
			{
				if(p.mName == propertyName) return p;
			}	
		}
		return null;
	}

	// Add a value to the targeted property
	public void AddToValue(string variable, int value = 1)
	{
		foreach(Property p in aProperties[variable])
		{
			p.mValue += value;
		}
		Check();
	}

	// Set the value of a property (To use a bit everywhere in the code)
	public void SetValue(string variable, int value)
	{
		foreach(Property p in aProperties[variable])
		{
			p.mValue = value;
		}
		Check();
	}

	// Define a new property and add it to the Dictionary
	public void DefineProperty(string variableName, string propertyName, int initialValue, string activationRule, int activationValue)
	{
		if(!aProperties.ContainsKey(variableName)) aProperties[variableName] = new List<Property>();

		aProperties[variableName].Add(new Property(propertyName, initialValue, activationRule, activationValue));
	}

	// Define a new achievement and add it to the Dictionary
	public void DefineAchievement(string name, int id, string[] properties)
	{
		aAchievements[name] = new Achievement(name, id, properties);
	}
}
