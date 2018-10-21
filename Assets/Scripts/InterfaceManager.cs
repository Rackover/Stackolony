using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InterfaceManager : MonoBehaviour {

	[Header("=== General ===")][Space(1)]
	public Text txtMode;

	[Space(1)][Header("=== Error Message ===")][Space(1)]
	public Text txtError;
	public RectTransform backgroundError;



	public void ShowError(string message)
	{
		backgroundError.gameObject.SetActive(true);

		backgroundError.sizeDelta = new Vector2(15f * message.Length, backgroundError.sizeDelta.y);
		this.gameObject.SetActive(true);
		txtError.text = message;

		StartCoroutine(WaitAndPrint(0.1f * message.Length));
	}

	IEnumerator WaitAndPrint(float delay)
    {
        // suspend execution for 5 seconds
        yield return new WaitForSeconds(delay);
		backgroundError.gameObject.SetActive(false);
    }
}
