using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayerManager : MonoBehaviour 
{
	public GameObject displayer;
	List<Displayer> displayers = new List<Displayer>();

	public Displayer SetRotationFeed(GameObject newObject, RawImage image, float rotation = 0f, float speed = 0f, float camDistance = 3f, float camFOV = 30f,  int size = 64)
	{
		Displayer d = GetDisplayer();
		d.Stage(newObject, image, rotation, speed, camDistance, camFOV, size);
		return d;
	}

	Displayer GetDisplayer()
	{
		foreach(Displayer d in displayers)
		{
			if(d.available)
				return d;
		}
		Vector3 newPosition = new Vector3(displayers.Count * 3, 0f, 0f);
		displayers.Add(Instantiate(displayer, newPosition, Quaternion.identity, transform).GetComponent<Displayer>());
		return displayers[displayers.Count - 1];
	}

    public void UnstageAll()
    {
        foreach(Displayer displayer in displayers) {
            displayer.Unstage();
        }
    }
}
