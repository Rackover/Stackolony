using System.Collections;
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

            RectTransform mRt = mO.GetComponent<RectTransform>();
            RectTransform xRt = menuExample.GetComponent<RectTransform>();
            mRt.position = new Vector3(
                xRt.position.x + offset*factor,
                xRt.position.y,
                xRt.position.z
            );

            mO.GetComponent<BuildingMenuDisplay>().button.GetComponent<Tooltip>().AddLocalizedLine(
                new Localization.Line("hud", "build"+menu.Key.ToString())    
            );

            GameObject content = mO.GetComponentInChildren<ContentSizeFitter>().gameObject;

            for (int i = 0; i < content.transform.childCount; i++) {
                Destroy(content.transform.GetChild(i).gameObject);
            }

            foreach (BlockScheme block in menu.Value) {
                GameObject item = Instantiate(itemExample, content.transform).transform.GetChild(0).gameObject;
                item.name = block.ID.ToString();
                item.GetComponent<BuildingMenuItem>().blockId = block.ID;
                item.GetComponent<BuildingMenuItem>().parentScrollRect = mO.GetComponentInChildren<ScrollRect>();

                Tooltip tt = item.GetComponent<Tooltip>();
                List<Tooltip.TooltipLocalizationEntry> entries = Tooltip.GetBuildingTooltip(block);
                foreach (Tooltip.TooltipLocalizationEntry entry in entries) {
                    tt.AddLocalizedLine(entry);
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
