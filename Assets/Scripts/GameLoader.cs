using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour 
{
	public SaveManager saveManager;

	void Awake()
	{
		Load();
	}

	public void Load()
	{
		saveManager.StartCoroutine(saveManager.ReadSaveData(() => saveManager.LoadSaveData(saveManager.loadedData)));
	}
}
