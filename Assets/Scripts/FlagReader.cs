using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FlagReader : MonoBehaviour 
{
	public string[] exisitingProfiles = {"all", "artist", "worker", "scientist", "military", "tourist"};

	public void ReadFlag(BlockLink blockLink, string flag)
	{
		string[] flagElements = flag.Split(new char[]{'_'}, System.StringSplitOptions.RemoveEmptyEntries);

		switch(flagElements[0])
		{
	#region Generator 
			case "Generator":
				if(flagElements.Length == 2)
				{
                    Generator newGenerator = blockLink.gameObject.AddComponent<Generator>();

                    try  // TRY SETTING UP THE POWER
					{
						newGenerator.power = int.Parse(flagElements[1]);
					}
					catch(FormatException fe)
					{
						Destroy(newGenerator);
						Debug.Log("WARNING - " + blockLink.block.name + " - Generator : Unvalid Power entry for as the first parameter. Please enter an int value.");
						break;
					}
					
					if(newGenerator.power < 0)
					{
						Destroy(newGenerator);
						Debug.Log("WARNING - " + blockLink.block.name + " - Generator : Power value should be higher than 0");
						break;
					}

					blockLink.activeFlags.Add(newGenerator);
				}
				else
				{
					Debug.Log("WARNING - " + blockLink.block.name + " - Generator : flag wrongly setup for. Should be something like this : 'Generator_3'");
				}
				break;

	#endregion

	#region MoodModifier 
			case "MoodModifier":
				if(flagElements.Length == 4)
				{
					MoodModifier newMoodModifier = blockLink.gameObject.AddComponent<MoodModifier>();

					try // TRY SETTING UP THE AMOUNT
					{
						newMoodModifier.range = int.Parse(flagElements[1]);
					}
					catch(FormatException fe)
					{
						Destroy(newMoodModifier);
						Debug.Log("WARNING - " + blockLink.block.name + " - MoodModifier : Unvalid range entry as the first parameter. Please enter an int value.");
						break;
					}

					try // TRY SETTING UP THE AMOUNT
					{
						newMoodModifier.amount = int.Parse(flagElements[2]);
					}
					catch(FormatException fe)
					{
						Destroy(newMoodModifier);
						Debug.Log("WARNING - " + blockLink.block.name + " - MoodModifier : Unvalid amount entry as the second parameter. Please enter an int value.");
						break;
					}

					newMoodModifier.profiles = SplitParametorInArray(flagElements[3]);

					string result = IsProfileArrayIsValid(newMoodModifier.profiles);

					if(result != "")
					{
						Destroy(newMoodModifier);
						Debug.Log("WARNING - " + blockLink.block.name + " - MoodModifier : " + result);
						break;
					}

					blockLink.activeFlags.Add(newMoodModifier);
				}
				else
				{
					Debug.Log("WARNING - " + blockLink.block.name + " - MoodModifier : flag wrongly setup for '" + blockLink.block.name + "'. Should be something like this : 'MoodModifier_3_5_worker'");
				}
				break;
	#endregion

	#region Occupator 
			case "Occupator":
				if(flagElements.Length == 4)
				{
					Occupator newOccupator = blockLink.gameObject.AddComponent<Occupator>();

					try  // TRY SETTING UP THE SLOTAMOUNT
					{
						newOccupator.slotAmount = int.Parse(flagElements[1]);
					}
					catch(FormatException fe)
					{
						Destroy(newOccupator);
						Debug.Log("WARNING - " + blockLink.block.name + " - Occupator : Unvalid slotAmount entry for as the first parameter. Please enter an int value.");
						break;
					}

					if(newOccupator.slotAmount < 0)
					{
						Destroy(newOccupator);
						Debug.Log("WARNING - " + blockLink.block.name + " - Occupator : Power value has to be higher than 0.");
						break;
					}
					
					
					newOccupator.profiles = SplitParametorInArray(flagElements[2]);

					string result = IsProfileArrayIsValid(newOccupator.profiles);

					if(result != "")
					{
						Destroy(newOccupator);
						Debug.Log("WARNING - " + blockLink.block.name + " - Occupator : " + result);
						break;
					}

					blockLink.activeFlags.Add(newOccupator);
				}
				else
				{
					Debug.Log("WARNING - " + blockLink.block.name + " - Occupator : flag wrongly setup. Should be something like this : 'Occupator_10_scientist,worker'");
				}
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

	#region WorkingHours 
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
	
#region Additionnal Function 

	string[] SplitParametorInArray(string parameter)
	{
		return parameter.Split(new char[]{','}, System.StringSplitOptions.RemoveEmptyEntries);
	}

	string IsProfileArrayIsValid(string[] array)
	{
		foreach(string profile in array) // Verifying if profiles array contains existing profiles
		{
			if(profile.Contains(" "))
			{
				return profile + " contains a space and it sucks.";
			}
			else if(Array.IndexOf(exisitingProfiles, profile) < -1)
			{
				return profile + " is not a existing profile name, check online sheets to find the right ones.";
			}

		}
		return "";
	}

#endregion
}
