using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbiantSound : MonoBehaviour {

	public string sound;
	public SFXManager sFXManager;

	void Start () 
	{
		sFXManager.PlaySoundAtPosition(sound, transform.position, 1, 1, true);

	}

}
