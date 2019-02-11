using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum OverlayType { Default, Type, FireRisks, Power, Food, Habitation, Density };

public class OverlayManager : MonoBehaviour
{
    //PUBLIC DATAS
    public Color defaultColor;
    public List<OverlayDatas> overlayDatas;
    public Dictionary<OverlayType, IOverlay> overlays = new Dictionary<OverlayType, IOverlay>();
    public OverlayType activeOverlay;

    [Header("Color settings")]
    public Color OverlaySelectedColor;
    public Color OverlayUnselectedColor;

    [System.Serializable]
    public class OverlayDatas
    {
        public Sprite sprite;
        public Gradient color;
    }

    //PRIvATE DATAS
    public interface IOverlay
    {
        string codeName { get; }
        Gradient color { get; set; }
        Color defaultColor { get; set; }
        Sprite sprite { get; set; }
        void SetBlocksColor();
    }

    public class Default : IOverlay
    {
        public string codeName
        {
            get { return "default"; }
        }
        public Gradient color { get; set; }
        public Sprite sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                block.overlayVisuals.Deactivate();
            }
        }
    }

    class Type : IOverlay
    {
        public string codeName
        {
            get { return "type"; }
        }
        public Gradient color { get; set; }
        public Sprite sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks) {

                Color chosenColor = defaultColor;

                foreach (Flag flag in block.activeFlags)
                {
                    if (FlagReader.GetCategory(block.scheme) == CityManager.BuildingType.Habitation)
                    {
                        chosenColor = color.Evaluate(0f);
                    }
                    else if (FlagReader.GetCategory(block.scheme) == CityManager.BuildingType.Occupators)
                    {
                        chosenColor = color.Evaluate(0.5f);
                    }
                    else if (FlagReader.GetCategory(block.scheme) == CityManager.BuildingType.Services)
                    {
                        chosenColor = color.Evaluate(1f);
                    }
                }

                block.overlayVisuals.Activate(chosenColor);
            }
        }
    }

    // TODO : IMPLEMENT FIRE RISKS
    class FireRisks : IOverlay
    {
        public string codeName
        {
            get { return "fireRisk"; }
        }
        public Gradient color { get; set; }
        public Sprite sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                block.overlayVisuals.Activate(defaultColor);
            }
        }
    }

    class Power : IOverlay
    {
        public string codeName
        {
            get { return "power"; }
        }
        public Gradient color { get; set; }
        public Sprite sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                Color chosenColor = defaultColor;

                if (block.GetConsumption() > 0)
                {
                    if (block.currentPower <= 0)
                    {
                        chosenColor = color.Evaluate(0f);
                    }
                    else if (block.currentPower >= block.GetConsumption())
                    {
                        chosenColor = color.Evaluate(1f);
                    }
                    else
                    {
                        chosenColor = color.Evaluate(0.5f);
                    }
                }
                block.overlayVisuals.Activate(chosenColor);
            }
        }
    }

    class Food : IOverlay
    {
        public string codeName
        {
            get { return "foodConsumption"; }
        }
        public Gradient color { get; set; }
        public Sprite sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                House house = block.GetComponent<House>();
                Color chosenColor = defaultColor;

                if (house != null)
                {
                    house.UpdateHouseInformations();
                    if (house.foodReceived <= 0 && house.foodConsumption > 0)
                    {
                        chosenColor = color.Evaluate(0f);
                    }
                    else if (house.foodConsumption >= house.foodReceived)
                    {
                        chosenColor = color.Evaluate(1f);
                    }
                    else if (house.foodConsumption < house.foodReceived)
                    {
                        chosenColor = color.Evaluate(0.5f);
                    }
                }
                block.overlayVisuals.Activate(chosenColor);
            }
        }
    }

    class Habitation : IOverlay
    {
        public string codeName
        {
            get { return "habitation"; }
        }
        public Gradient color { get; set; }
        public Sprite sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                House house = block.GetComponent<House>();
                Color chosenColor = defaultColor;
                if (house != null)
                {
                    house.UpdateHouseInformations();
                    float averageNotation = 0;
                    PopulationManager popManager = GameManager.instance.populationManager;
                    CityManager cityManager = GameManager.instance.cityManager;
                    for (int i = 0; i < popManager.populationTypeList.Length; i++)
                    {
                        averageNotation += cityManager.GetHouseNotation(house, popManager.populationTypeList[i]);
                    }

                    averageNotation = averageNotation / popManager.populationTypeList.Length;
                    averageNotation -= cityManager.moodValues.noOccupations;
                    averageNotation -= cityManager.moodValues.wrongPopulationType;

                    if (averageNotation < cityManager.moodValues.badNotationTreshold)
                    {
                        chosenColor = color.Evaluate(0f);
                    }
                    else if (averageNotation > cityManager.moodValues.goodNotationTreshold)
                    {
                        chosenColor = color.Evaluate(1f);
                    }
                    else
                    {
                        chosenColor = color.Evaluate(0.5f);
                    }
                }
                block.overlayVisuals.Activate(chosenColor);
            }
        }
    }

    class Density : IOverlay
    {
        public string codeName
        {
            get { return "density"; }
        }
        public Gradient color { get; set; }
        public Sprite sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                House house = block.GetComponent<House>();
                Color chosenColor = defaultColor;
                if (house != null)
                {
                    house.UpdateHouseInformations();
                    if (house.affectedCitizen.Count == 0)
                    {
                        chosenColor = color.Evaluate(0f);
                    }
                    else if (house.affectedCitizen.Count >= house.slotAmount)
                    {
                        chosenColor = color.Evaluate(1f);
                    }
                    else
                    {
                        chosenColor = color.Evaluate(0.5f);
                    }
                }
                block.overlayVisuals.Activate(chosenColor);
            }
        }
    }

    void InitOverlays()
    {
        overlays[OverlayType.Type] = new Type();
        overlays[OverlayType.FireRisks] = new FireRisks();
        overlays[OverlayType.Power] = new Power();
        overlays[OverlayType.Food] = new Food();
        overlays[OverlayType.Habitation] = new Habitation();
        overlays[OverlayType.Density] = new Density();
        overlays[OverlayType.Default] = new Default();

        LoadOverlaysDatas();
    }

    void LoadOverlaysDatas()
    {
        for (int i = 0; i < overlayDatas.Count; i++)
        {
            overlays[(OverlayType)i].color = overlayDatas[i].color;
            overlays[(OverlayType)i].sprite = overlayDatas[i].sprite;
            overlays[(OverlayType)i].defaultColor = defaultColor;
        }
    }

    public void SelectOverlay(OverlayType type)
    {
        overlays[type].SetBlocksColor();
        activeOverlay = type;
    }

    public void UpdateOverlay()
    {
        SelectOverlay(activeOverlay);
    }

    private void Awake()
    {
        InitOverlays();
    }
}
