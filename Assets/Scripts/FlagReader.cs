using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagReader : MonoBehaviour 
{
	public void ReadFlag(BlockLink blockLink, string flag)
	{
		string[] flagElements = flag.Split(new char[]{'_'}, System.StringSplitOptions.RemoveEmptyEntries);

		switch(flagElements[0])
		{
			case "Generator":
				Debug.Log("Adding Generator behaviour on the block and giving it values.");
				Generator newGenerator = blockLink.gameObject.AddComponent<Generator>();
				newGenerator.power = int.Parse(flagElements[1]);
				blockLink.activeFlags.Add(newGenerator);
				break;

			case "MoodModifier":
				Debug.Log("Adding MoodModifier behaviour on the block and giving it values.");
				MoodModifier newMoodModifier = blockLink.gameObject.AddComponent<MoodModifier>();
				newMoodModifier.range = int.Parse(flagElements[1]);
				newMoodModifier.amount = int.Parse(flagElements[2]);
				newMoodModifier.profiles = SplitParametorInArray(flagElements[3]);
				blockLink.activeFlags.Add(newMoodModifier);
				break;
					
			case "Occupator":
				Debug.Log("Adding Occupator behaviour on the block and giving it values.");
				Occupator newOccupator = blockLink.gameObject.AddComponent<Occupator>();
				newOccupator.slotAmount = int.Parse(flagElements[1]);
				newOccupator.profiles = SplitParametorInArray(flagElements[2]);
				blockLink.activeFlags.Add(newOccupator);
				break;
								
			case "House":
				Debug.Log("Adding Home behaviour on the block and giving it values.");
				House newHome = blockLink.gameObject.AddComponent<House>();
				newHome.slotAmount = int.Parse(flagElements[1]);
				newHome.profiles = SplitParametorInArray(flagElements[2]);
				blockLink.activeFlags.Add(newHome);
				break;

			case "WorkingHours":
				Debug.Log("Adding Home behaviour on the block and giving it values.");
				WorkingHours newHours = blockLink.gameObject.AddComponent<WorkingHours>();
				newHours.startHour = float.Parse(flagElements[1]);
				newHours.endHour = float.Parse(flagElements[2]);
				blockLink.activeFlags.Add(newHours);
				break;

			default:
				Debug.Log("Warning : " + flagElements[0] + " flag is undefined. Did you forget to add it to the FlagReader ?");
				break;
		}
	}
	
	string[] SplitParametorInArray(string parameter)
	{
		return parameter.Split(new char[]{','}, System.StringSplitOptions.RemoveEmptyEntries);
	}
}
