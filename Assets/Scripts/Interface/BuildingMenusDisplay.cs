using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class BuildingMenusDisplay : MonoBehaviour {

    enum BuildingTypes { Habitation=0, Services=1, Occupators=2};
    Dictionary<BuildingTypes, List<BlockScheme>> categories = new Dictionary<BuildingTypes, List<BlockScheme>>();

    public GameObject menuExample;
    public GameObject itemExample;
    public float xOffset = 20f;
    public List<Sprite> logos = new List<Sprite>();

	// Use this for initialization
	void Start () {
        FillCategories();
        SpawnMenus();
        Destroy(menuExample);
        Destroy(itemExample);
	}

    private void SpawnMenus()
    {
        float offset = 0;
        foreach (KeyValuePair<BuildingTypes, List<BlockScheme>> menu in categories) {
            GameObject mO = Instantiate(menuExample, transform);
            mO.GetComponentInChildren<Button>().gameObject.GetComponentsInChildren<Image>()[1].sprite = logos[(int)menu.Key];

            mO.GetComponent<RectTransform>().position = new Vector3(
                menuExample.GetComponent<RectTransform>().position.x + offset,
                menuExample.GetComponent<RectTransform>().position.y,
                menuExample.GetComponent<RectTransform>().position.z
            );

            GameObject content = mO.GetComponentInChildren<ContentSizeFitter>().gameObject;

            for (int i = 0; i < content.transform.childCount; i++) {
                Destroy(content.transform.GetChild(i).gameObject);
            } 

            foreach(BlockScheme block in menu.Value) {
                GameObject item = Instantiate(itemExample, content.transform);
                item.name = block.ID.ToString();
                item.GetComponent<BuildingMenuItem>().blockId = block.ID;

                Tooltip tt = item.GetComponent<Tooltip>();
                tt.AddLocalizedLine(new Localization.Line("blockName", "block" + block.ID.ToString()));
                tt.AddLocalizedLine(new Localization.Line("blockDescription", "block" + block.ID.ToString()));
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

            bool assigned = false;
            string[] rawFlags = FlagReader.GetRawFlags(block).ToArray();

            // Habitation
            if (rawFlags.Contains("House")) {
                
                if (!categories.ContainsKey(BuildingTypes.Habitation)) categories[BuildingTypes.Habitation] = new List<BlockScheme>();
                categories[BuildingTypes.Habitation].Add(block);
                assigned = true;
            }

            // Occupators
            if (rawFlags.Contains("Occupator")) {
                if (!categories.ContainsKey(BuildingTypes.Occupators)) categories[BuildingTypes.Occupators] = new List<BlockScheme>();
                categories[BuildingTypes.Occupators].Add(block);
                assigned = true;
            }

            // Services
            if (!assigned) {
                if (!categories.ContainsKey(BuildingTypes.Services)) categories[BuildingTypes.Services] = new List<BlockScheme>();
                categories[BuildingTypes.Services].Add(block);
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
