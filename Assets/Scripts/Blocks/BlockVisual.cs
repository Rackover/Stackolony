using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockVisual : MonoBehaviour 
{
	public Animator animator;
	GameObject visuals;

	public void NewVisual(GameObject obj)
	{
		if(visuals != null) Destroy(visuals);
		visuals = Instantiate(obj, transform.position, Quaternion.identity, transform);
		visuals.name = "Model";
	}

	public void Hide()
	{
		Debug.Log(visuals);
		visuals.SetActive(false);
	}

	public void Show()
	{
		Debug.Log(visuals);
		visuals.SetActive(true);
	}
}

