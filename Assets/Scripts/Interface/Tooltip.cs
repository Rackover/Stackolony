using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler  {

    
    public List<PublicLocalization> localizations = new List<PublicLocalization>();

    [System.Serializable]
    public class PublicLocalization
    {
        public string id;
        public string category;
    }

    [System.Serializable]
    public class TooltipLocalizationEntry
    {
        public string id;
        public string category;
        public string[] values;
        public tooltipType type;

        public TooltipLocalizationEntry(string _id, string _category, tooltipType _type, params string[] _values)
        {
            id = _id;
            category = _category;
            type = _type;
            values = _values;
        }
    }

    public enum tooltipType { Neutral, Positive, Negative};
    public bool isFirstLineBold = true;
    public List<Color> colors = new List<Color> { Color.black, Color.Lerp(Color.green, Color.black, 0.5f), Color.Lerp(Color.red, Color.black, 0.5f) };

    TooltipGO tooltipGO; //Correspond à un script lié au gameObject de tooltip
    List<TooltipLocalizationEntry> locs = new List<TooltipLocalizationEntry>();
    bool isActive;

    void Awake()
    {
        isActive = false;
        foreach(PublicLocalization pubLoc in localizations) {
            AddLocalizedLine(
                new TooltipLocalizationEntry(
                    pubLoc.id, pubLoc.category, tooltipType.Neutral
                )
            );
        }
    }

    public void ClearLines()
    {
        locs.Clear();
    }

    public void AddLocalizedLine(TooltipLocalizationEntry line)
    {
        locs.Add(line);
    }

    public void AddLocalizedLine(Localization.Line line, tooltipType type=tooltipType.Neutral)
    {
        locs.Add(new TooltipLocalizationEntry(line.id, line.category, type, line.values));
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
            txt += "<color=#"+ ColorUtility.ToHtmlStringRGB(colors[(int)locs[i].type])+ ">"+GameManager.instance.localization.GetLine(locs[i].id, locs[i].values)+"</color>";

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
