using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Interface : MonoBehaviour {

	[Header("Cursor")][Space(1)]
	public RectTransform cursorTransform;
	public Image cursorImage;

	[Space(1)][Header("Option")]
	public Slider userBorderSensibility;
	public Slider userRotationSensibility;
	public Slider userGrabSensitivity;

	[Space(1)][Header("Error")]
	public Text txtError;
	public RectTransform backgroundError;
	
	SFXManager sfx;
	Library lib;

	void Awake()
	{
		lib = FindObjectOfType<Library>();
		sfx = FindObjectOfType<SFXManager>();
	}

	#region ErrorMessage
	public void ShowError(string message)
	{
        sfx.PlaySoundWithRandomParameters("Error",1,1,0.8f,1.2f);

		backgroundError.gameObject.SetActive(true);
		backgroundError.sizeDelta = new Vector2(15f * message.Length, backgroundError.sizeDelta.y);
		this.gameObject.SetActive(true);
		txtError.text = message;
		StartCoroutine(WaitAndPrint(0.1f * message.Length));
	}
	IEnumerator WaitAndPrint(float delay)
    {
        yield return new WaitForSeconds(delay);
		backgroundError.gameObject.SetActive(false);
    }
	#endregion

	public void ChangeCursor(string mode)	
	{
		cursorImage.enabled = true;
		switch (mode) 
		{
			case "Default":
				cursorImage.sprite = null;
				cursorImage.enabled = false;
				break;

			case "Build":
				cursorImage.sprite = lib.buildIcon;
				break;

			case "Delete":
				cursorImage.sprite = lib.destroyIcon;
				break;

			case "Bridge":
				cursorImage.sprite = lib.bridgeIcon;
				break;
		}
	}
}