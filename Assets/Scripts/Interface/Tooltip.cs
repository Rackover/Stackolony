using System;
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
    public static readonly State[] negativeStates = { State.OnFire, State.OnRiot, State.Damaged, State.Unpowered };
    public bool isFirstLineBold = true;
    public bool disableIfCursorOnUI = false;
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
    
    void OnMouseEnter()
    {
        if (disableIfCursorOnUI && GameManager.instance.cursorManagement.cursorOnUI) return;
        OnPointerEnter(new PointerEventData(FindObjectOfType<EventSystem>()));
    }

    void OnMouseExit()
    {
        if (disableIfCursorOnUI && GameManager.instance.cursorManagement.cursorOnUI) return;
        OnPointerExit(new PointerEventData(FindObjectOfType<EventSystem>()));
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
        if (isActive) {
            tooltipGO.Enable();
            tooltipGO.transform.position = Input.mousePosition + new Vector3(tooltipGO.shift.x, tooltipGO.shift.y, 0);
            tooltipGO.UpdateTooltipSizeAndPosition();
        }
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (tooltipGO == null) {
            tooltipGO = FindObjectOfType<TooltipGO>();
        }
        tooltipGO.Disable();
        isActive = false;
    }

    public static List<TooltipLocalizationEntry> GetBuildingTooltip(Block block)
    {
        List<TooltipLocalizationEntry> entries = new List<TooltipLocalizationEntry>();

        foreach(KeyValuePair< State, StateBehavior> state in block.states) {
            entries.Add(
                new TooltipLocalizationEntry(
                    state.Key.ToString(),
                    "state",
                    new List<State>(negativeStates).Contains(state.Key) ? tooltipType.Negative : tooltipType.Neutral
                )
            );
        }

        return GetBuildingTooltip(block.scheme, entries);
    }
    
    public static List<TooltipLocalizationEntry> GetBuildingTooltip(BlockScheme scheme, List<TooltipLocalizationEntry> _entries = null)
    {
        List<TooltipLocalizationEntry> entries = new List<TooltipLocalizationEntry>();

        entries.Add(new TooltipLocalizationEntry("block" + scheme.ID.ToString(), "blockName", tooltipType.Neutral));
        entries.Add(new TooltipLocalizationEntry("block" + scheme.ID.ToString(), "blockDescription", tooltipType.Neutral));

        // Porting previous block tooltip entries to this list
        if (_entries != null) {
            foreach(TooltipLocalizationEntry tte in _entries) {
                entries.Add(tte);
            }
        }

        // Flag reading to get the block bonuses and maluses
        foreach (List<string> flag in FlagReader.GetFlags(scheme)) {
            string name = flag[0];
            flag.Remove(name);
            string[] parameters = flag.ToArray();

            for (int i = 0; i < parameters.Length; i++) {
                string popInfo = "";
                bool wasPop = false;
                foreach (string popName in parameters[i].Split('-')) {
                    if (GameManager.instance.populationManager.GetPopulationByCodename(popName) != null) {
                        if (popInfo.Length > 0) {
                            popInfo += " " + GameManager.instance.localization.GetLineFromCategory("stats", "or") + " ";
                        }
                        popInfo += GameManager.instance.localization.GetLineFromCategory("populationType", popName);
                        wasPop = true;
                    }
                }
                if (wasPop) {
                    parameters[i] = popInfo;
                }
            }

            entries.Add( new TooltipLocalizationEntry(
                name.ToLower(), "flagParameter", FlagReader.IsPositive(name) ? tooltipType.Positive : tooltipType.Negative, parameters
            ));
        }

        return entries;
    }
}
