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

    [Serializable]
    public class Entry
    {
        public entryType entry;
        public string id;
        public string category;
        public string[] values;
        public string plaintext = "";
        public informationType type;
        public string[] formatters = new string[] { };

        public Entry(string _id, string _category, informationType _type, params string[] _values)
        {
            id = _id;
            category = _category;
            type = _type;
            values = _values;
            entry = entryType.Localization;
        }

        public Entry(entryType _entry, string text = "")
        {
            entry = _entry;
            if (entry == entryType.Plaintext) {
                plaintext = text;
            }
        }
    }

    public enum informationType { Neutral, Positive, Negative};
    public static readonly State[] negativeStates = { State.OnFire, State.OnRiot, State.Damaged, State.Unpowered };
    public bool isFirstLineBold = true;
    public bool disableIfCursorOnUI = false;
    public List<Color> colors = new List<Color> { Color.black, Color.Lerp(Color.green, Color.black, 0.5f), Color.Lerp(Color.red, Color.black, 0.5f) };

    public enum entryType { Localization, Plaintext, LineBreak};
    TooltipGO tooltipGO; //Correspond à un script lié au gameObject de tooltip
    List<Entry> locs = new List<Entry>();
    bool isActive;

    void Awake()
    {
        isActive = false;
        foreach(PublicLocalization pubLoc in localizations) {
            AddLocalizedLine(
                new Entry(
                    pubLoc.id, pubLoc.category, informationType.Neutral
                )
            );
        }
    }

    public void ClearLines()
    {
        locs.Clear();
    }

    public void AddLocalizedLine(Entry line)
    {
        locs.Add(line);
    }

    public void AddLocalizedLine(Localization.Line line, informationType type=informationType.Neutral)
    {
        locs.Add(new Entry(line.id, line.category, type, line.values));
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

            string newStr = "";

            switch (locs[i].entry) {
                case entryType.LineBreak: continue;
                case entryType.Plaintext: newStr = locs[i].plaintext; break;
                case entryType.Localization:
                    GameManager.instance.localization.SetCategory(locs[i].category);
                    newStr = "<color=#" + ColorUtility.ToHtmlStringRGB(colors[(int)locs[i].type]) + ">" + GameManager.instance.localization.GetLine(locs[i].id, locs[i].values) + "</color>";
                    break;
            }

            if (i == 0 && isFirstLineBold) {
                newStr = "<b>" + newStr + "</b>";
            }

            foreach(string formatter in locs[i].formatters) {
                newStr = "<"+formatter+">" + newStr + "</"+formatter+">";
            }

            txt += newStr;
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

    public static List<Entry> GetBuildingTooltip(Block block)
    {
        List<Entry> entries = new List<Entry>();

        foreach(KeyValuePair< State, StateBehavior> state in block.states) {
            entries.Add(
                new Entry(
                    state.Key.ToString(),
                    "state",
                    new List<State>(negativeStates).Contains(state.Key) ? informationType.Negative : informationType.Neutral
                )
            );
        }

        return GetBuildingTooltip(block.scheme, entries);
    }
    
    public static List<Entry> GetBuildingTooltip(BlockScheme scheme, List<Entry> _entries = null)
    {
        List<Entry> entries = new List<Entry>();

        entries.Add(new Entry("block" + scheme.ID.ToString(), "blockName", informationType.Neutral));
        entries.Add(new Entry("block" + scheme.ID.ToString(), "blockDescription", informationType.Neutral));
        entries.Add(new Entry(entryType.LineBreak));

        // Porting previous block tooltip entries to this list
        if (_entries != null) {
            foreach (Entry tte in _entries) {
                entries.Add(tte);
            }
            entries.Add(new Entry(entryType.LineBreak));
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
                            popInfo += " " + GameManager.instance.localization.GetLineFromCategory("stats", "orSeparator") + " ";
                        }
                        popInfo += GameManager.instance.localization.GetLineFromCategory("populationType", popName);
                        wasPop = true;
                    }
                }
                if (wasPop) {
                    parameters[i] = popInfo;
                }
            }

            entries.Add( new Entry(
                name.ToLower(), "flagParameter", FlagReader.IsPositive(name) ? informationType.Positive : informationType.Negative, parameters
            ));
        }

        // Conditional unlocking
        ConditionalUnlocks unlocker = GameManager.instance.cityManager.conditionalUnlocker;
        if (!unlocker.CanBeUnlocked(scheme.ID)) {

            entries.Add(new Entry(entryType.LineBreak));

            // Bold line
            entries.Add(new Entry(
                "toUnlockThisBuildingYouMust", "conditionalUnlock", informationType.Neutral
            ) { formatters = new string[1] { "b" } });

            // Condition (= <= >= < >)
            foreach (ScriptInterpreter.FormattedComparison condition in unlocker.GetFormattedUnlockConditions(scheme.ID)) {

                // If Int, no need to tranlate it

                entries.Add(new Entry(
                    condition.oprtr, "conditionalUnlock", informationType.Negative,
                        condition.lefthandData.GetLocalization(GameManager.instance.localization),
                        condition.righthandData.GetLocalization(GameManager.instance.localization)
                ));
            }
        }

        return entries;
    }
}
