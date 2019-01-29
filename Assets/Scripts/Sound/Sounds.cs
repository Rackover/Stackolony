using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "sound", menuName = "sound")]
public class Sounds : ScriptableObject 
{
	public SoundManager.Sound[] sounds;
}
