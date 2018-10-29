using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newInputProfile", menuName = "InputProfile")]
public class TempInputManager : ScriptableObject {

	public string profileName;

	public Input[] inputs;

	[System.Serializable]
	public class Input
	{
		public string name;
	}
}
