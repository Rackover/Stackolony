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

    [System.Serializable]
    public class OverlayDatas
    {
        public Image sprite;
        public Gradient color;
    }

    //PRIvATE DATAS
    public interface IOverlay
    {
        string codeName { get; }
        Gradient color { get; set; }
        Color defaultColor { get; set; }
        Image sprite { get; set; }
        void SetBlocksColor();
    }

    public class Default : IOverlay
    {
        public string codeName
        {
            get { return "default"; }
        }
        public Gradient color { get; set; }
        public Image sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                Material blockMat = block.overlayVisuals.GetComponent<MeshRenderer>().material;
                blockMat.color = defaultColor;
                block.overlayVisuals.SetActive(false);
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
        public Image sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                block.overlayVisuals.SetActive(true);
                Material blockMat = block.overlayVisuals.GetComponent<MeshRenderer>().material;
                blockMat.color = defaultColor;
                foreach (Flag flag in block.activeFlags)
                {
                    if (FlagReader.GetCategory(block.scheme) == CityManager.BuildingType.Habitation)
                    {
                        blockMat.color = color.Evaluate(0f);
                    }
                    else if (FlagReader.GetCategory(block.scheme) == CityManager.BuildingType.Occupators)
                    {
                        blockMat.color = color.Evaluate(0.5f);
                    }
                    else if (FlagReader.GetCategory(block.scheme) == CityManager.BuildingType.Services)
                    {
                        blockMat.color = color.Evaluate(1f);
                    }
                }
            }
        }
    }

    // TODO : IMPLEMENT FIRE RISKS
    class FireRisks : IOverlay
    {
        public string codeName
        {
            get { return "firerisk"; }
        }
        public Gradient color { get; set; }
        public Image sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                Material blockMat = block.overlayVisuals.GetComponent<MeshRenderer>().material;
                blockMat.color = defaultColor;
                block.overlayVisuals.SetActive(true);
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
        public Image sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                Material blockMat = block.overlayVisuals.GetComponent<MeshRenderer>().material;
                blockMat.color = defaultColor;
                if (block.scheme.consumption > 0)
                {
                    if (block.currentPower <= 0)
                    {
                        blockMat.color = color.Evaluate(0f);
                    }
                    else if (block.currentPower >= block.scheme.consumption)
                    {
                        blockMat.color = color.Evaluate(1f);
                    }
                    else
                    {
                        blockMat.color = color.Evaluate(0.5f);
                    }
                }
                block.overlayVisuals.SetActive(true);
            }
        }
    }

    class Food : IOverlay
    {
        public string codeName
        {
            get { return "foodconsumption"; }
        }
        public Gradient color { get; set; }
        public Image sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                House house = block.GetComponent<House>();
                Material blockMat = block.overlayVisuals.GetComponent<MeshRenderer>().material;
                blockMat.color = defaultColor;
                if (house != null)
                {
                    house.UpdateHouseInformations();
                    if (house.foodReceived <= 0 && house.foodConsumption > 0)
                    {
                        blockMat.color = color.Evaluate(0f);
                    }
                    else if (house.foodConsumption >= house.foodReceived)
                    {
                        blockMat.color = color.Evaluate(1f);
                    }
                    else if (house.foodConsumption < house.foodReceived)
                    {
                        blockMat.color = color.Evaluate(0.5f);
                    }
                }
                block.overlayVisuals.SetActive(true);
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
        public Image sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                House house = block.GetComponent<House>();
                Material blockMat = block.overlayVisuals.GetComponent<MeshRenderer>().material;
                blockMat.color = defaultColor;
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
                    averageNotation += -cityManager.houseNotation.notEnoughFood;
                    averageNotation += -cityManager.houseNotation.noOccupations;
                    averageNotation += -cityManager.houseNotation.wrongPopulationType;

                    if (averageNotation < cityManager.houseNotation.badNotationTreshold)
                    {
                        blockMat.color = color.Evaluate(0f);
                    }
                    else if (averageNotation > cityManager.houseNotation.goodNotationTreshold)
                    {
                        blockMat.color = color.Evaluate(1f);
                    }
                    else
                    {
                        blockMat.color = color.Evaluate(0.5f);
                    }
                }
                block.overlayVisuals.SetActive(true);
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
        public Image sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                House house = block.GetComponent<House>();
                Material blockMat = block.overlayVisuals.GetComponent<MeshRenderer>().material;
                blockMat.color = defaultColor;
                if (house != null)
                {
                    house.UpdateHouseInformations();
                    if (house.affectedCitizen.Count == 0)
                    {
                        blockMat.color = color.Evaluate(0f);
                    }
                    else if (house.affectedCitizen.Count >= house.slotAmount)
                    {
                        blockMat.color = color.Evaluate(1f);
                    }
                    else
                    {
                        blockMat.color = color.Evaluate(0.5f);
                    }
                }
                block.overlayVisuals.SetActive(true);
            }
        }
    }

    void InitOverlays()
    {
        overlays[OverlayType.Default] = new Default();
        overlays[OverlayType.Type] = new Type();
        overlays[OverlayType.FireRisks] = new FireRisks();
        overlays[OverlayType.Power] = new Power();
        overlays[OverlayType.Food] = new Food();
        overlays[OverlayType.Habitation] = new Habitation();
        overlays[OverlayType.Density] = new Density();

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
