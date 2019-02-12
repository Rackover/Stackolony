﻿using UnityEngine;
using UnityEngine.UI;

public class CursorDisplay : MonoBehaviour {

	public RectTransform cursorTransform;
	public Image cursorImage;
    private Image imageComponent;

    private Vector2 defaultSize;
    private RectTransform rectTransform;

    Notifications notifier;

    private void Start()
    {
        GameManager.instance.cursorManagement.CursorError +=  (x) => DisplayUserError(x);
        imageComponent = GetComponent<Image>();
        notifier = FindObjectOfType<Notifications>();
        rectTransform = GetComponent<RectTransform>();
        defaultSize = rectTransform.sizeDelta;
    }

    private void Update()
    {
        Cursor.visible = false;
        cursorTransform.position = Input.mousePosition;
        ChangeCursor(GameManager.instance.cursorManagement.selectedMode);
        transform.SetSiblingIndex(transform.parent.childCount);
    }

    public void SetIcon(Sprite icon, float sizeCoef) 
    {
        imageComponent.sprite = icon;
        rectTransform.sizeDelta = defaultSize * sizeCoef;
        cursorImage.gameObject.SetActive(false);
    }

    public void ResetIcon()
    {
        imageComponent.sprite = GameManager.instance.library.cursorSprite;
        rectTransform.sizeDelta = defaultSize;
        cursorImage.gameObject.SetActive(true);
    }
    
    void DisplayUserError(string locId)
    {
        if (notifier == null) {
            notifier = FindObjectOfType<Notifications>();
        }
        notifier.Notify(new Notifications.Notification(locId, Color.red));
        GameManager.instance.soundManager.Play("Error");
    }

    public void ChangeCursor(CursorManagement.cursorMode mode)	
	{
		cursorImage.enabled = true;
		switch (mode) 
		{
            default:
				cursorImage.sprite = null;
				cursorImage.enabled = false;
				break;

			case CursorManagement.cursorMode.Move:
                cursorImage.enabled = true;
                cursorImage.sprite = GameManager.instance.library.dragIcon;
				break;

            case CursorManagement.cursorMode.Bridge:
                cursorImage.enabled = true;
                cursorImage.sprite = GameManager.instance.library.bridgeIcon;
                break;

            case CursorManagement.cursorMode.Delete:
                cursorImage.enabled = true;
                cursorImage.sprite = GameManager.instance.library.destroyIcon;
				break;
		}
	}
}
