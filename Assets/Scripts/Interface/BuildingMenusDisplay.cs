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
            // Create game object
            GameObject mO = Instantiate(menuExample, transform);

            // Assignate type icon
            mO.GetComponentInChildren<Button>().gameObject.GetComponentsInChildren<Image>()[1].sprite = GameManager.instance.library.buildingsIcons[(int)menu.Key];

            // Menu rectangle and xample transform
            RectTransform mRt = mO.GetComponent<RectTransform>();
            RectTransform xRt = menuExample.GetComponent<RectTransform>();
            mRt.position = new Vector3(
                xRt.position.x + offset*factor,
                xRt.position.y,
                xRt.position.z
            );

            // Add localized line to build tooltip
            StartCoroutine(AddBuildMenuTooltips(mO, menu.Key.ToString()));

            // Destroying examples from the fitter
            GameObject content = mO.GetComponentInChildren<ContentSizeFitter>().gameObject;
            for (int i = 0; i < content.transform.childCount; i++) {
                Destroy(content.transform.GetChild(i).gameObject);
            }

            // Creating each building menu item
            foreach (BlockScheme scheme in menu.Value) {

                GameObject item = Instantiate(itemExample, content.transform).transform.GetChild(0).gameObject;
                item.name = scheme.ID.ToString();
                item.GetComponent<BuildingMenuItem>().blockId = scheme.ID;
                item.GetComponent<BuildingMenuItem>().parentScrollRect = mO.GetComponentInChildren<ScrollRect>();

                Tooltip tt = item.GetComponent<Tooltip>();
                StartCoroutine(AddTooltipsWhenPossible(tt, scheme));
            }

            mO.transform.GetChild(0).gameObject.SetActive(false);

            offset += xOffset;
        }
    }

    IEnumerator AddBuildMenuTooltips(GameObject mO, string buildingType)
    {
        yield return new WaitUntil(delegate{ return GameManager.instance.localization.GetLanguages().Count > 0; });
        mO.GetComponent<BuildingMenuDisplay>().button.GetComponent<Tooltip>().AddLocalizedLine(
            new Localization.Line("hud", "build" + buildingType)
        );
        yield return true;
    }

    IEnumerator AddTooltipsWhenPossible(Tooltip tt, BlockScheme scheme)
    {
        yield return new WaitUntil(delegate { return GameManager.instance.localization.GetLanguages().Count > 0; });
        List<Tooltip.Entry> entries = Tooltip.GetBuildingTooltip(scheme);
        foreach (Tooltip.Entry entry in entries) {
            tt.AddLocalizedLine(entry);
        }
        yield return true;
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
