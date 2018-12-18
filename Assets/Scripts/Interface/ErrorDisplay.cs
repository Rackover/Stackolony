using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorDisplay : MonoBehaviour
{
	public RectTransform backgroundError;
	public Text txtError;

	public void ShowError(string message)
	{
        GameManager.instance.sfxManager.PlaySoundWithRandomParameters("Error",1,1,0.8f,1.2f);
		
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
}
