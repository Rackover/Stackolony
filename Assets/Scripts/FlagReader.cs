using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FlagReader : MonoBehaviour 
{
	public void ReadFlag(BlockLink blockLink, string flag)
	{
		string[] flagElements = flag.Split(new char[]{'_'}, System.StringSplitOptions.RemoveEmptyEntries);

		switch(flagElements[0])
		{
	#region Generator 
			case "Generator":
				Generator newGenerator = blockLink.gameObject.AddComponent<Generator>();

				try
				{
					newGenerator.power = int.Parse(flagElements[1]);
				}
				catch(ArgumentNullException ane)
				{
					Destroy(newGenerator);
					Debug.Log(ane.Message);
					break;
				}

				if(newGenerator.power < 0)
					newGenerator.power = 0;

				blockLink.activeFlags.Add(newGenerator);
				break;
#endregion

	#region MoodModifier 
			case "MoodModifier":
				MoodModifier newMoodModifier = blockLink.gameObject.AddComponent<MoodModifier>();
				newMoodModifier.range = int.Parse(flagElements[1]);
				newMoodModifier.amount = int.Parse(flagElements[2]);
				newMoodModifier.profiles = SplitParametorInArray(flagElements[3]);
				blockLink.activeFlags.Add(newMoodModifier);
				break;
#endregion

	#region Occupator 
			case "Occupator":
				Occupator newOccupator = blockLink.gameObject.AddComponent<Occupator>();
				newOccupator.slotAmount = int.Parse(flagElements[1]);
				newOccupator.profiles = SplitParametorInArray(flagElements[2]);
				blockLink.activeFlags.Add(newOccupator);
				break;
#endregion

	#region House 
			case "House":
				House newHome = blockLink.gameObject.AddComponent<House>();
				newHome.slotAmount = int.Parse(flagElements[1]);
				newHome.profiles = SplitParametorInArray(flagElements[2]);
				blockLink.activeFlags.Add(newHome);
				break;
#endregion

	#region WorkingHoure 
			case "WorkingHours":
				WorkingHours newHours = blockLink.gameObject.AddComponent<WorkingHours>();
				newHours.startHour = float.Parse(flagElements[1]);
				newHours.endHour = float.Parse(flagElements[2]);
				blockLink.activeFlags.Add(newHours);
				break;
	#endregion

	#region PoliceStation 
			case "PoliceStation":
				PoliceStation newPoliceStation = blockLink.gameObject.AddComponent<PoliceStation>();
				newPoliceStation.range = int.Parse(flagElements[1]);
				blockLink.activeFlags.Add(newPoliceStation);
				break;
#endregion

	#region FiremanStation 
			case "FiremanStation":
				FiremanStation newFiremanStation = blockLink.gameObject.AddComponent<FiremanStation>();
				newFiremanStation.range = int.Parse(flagElements[1]);
				blockLink.activeFlags.Add(newFiremanStation);
				break;
#endregion

	#region Repairer 
			case "Repairer":
				Repairer newRepairer = blockLink.gameObject.AddComponent<Repairer>();
				newRepairer.range = int.Parse(flagElements[1]);
				blockLink.activeFlags.Add(newRepairer);
				break;
	#endregion

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
