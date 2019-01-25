using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrashButton : MonoBehaviour {


    Button button;
    Image image;
    bool hovered = false;

    private void Start()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    void Disable()
    {
        button.enabled = false;
        image.enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
    }

    void Enable()
    {
        button.enabled = true;
        image.enabled = true;
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void Hover()
    {
        hovered = true;
    }

    public void UnHover()
    {
        hovered = false;
    }

    public void TrashCurrentBuilding()
    {
        CursorManagement cursorMan = GameManager.instance.cursorManagement;

        if (cursorMan.isDragging && cursorMan.selectedBlock != null) {
            GameManager.instance.gridManagement.DestroyBlock(cursorMan.selectedBlock.gridCoordinates);
        }
    }

    private void Update()
    {
        Disable();
        if (GameManager.instance.cursorManagement.isDragging) {
            Enable();
        }

        if (hovered && Input.GetButtonUp("Select")) {
            TrashCurrentBuilding();
        }
    }
}
