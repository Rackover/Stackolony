using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Interface : MonoBehaviour {

    [Header("Settings")][Space(1)]
    public float refreshRate;

	[Header("Cursor")][Space(1)]
	public RectTransform cursorTransform;
	public Image cursorImage;
    
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
				cursorImage.sprite = GameManager.instance.library.buildIcon;
				break;

			case "Delete":
				cursorImage.sprite = GameManager.instance.library.destroyIcon;
				break;

			case "Bridge":
				cursorImage.sprite = GameManager.instance.library.bridgeIcon;
				break;
		}
	}
}