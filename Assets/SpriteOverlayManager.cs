﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteOverlayManager : MonoBehaviour 
{
	public RectTransform canvas;
	
	[Header("Settings")]
	public float minSize = 10f;
	public float maxSize = 100f;
	[Header("Debug")]
	public Transform[] targets;
	public Sprite[] sprites;
	List<int> signIds = new List<int>();

	public class Sign
	{
		public int id;
		public Image image;
		public RectTransform self;
		public bool available;
	}
	
	List<Sign> signs = new List<Sign>();

	void Start()
	{
		for(int i = 0; i < targets.Length; i++)
		{
			signIds.Add(NewSignOverlay(sprites[i], targets[i].position));
		}
	}

	void Update()
	{
		for(int i = 0; i < signIds.Count; i++)
		{
			UpdateSign(signIds[i], targets[i].position);
		}
	}

	public void UpdateSign(int id, Vector3 pos)
	{
		Vector3 newPosition = Camera.main.WorldToScreenPoint(pos);

		if(newPosition.z > 0)
		{
			signs[id].self.position = newPosition;
			float size = GetSize(pos);
			signs[id].self.sizeDelta = new Vector2(size, size);
		}
	}

	float GetSize(Vector3 pos)
	{
		return 	1/Vector3.Distance(pos, Camera.main.transform.position) * maxSize;
	}


	public int NewSignOverlay(Sprite img, Vector3 pos)
	{
		Sign sign = GetSign();

		sign.image.sprite = img;
		sign.self.position = Camera.main.WorldToScreenPoint(pos);

		float size = GetSize(pos);
		sign.self.sizeDelta = new Vector2(size, size);

		return sign.id;
	}

	public Sign GetSign()
	{
		foreach(Sign s in signs){ if(s.available) return s; }

		GameObject go = new GameObject();
		go.transform.SetParent(canvas.transform);

		Sign sign = new Sign();
		sign.self = go.AddComponent<RectTransform>();
		sign.image = go.AddComponent<Image>();
		sign.id = signs.Count;
		sign.self.name = "Sign_" + sign.id;
		signs.Add(sign);

		return sign;
	}
}
