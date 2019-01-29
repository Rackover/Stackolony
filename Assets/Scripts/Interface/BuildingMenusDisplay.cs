﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class BuildingMenusDisplay : MonoBehaviour {

    Dictionary<CityManager.BuildingType, List<BlockScheme>> categories = new Dictionary<CityManager.BuildingType, List<BlockScheme>>();

    public GameObject menuExample;
    public GameObject itemExample;
    public float xOffset = 20f;

	// Use this for initialization
	void Start () {
        FillCategories();
        SpawnMenus();
        Destroy(menuExample);
        Destroy(itemExample);
	}

    private void SpawnMenus()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        float factor = canvas.scaleFactor;
        float offset = 0;
        foreach (KeyValuePair<CityManager.BuildingType, List<BlockScheme>> menu in categories) {
            GameObject mO = Instantiate(menuExample, transform);
            mO.GetComponentInChildren<Button>().gameObject.GetComponentsInChildren<Image>()[1].sprite = GameManager.instance.library.buildingsIcons[(int)menu.Key];

            mO.GetComponent<RectTransform>().position = new Vector3(
                menuExample.GetComponent<RectTransform>().position.x + offset*factor,
                menuExample.GetComponent<RectTransform>().position.y,
                menuExample.GetComponent<RectTransform>().position.z
            );

            GameObject content = mO.GetComponentInChildren<ContentSizeFitter>().gameObject;

            for (int i = 0; i < content.transform.childCount; i++) {
                Destroy(content.transform.GetChild(i).gameObject);
            } 

            foreach(BlockScheme block in menu.Value) {
                GameObject item = Instantiate(itemExample, content.transform).transform.GetChild(0).gameObject;
                item.name = block.ID.ToString();
                item.GetComponent<BuildingMenuItem>().blockId = block.ID;

                Tooltip tt = item.GetComponent<Tooltip>();
                tt.AddLocalizedLine(new Localization.Line("blockName", "block" + block.ID.ToString()));
                tt.AddLocalizedLine(new Localization.Line("blockDescription", "block" + block.ID.ToString()));

                // Flag reading to get the block bonuses and maluses
                foreach (List<string> flag in FlagReader.GetFlags(block)) {
                    string name = flag[0];
                    flag.Remove(name);
                    string[] parameters = flag.ToArray();
                    tt.AddLocalizedLine(new Tooltip.TooltipLocalizationEntry(
                        name.ToLower(), "flagParameter", FlagReader.IsPositive(name) ? Tooltip.tooltipType.Positive : Tooltip.tooltipType.Negative, parameters
                    ));
                }
            }

            mO.transform.GetChild(0).gameObject.SetActive(false);

            offset += xOffset;
        }
    }

    private void FillCategories()
    {
        foreach (BlockScheme block in GameManager.instance.library.blocks) {
            if (!block.isBuyable) {
                continue;
            }

            CityManager.BuildingType category = FlagReader.GetCategory(block);

                if (!categories.ContainsKey(category)) categories[category] = new List<BlockScheme>();
                categories[category].Add(block);
        }
    }
}
