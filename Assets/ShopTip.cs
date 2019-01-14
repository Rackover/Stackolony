using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler  
{
	public ShopDisplay shopDisplay;

    void Awake()
    {
		if(shopDisplay == null) shopDisplay = GetComponent<ShopDisplay>();
    }

    public void Update()
    {
		/*
        if (isActive)
        {
            tooltipGO.transform.position = Input.mousePosition + new Vector3(-10,-10,0) ;
            tooltipGO.SetText();
        }
		*/
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
		GameManager.instance.blockInfobox.ShowBlockSheme(shopDisplay.myBlock, shopDisplay.self.position);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
		GameManager.instance.blockInfobox.Hide();
    }
}
