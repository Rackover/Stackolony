using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler  {

    private TooltipGO tooltipGO; //Correspond à un script lié au gameObject de tooltip
    public string tooltipText;

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (tooltipGO == null)
        {
            tooltipGO = FindObjectOfType<TooltipGO>();
        }
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
    }
}
