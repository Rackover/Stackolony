using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler  {

    private TooltipGO tooltipGO; //Correspond à un script lié au gameObject de tooltip
    public string tooltipText;
    private bool isActive;

    void Awake()
    {
        isActive = false;
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (tooltipGO == null)
        {
            tooltipGO = FindObjectOfType<TooltipGO>();
        }
        tooltipGO.transform.position = pointerEventData.position;
        tooltipGO.text = tooltipText;
        tooltipGO.SetText();
        isActive = true;
    }

    public void Update()
    {
        if (isActive)
        {
            tooltipGO.transform.position = Input.mousePosition + new Vector3(-10,-10,0) ;
            tooltipGO.SetText();
        }
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        tooltipGO.transform.position = new Vector3(-1000, -1000, -1000);
        isActive = false;
    }
}
