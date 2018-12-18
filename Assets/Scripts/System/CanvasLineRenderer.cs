using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Line
{
	public bool active;
	public RectTransform self;
	public Image image;
}

public class CanvasLineRenderer : MonoBehaviour 
{
	List<Line> lines = new List<Line>();

	public void DrawCanvasLine(Vector3 origin, Vector3 target, float _width, Color _color)
	{
		Line newLine = GetLine();
		
		newLine.active = true;
		newLine.image.color = _color;
		
		Vector3 differenceVector = origin - target;

		newLine.self.sizeDelta = new Vector2(differenceVector.magnitude / newLine.image.canvas.scaleFactor, _width);
		newLine.self.pivot = new Vector2(0, 0.5f);
		newLine.self.position = origin;
		float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
		newLine.self.localRotation = Quaternion.Euler(0, 0, angle + 180f);
	}

	Line GetLine()
	{
		for( int i = 0; i < lines.Count; i++)
		{
			if(!lines[i].active) return lines[i];
		}

		lines.Add(new Line());

		GameObject newLineObject = new GameObject();
		newLineObject.transform.SetParent(this.transform);
		newLineObject.name = "Line_" + (lines.Count - 1).ToString();

		lines[lines.Count - 1].self = newLineObject.AddComponent<RectTransform>();
		lines[lines.Count - 1].image = newLineObject.AddComponent<Image>();
		return lines[lines.Count - 1];
	}

	void Update()
	{
		foreach(Line l in lines)
		{
			if(!l.active) l.image.enabled = false;
			else l.image.enabled = true;

			l.active = false;
		}
	}
}
