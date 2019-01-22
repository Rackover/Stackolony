using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockVisual : MonoBehaviour 
{
	Animator animator;
	GameObject visuals;

	public void NewVisual(GameObject obj)
	{
		visuals = Instantiate(obj, transform.position, Quaternion.identity, transform);
		visuals.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
		visuals.name = "Model";
	}

	public void Hide()
	{
		visuals.SetActive(false);
	}

	public void Show()
	{
		visuals.SetActive(true);
	}

	
}

