using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler  {


    public bool isFirstLineBold = true;

    private TooltipGO tooltipGO; //Correspond à un script lié au gameObject de tooltip
    List<Localization.Line> locs = new List<Localization.Line>();
    private bool isActive;

    void Awake()
    {
        isActive = false;
    }

    public void AddLocalizedLine(Localization.Line line)
    {
        locs.Add(line);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (tooltipGO == null)
        {
            tooltipGO = FindObjectOfType<TooltipGO>();
        }
        tooltipGO.transform.position = pointerEventData.position + tooltipGO.shift;

        string txt = "";
        for (int i = 0; i < locs.Count; i++) {

            if (i > 0) {
                txt += "\n";
            }

            GameManager.instance.localization.SetCategory(locs[i].category);
            txt += GameManager.instance.localization.GetLine(locs[i].id);

            if (i == 0 && isFirstLineBold) {
                txt = "<b>" + txt + "</b>";
            }
        }

        tooltipGO.SetText(txt);
        isActive = true;
    }

    public void Update()
    {
        if (isActive)
        {
            tooltipGO.transform.position = Input.mousePosition + new Vector3(tooltipGO.shift.x, tooltipGO.shift.y, 0);
        }
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        tooltipGO.transform.position = new Vector3(-1000, -1000, -1000);
        isActive = false;
    }
}
