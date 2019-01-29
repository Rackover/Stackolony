using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour 
{
	Button button;
	public string sound;

	void Awake()
	{
		button = GetComponent<Button>();
		if(button == null) Destroy(button);
	}

	void Start()
	{
		button.onClick.AddListener(PlaySound);
	}

	public void PlaySound()
	{
		GameManager.instance.soundManager.Play(sound);
	}
}
