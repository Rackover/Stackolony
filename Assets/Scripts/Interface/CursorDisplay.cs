using UnityEngine;
using UnityEngine.UI;

public class CursorDisplay : MonoBehaviour {

	public RectTransform cursorTransform;
	public Image cursorImage;

    Notifications notifier;

    private void Start()
    {
        GameManager.instance.cursorManagement.CursorError +=  (x) => DisplayUserError(x);
        notifier = FindObjectOfType<Notifications>();
    }

    private void Update()
    {
        Cursor.visible = false;
        cursorTransform.position = Input.mousePosition;
        ChangeCursor(GameManager.instance.cursorManagement.selectedMode);
        transform.SetSiblingIndex(transform.parent.childCount);
    }
    
    void DisplayUserError(string locId)
    {
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
