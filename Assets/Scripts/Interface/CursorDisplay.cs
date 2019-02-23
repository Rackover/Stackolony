using UnityEngine;
using UnityEngine.UI;

public class CursorDisplay : MonoBehaviour {

    public Transform canvas;

    Image image;
    RectTransform rectTransform;
    Notifications notifier;

    private void Start()
    {
        GameManager.instance.cursorManagement.CursorError +=  (x) => DisplayUserError(x);
        image = GetComponent<Image>();
        notifier = FindObjectOfType<Notifications>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        Cursor.visible = false;
        rectTransform.position = Input.mousePosition;

        UpdateCursor(GameManager.instance.cursorManagement);
        canvas.SetAsLastSibling();
    }
    

    void DisplayUserError(string locId)
    {
        if (notifier == null) {
            notifier = FindObjectOfType<Notifications>();
        }
        notifier.Notify(new Notifications.Notification(locId, Color.red));
        GameManager.instance.soundManager.Play("Error");
    }

    public void UpdateCursor(CursorManagement cursorMan)	
	{
        transform.SetAsLastSibling();
        Library lib = GameManager.instance.library;

        // Default cursor
        image.sprite = lib.cursorIcon;

        // State machine to determine cursor appareance
        if (!GameManager.instance.IsInGame()) {
            image.sprite = lib.cursorIcon;
            return;
        }

        // Bridge
        if (cursorMan.isBridging) {
            image.sprite = lib.bridgeIcon;
        }
        
        // Drag
        if (cursorMan.couldDrag) {
            image.sprite = lib.couldDragIcon;
        }
        if (cursorMan.isDragging) {
            image.sprite = lib.dragIcon;
        }
        
	}
}
