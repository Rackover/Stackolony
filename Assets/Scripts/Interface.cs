using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Interface : MonoBehaviour {

	[Header("=== Cursor ===")][Space(1)]
	public RectTransform cursorTransform;
	public Image cursorIcon;

	public Sprite defaultCursor;
	public Sprite destroyCursor;
	public Sprite buildCursor;
	public Sprite bridgeCursor;

	[Header("=== General ===")][Space(1)]
	public Text txtMode;

	[Space(1)][Header("=== Error Message ===")][Space(1)]
	public Text txtError;
	public RectTransform backgroundError;

    [Header("=== References ===")]
    [Space(1)]
    public SFXManager sfxManager;

	public void ShowError(string message)
	{
        sfxManager.PlaySound("Error");
		backgroundError.gameObject.SetActive(true);
		backgroundError.sizeDelta = new Vector2(15f * message.Length, backgroundError.sizeDelta.y);
		this.gameObject.SetActive(true);
		txtError.text = message;
		StartCoroutine(WaitAndPrint(0.1f * message.Length));
	}

	IEnumerator WaitAndPrint(float delay)
    {
		// HELLO
        // suspend execution for 5 seconds
        yield return new WaitForSeconds(delay);
		backgroundError.gameObject.SetActive(false);
    }

	public void ChangeCursor(string mode)
	{
		cursorIcon.enabled = true;
		switch (mode) {
		case "Default":
			cursorIcon.sprite = null;
			cursorIcon.enabled = false;
			break;

		case "Build":
			cursorIcon.sprite = buildCursor;
			break;

		case "Delete":
			cursorIcon.sprite = destroyCursor;
			break;

		case "Bridge":
			cursorIcon.sprite = bridgeCursor;
			break;
	}
	}
}
