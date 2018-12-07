using UnityEngine;
using UnityEngine.UI;

public class CanvasLineRenderer : MonoBehaviour 
{
	RectTransform self;
	Image image;
	bool active;

	void Awake()
	{
		self = GetComponent<RectTransform>();
		image = GetComponent<Image>();
		if(image == null)
		{
			image = gameObject.AddComponent<Image>();
		}
	}

	public void DrawCanvasLine(Vector3 origin, Vector3 target, float _width, Color _color)
	{
		active = true;
		image.color = _color;
		
		Vector3 differenceVector = origin - target;
 
		self.sizeDelta = new Vector2(differenceVector.magnitude / image.canvas.scaleFactor, _width);
		self.pivot = new Vector2(0, 0.5f);
		self.position = origin;
		float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
		self.localRotation = Quaternion.Euler(0, 0, angle + 180f);
	}

	void Update()
	{
		if(!active)
			image.enabled = false;
		else
			image.enabled = true;

		active = false;
	}
}
