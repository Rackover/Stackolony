using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayerManager : MonoBehaviour 
{
	public GameObject displayer;
	List<Displayer> displayers = new List<Displayer>();
/*
	[Header("Test")]
	public GameObject[] objects;
	public RawImage[] images;

	void Start()
	{
		for(int i = 0; i < objects.Length; i++)
		{
			SetRotationFeed(objects[i], images[i]);
		}
	}

	[ContextMenu("DEBUG")]
	void DebugStage()
	{
		SetRotationFeed(objects[0], images[0]);
	}
*/
	public Displayer SetRotationFeed(GameObject _model, RawImage _feed, int _quality = 128)
	{
		Displayer d = GetDisplayer();
		d.Stage(_model, _feed, _quality);
		return d;
	}

	Displayer GetDisplayer()
	{
		foreach(Displayer d in displayers)
		{
			if(d.available)
				return d;
		}
		Vector3 newPosition = new Vector3(displayers.Count * 2, 0f, 0f);
		displayers.Add(Instantiate(displayer, newPosition, Quaternion.identity, transform).GetComponent<Displayer>());
		return displayers[displayers.Count - 1];
	}
}
