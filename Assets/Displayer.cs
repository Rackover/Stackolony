using UnityEngine;
using UnityEngine.UI;

public enum View{ Front, Plundge, Top }

public class Displayer : MonoBehaviour 
{
	public Camera cam;

	[HideInInspector] public bool available;
	RawImage feed;
	GameObject model;
	RenderTexture cTexture;

	public Texture2D GetRender(int width = 256, int height = 256)
	{
		cam.enabled = true;

		RenderTexture renderTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
		cam.targetTexture = renderTexture;
		RenderTexture.active = cam.targetTexture;
		cam.Render();

		Texture2D image = new Texture2D(width, height);
		image.ReadPixels( new Rect(0, 0, width, height), 0, 0);
		image.Apply();

		Unstage();
		return image;
	}
	
	public void Stage(GameObject _model, RawImage _feed, int quality)
	{
		cam.enabled = true;

		model = Instantiate(_model, transform.position, Quaternion.identity, transform);
		feed = _feed;
		cTexture = new RenderTexture(quality, quality, 16, RenderTextureFormat.ARGB32);
		feed.texture = cTexture;
		cam.targetTexture = cTexture;
		available = false;
	}

	public void Unstage()
	{
		cam.targetTexture = null;
		cam.enabled = false;
		Destroy(model);
		Destroy(cTexture);
		available = true;
	}

	void Update()
	{
		if(model != null)
			model.transform.Rotate(Vector3.up);
	}	
}
