using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockVisual : MonoBehaviour 
{
	[HideInInspector] public Animator animator;
	GameObject visuals;

	public void Awake()
	{
		if(animator == null) animator = GetComponent<Animator>();
	}

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

