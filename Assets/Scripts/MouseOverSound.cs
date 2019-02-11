using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverSound : MonoBehaviour, IPointerEnterHandler
{
	public string sound;

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameManager.instance.soundManager.Play(sound);
    }
}
