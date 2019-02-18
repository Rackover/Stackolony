using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Achievements
{
    public Achievements(System.Action<int> evnt)
    {
        AchievementUnlocked = evnt;
    }

    System.Action<int> AchievementUnlocked;
    ScriptInterpreter scriptInterpreter = new ScriptInterpreter();
    List<Achievement> achievements = new List<Achievement>();

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
	}

	public class Achievement
	{
		public int id;				// The id of the achievement
		public bool unlocked;		// If the achievement is unlocked or not

        string script = string.Empty;
        ScriptInterpreter interpreter;


        public Achievement(int _id, string _condition, ScriptInterpreter _interpreter)
		{
			id = _id;
            unlocked = false;
            interpreter = _interpreter;

            script = "[" + _condition + "{UNLOCK_ACHIEVEMENT(id:" + id + ");}ELSE{}];";
        }
        
        // Returns true if this precise check triggered an unlock
        public bool Check()
        {
            if (unlocked) {
                return false;
            }
            
            ScriptInterpreter.Execute(
                interpreter.MakeGameEffects(script)
            );

            // Logging
            if (unlocked) {
                string achievementName = GameManager.instance.localization.GetLineFromCategory("achievementName", "achievement" + id);
                Logger.Info("Achievement_" + id + " : " + achievementName + " unlocked !");

                GameManager.instance.achievementManager.unlockedAchievements.Add(id); // Add the achievement ID to the player save
                // g_SteamAchievements->SetAchievement(entry.value.name); // Trigger steam achievement	

                return true;
            }

            return false;
        }
	}
    		
    public IEnumerator CheckAllAchievements()
    {
        foreach(Achievement achievement in achievements) {
            if (achievement.unlocked) continue;
            if (achievement.Check()) AchievementUnlocked.Invoke(achievement.id);
            yield return null;
        }
        yield return true;
    }

	// Define a new Achievement
	public void DefineAchievement(int id, string conditionScript)
	{
		achievements.Add(new Achievement(id, conditionScript, scriptInterpreter));
	}

    public void ClearAchievements()
    {
        achievements.Clear();
    }

    public void UnlockAchievement(int id)
    {
        foreach(Achievement achievement in achievements) {
            if (achievement.id == id) {
                achievement.unlocked = true;
                return;
            }
        }
    }
}
