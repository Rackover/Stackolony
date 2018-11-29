using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagReader : MonoBehaviour 
{
	public void ReadFlag(GameObject obj, string flag)
	{
		string[] flagElements = flag.Split(new char[]{'_'}, System.StringSplitOptions.RemoveEmptyEntries);
		
		switch(flagElements[0])
		{
			case "Generator":
				Debug.Log("Adding Generator behaviour on the block and giving it values.");
				Generator newGenerator = obj.AddComponent<Generator>();
				newGenerator.power = int.Parse(flagElements[1]);
				newGenerator.type = flagElements[2];
				break;
			default:
				Debug.Log("Warning : " + flagElements[0] + " flag is undefined. Did you forget to add it to the FlagReader ?");
				break;
		}
	}
}
