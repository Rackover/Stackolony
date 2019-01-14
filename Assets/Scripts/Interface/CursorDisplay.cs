using UnityEngine;
using UnityEngine.UI;

public class CursorDisplay : MonoBehaviour {

	public RectTransform cursorTransform;
	public Image cursorImage;

    private void Awake()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
        cursorTransform.position = Input.mousePosition;
        ChangeCursor(GameManager.instance.cursorManagement.selectedMode.ToString());
        transform.SetSiblingIndex(transform.parent.childCount);
    }

    public void ChangeCursor(string mode)	
	{
		cursorImage.enabled = true;
		switch (mode) 
		{
            default:
				cursorImage.sprite = null;
				cursorImage.enabled = false;
				break;

			case "Default":
                cursorImage.enabled = true;
                cursorImage.sprite = GameManager.instance.library.dragIcon;
				break;

            case "Drag":
                cursorImage.enabled = true;
                cursorImage.sprite = GameManager.instance.library.bridgeIcon;
                break;

            case "Delete":
                cursorImage.enabled = true;
                cursorImage.sprite = GameManager.instance.library.destroyIcon;
				break;

			case "Bridge":
                cursorImage.enabled = true;
                cursorImage.sprite = GameManager.instance.library.bridgeIcon;
				break;
		}
	}
}
