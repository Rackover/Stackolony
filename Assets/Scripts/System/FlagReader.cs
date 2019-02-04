using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class FlagReader : MonoBehaviour 
{
    public List<string> existingProfiles;

    void Start()
    {
        // FIND ALL EXISTING PROFILES
        int profileCount = GameManager.instance.populationManager.populationTypeList.Length;
        existingProfiles = new List<string>();
        for (var i = 0; i < profileCount; i++)
        {
            existingProfiles.Add(GameManager.instance.populationManager.populationTypeList[i].codeName);
        }
    }

    public static bool IsPositive(string flagName)
    {
        switch (flagName) {
            case "Generator":
            case "MoodModifier":
            case "Occupator":
            case "House":
            case "FoodProvider":
            case "PoliceStation":
            case "FiremanStation":
            case "Repairer":
            case "Spatioport":
                return true;
        }
        return false;

    }

    public static List<string> UnpackFlag(string flag)
    {
        return new List<string>(flag.Split('_'));
    }

    public string GetInvertedFlag(string flag)
    {
        string invertedFlag = "";
        for (int i = 0; i < flag.Length; i++)
        {
            if (System.Char.IsDigit(flag[i]) && flag[i-1].ToString() == "_") {
                invertedFlag += "-" + flag[i];
            } else {
                invertedFlag += flag[i];
            }
        }



        return invertedFlag;
    }

    public static List<string> GetRawFlags (BlockScheme blockScheme) {
        List<List<string>> flags = GetFlags(blockScheme);

        List<string> list = new List<string>();
        foreach(List<string> parameters in flags) {
            list.Add(parameters[0]);
        }

        return list;
    }
    
    public static List<List<string>> GetFlags(BlockScheme blockScheme)
    {
        List<List<string>> list = new List<List<string>>();

        foreach (string flag in blockScheme.flags) {
            list.Add(UnpackFlag(flag));
        }

        return list;
    }

    public static CityManager.BuildingType GetCategory(BlockScheme scheme)
    {
        string[] rawFlags = GetRawFlags(scheme).ToArray();

        // Habitation
        if (rawFlags.Contains("House")) {
            return CityManager.BuildingType.Habitation;
        }

        // Occupators
        if (rawFlags.Contains("Occupator")) {
            return CityManager.BuildingType.Occupators;
        }

        // Services
        return CityManager.BuildingType.Services;
    }

    public void ReadFlag(Block block, string flag)
	{
		string[] flagElements = flag.Split(new char[]{'_'}, System.StringSplitOptions.RemoveEmptyEntries);

		switch(flagElements[0])
		{
	#region Generator 
			case "Generator":
				if(flagElements.Length == 2)
				{
                    Generator foundGenerator = block.GetComponent<Generator>();

                    if (foundGenerator == null)
                    {
                        Generator newGenerator = block.gameObject.AddComponent<Generator>();

                        try  // TRY SETTING UP THE POWER
                        {
                            newGenerator.power = int.Parse(flagElements[1]);
                        }
                        catch (FormatException fe)
                        {
                            Destroy(newGenerator);
                            Debug.Log(block.scheme.name + " - Generator : Unvalid Power entry for as the first parameter. Please enter an int value.");
                            break;
                        }

                        if (newGenerator.power < 0)
                        {
                            Destroy(newGenerator);
                            Debug.Log(block.scheme.name + " - Generator : Power value should be higher than 0");
                            break;
                        }

                        block.activeFlags.Add(newGenerator);
                    } else
                    {
                        foundGenerator.power += int.Parse(flagElements[1]);
                    }
				}
				else
				{
                    Logger.Warn(block.scheme.name + " - Generator : flag wrongly setup for. Should be something like this : 'Generator_3'");
				}
				break;

	#endregion

	#region Occupator 
			case "Occupator":
				if(flagElements.Length == 4)
				{
                    Occupator foundOccupator = block.GetComponent<Occupator>();
                    if (foundOccupator == null)
                    {
                        Occupator newOccupator = block.gameObject.AddComponent<Occupator>();
                        int slotAmount = 0;
                        try  // TRY SETTING UP THE SLOTAMOUNT
                        {
                            slotAmount = int.Parse(flagElements[1]);
                        }
                        catch (FormatException fe)
                        {
                            Destroy(newOccupator);
                            Logger.Warn(block.scheme.name + " - Occupator : Unvalid slotAmount entry for as the first parameter. Please enter an int value.");
                            break;
                        }

                        if (slotAmount < 0)
                        {
                            Destroy(newOccupator);
                            Logger.Warn(block.scheme.name + " - Occupator : slot value has to be higher than 0.");
                            break;
                        }

                        try  // TRY SETTING UP THE RANGE
                        {
                            newOccupator.range = int.Parse(flagElements[2]);
                        }
                        catch (FormatException fe)
                        {
                            Destroy(newOccupator);
                            Logger.Warn(block.scheme.name + " - Occupator : Unvalid range entry for as the first parameter. Please enter an int value.");
                            break;
                        }

                        if (newOccupator.range < 0)
                        {
                            Destroy(newOccupator);
                            Logger.Warn(block.scheme.name + " - Occupator : range value has to be higher than 0.");
                            break;
                        }


                        string[] profiles = SplitParametorInArray(flagElements[3]);

                        string result = IsProfileArrayIsValid(profiles);

                        if (result != "")
                        {
                            Destroy(newOccupator);
                            Logger.Warn(block.scheme.name + " - Occupator : " + result);
                            break;
                        }

                        newOccupator.slots = slotAmount;
                        Population[] acceptedPop = new Population[profiles.Length];

                        for (int i = 0; i < profiles.Length; i++)
                        {
                            foreach (Population pop in GameManager.instance.populationManager.populationTypeList)
                            {
                                if (profiles[i] == pop.codeName)
                                {
                                    acceptedPop[i] = pop;
                                }
                            }
                        }

                        newOccupator.acceptedPopulation = acceptedPop;


                        block.activeFlags.Add(newOccupator);
                    } else
                    {
                        foundOccupator.range += int.Parse(flagElements[2]);
                        foundOccupator.slots += int.Parse(flagElements[1]);
                    }
                }
				else
				{
                    Logger.Warn( block.scheme.name + " - Occupator : flag wrongly setup. Should be something like this : 'Occupator_10_5_scientist,worker'");
				}
				break;
	#endregion

	#region House 
			case "House":
                House foundHouse = block.GetComponent<House>();
                if (foundHouse == null)
                {
                    House newHome = block.gameObject.AddComponent<House>();
                    newHome.slotAmount = int.Parse(flagElements[1]);
                    newHome.standingLevel = int.Parse(flagElements[2]);
                    string[] profilesList = SplitParametorInArray(flagElements[3]);
                    Population[] acceptedPopList = new Population[profilesList.Length];
                    for (int i = 0; i < profilesList.Length; i++)
                    {
                        foreach (Population pop in GameManager.instance.populationManager.populationTypeList)
                        {
                            if (profilesList[i] == pop.codeName)
                            {
                                acceptedPopList[i] = pop;
                            }
                        }
                    }
                    newHome.acceptedPop = acceptedPopList;
                    newHome.InitCitizensSlots();
                    block.activeFlags.Add(newHome);
                } else
                {
                    foundHouse.slotAmount += int.Parse(flagElements[1]);
                    foundHouse.standingLevel = int.Parse(flagElements[2]);
                }
				break;
            #endregion

    #region FoodProvider
            case "FoodProvider":
                FoodProvider foundFoodProvider = block.GetComponent<FoodProvider>();
                if (foundFoodProvider == null)
                {
                    FoodProvider newFoodProvider = block.gameObject.AddComponent<FoodProvider>();
                    newFoodProvider.range = int.Parse(flagElements[1]);
                    newFoodProvider.foodTotal = int.Parse(flagElements[2]);
                    newFoodProvider.foodLeft = newFoodProvider.foodTotal;
                    block.activeFlags.Add(newFoodProvider);
                } else
                {
                    foundFoodProvider.range += int.Parse(flagElements[1]);
                    foundFoodProvider.foodTotal += int.Parse(flagElements[2]);
                    foundFoodProvider.foodLeft += foundFoodProvider.foodTotal;
                }
                break;
            #endregion

    #region WorkingHours 
            case "WorkingHours":
                WorkingHours foundWorkingHours = block.GetComponent<WorkingHours>();
                if (foundWorkingHours == null)
                {
                    WorkingHours newHours = block.gameObject.AddComponent<WorkingHours>();
                    newHours.startHour = float.Parse(flagElements[1]);
                    newHours.endHour = float.Parse(flagElements[2]);
                    block.activeFlags.Add(newHours);
                } else
                {
                    foundWorkingHours.startHour = float.Parse(flagElements[1]);
                    foundWorkingHours.endHour = float.Parse(flagElements[2]);
                }
				break;
	#endregion

	#region PoliceStation 
			case "PoliceStation":
                PoliceStation foundPoliceStation = block.GetComponent<PoliceStation>();
                if (foundPoliceStation == null)
                {
                    PoliceStation newPoliceStation = block.gameObject.AddComponent<PoliceStation>();
                    newPoliceStation.range = int.Parse(flagElements[1]);
                    block.activeFlags.Add(newPoliceStation);
                } else
                {
                    foundPoliceStation.range += int.Parse(flagElements[1]);
                }
				break;
#endregion

	#region FiremanStation 
			case "FiremanStation":
                FiremanStation foundFiremanStation = block.GetComponent<FiremanStation>();
                if (foundFiremanStation == null)
                {
                    FiremanStation newFiremanStation = block.gameObject.AddComponent<FiremanStation>();
                    newFiremanStation.range = int.Parse(flagElements[1]);
                    block.activeFlags.Add(newFiremanStation);
                } else
                {
                    foundFiremanStation.range += int.Parse(flagElements[1]);
                }
				break;
#endregion

	#region Repairer 
			case "Repairer":
                Repairer foundRepairer = block.GetComponent<Repairer>();
                if (foundRepairer == null)
                {
                    Repairer newRepairer = block.gameObject.AddComponent<Repairer>();
                    newRepairer.range = int.Parse(flagElements[1]);
                    block.activeFlags.Add(newRepairer);
                } else
                {
                    foundRepairer.range += int.Parse(flagElements[1]); 
                }
				break;
            #endregion

    #region Spatioport
            case "Spatioport":
                Spatioport foundSpatioport = block.GetComponent<Spatioport>();
                if (foundSpatioport == null)
                {
                    Spatioport newSpatioport = block.gameObject.AddComponent<Spatioport>();
                    block.activeFlags.Add(newSpatioport);
                }
                break;
    #endregion

    #region FlagNeeder
            case "FlagNeeder":
                FlagNeeder foundFlagNeeder = block.GetComponent<FlagNeeder>();
                if (foundFlagNeeder == null)
                {
                    FlagNeeder newFlagReader = block.gameObject.AddComponent<FlagNeeder>();
                    newFlagReader.needed = flagElements[1];
                    newFlagReader.range = int.Parse(flagElements[2]);
                    block.activeFlags.Add(newFlagReader);
                } else
                {
                    foundFlagNeeder.needed = flagElements[1];
                    foundFlagNeeder.range += int.Parse(flagElements[2]);
                }
                break;
    #endregion

    #region Nest
            case "Nest":
                Nest foundNest = block.GetComponent<Nest>();
                if (foundNest == null)
                {
                    Nest newNest = block.gameObject.AddComponent<Nest>();
                    block.activeFlags.Add(newNest);
                }
                break;
    #endregion

    #region Mine
            case "Mine":
                Mine foundMine = block.GetComponent<Mine>();
                if (foundMine == null)
                {
                    Mine newMine = block.gameObject.AddComponent<Mine>();
                    block.activeFlags.Add(newMine);
                }
                break;
            #endregion

    #region NuisanceGenerator
            case "NuisanceGenerator":
                NuisanceGenerator foundNuisanceGenerator = block.GetComponent<NuisanceGenerator>();
                if (foundNuisanceGenerator == null)
                {
                    NuisanceGenerator newNGenerator = block.gameObject.AddComponent<NuisanceGenerator>();
                    newNGenerator.range = int.Parse(flagElements[1]);
                    newNGenerator.amount = int.Parse(flagElements[2]);
                    block.activeFlags.Add(newNGenerator);
                } else
                {
                    foundNuisanceGenerator.range += int.Parse(flagElements[1]);
                    foundNuisanceGenerator.amount += int.Parse(flagElements[2]);
                }
                break;
            #endregion

    #region FireRiskGenerator
            case "FireRiskGenerator":
                FireRiskGenerator foundFireRiskGenerator = block.GetComponent<FireRiskGenerator>();
                if (foundFireRiskGenerator == null)
                {
                    FireRiskGenerator newFG = block.gameObject.AddComponent<FireRiskGenerator>();
                    newFG.amountInPercent = int.Parse(flagElements[1]);
                    block.activeFlags.Add(newFG);
                }
                else
                {
                    foundFireRiskGenerator.amountInPercent += int.Parse(flagElements[1]);
                }
                break;
            #endregion
            default:
				Logger.Warn("Warning : " + flagElements[0] + " flag is undefined. Did you forget to add it to the FlagReader ?");
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
            else if (!existingProfiles.Contains(profile))
            {
                return profile + "is not an existing profile, check online sheets to find the right ones";
            }

		}
		return "";
	}

#endregion
}
