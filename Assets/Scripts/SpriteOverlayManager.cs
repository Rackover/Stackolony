
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteOverlayManager : MonoBehaviour 
{
	[Header("Settings")]
	public float maxSize = 100f;
	public Sprite unpoweredSprite;
	List<int> signIds = new List<int>();

	public class Sign
	{
		public int id;
		public Image image;
		public RectTransform self;
		public bool available = true;
	}
	
	List<Sign> signs = new List<Sign>();

	void Update()
	{	
		foreach(Sign s in signs){ KillSign(s.id); }
		foreach(Block b in GameManager.instance.systemManager.AllBlocks)
		{
			if(b.states.ContainsKey(State.Unpowered)) NewSignOverlay(unpoweredSprite, b.gameObject.transform.position);
		}
	}

	public void KillSign(int id)
	{
		signs[id].image.enabled = false;
		signs[id].image.sprite = null;
		signs[id].available = true;
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

		sign.image.enabled = true;
		sign.image.sprite = img;
		sign.self.position = Camera.main.WorldToScreenPoint(pos);

		float size = GetSize(pos);
		sign.self.sizeDelta = new Vector2(size, size);

		sign.available = false;
		return sign.id;
	}

	public Sign GetSign()
	{
		foreach(Sign s in signs){ if(s.available) return s; }

		GameObject go = new GameObject();
		go.transform.SetParent(transform);

		Sign sign = new Sign();
		sign.self = go.AddComponent<RectTransform>();
		sign.image = go.AddComponent<Image>();
		sign.image.raycastTarget = false;
		sign.id = signs.Count;
		sign.self.name = "Sign_" + sign.id;
		signs.Add(sign);

		return sign;
	}
}
