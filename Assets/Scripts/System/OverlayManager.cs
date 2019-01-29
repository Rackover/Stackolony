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
        string displayName { get; }
        Gradient color { get; set; }
        Color defaultColor { get; set; }
        Image sprite { get; set; }
        void SetBlocksColor();
    }

    public class Default : IOverlay
    {
        public string displayName
        {
            get { return "Default"; }
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
        public string displayName
        {
            get { return "Type"; }
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
                foreach (Flag flag in block.activeFlags)
                {
                    if (flag.GetType() == typeof(House))
                    {
                        blockMat.color = color.Evaluate(0f);
                    }
                    else if (flag.GetType() == typeof(Occupator))
                    {
                        blockMat.color = color.Evaluate(0.5f);
                    }
                    else if (flag.GetType() == typeof(FiremanStation) ||
                      flag.GetType() == typeof(Generator) ||
                      flag.GetType() == typeof(FoodProvider) ||
                      flag.GetType() == typeof(PoliceStation) ||
                      flag.GetType() == typeof(Repairer) ||
                      flag.GetType() == typeof(Spatioport))
                    {
                        blockMat.color = color.Evaluate(1f);
                    }
                    else
                    {
                        blockMat.color = defaultColor;
                    }
                }
            }
        }
    }

    class FireRisks : IOverlay
    {
        public string displayName
        {
            get { return "Fire Risks"; }
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
        public string displayName
        {
            get { return "Power distribution"; }
        }
        public Gradient color { get; set; }
        public Image sprite { get; set; }
        public Color defaultColor { get; set; }
        public void SetBlocksColor()
        {
            foreach (Block block in GameManager.instance.systemManager.AllBlocks)
            {
                Material blockMat = block.overlayVisuals.GetComponent<MeshRenderer>().material;
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
                else
                {
                    blockMat.color = defaultColor;
                }
                block.overlayVisuals.SetActive(true);
            }
        }
    }

    class Food : IOverlay
    {
        public string displayName
        {
            get { return "Food consumption"; }
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
        public string displayName
        {
            get { return "Habitation quality"; }
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
                    float mediumNotation = 0;
                    PopulationManager popManager = GameManager.instance.populationManager;
                    CityManager cityManager = GameManager.instance.cityManager;
                    for (int i = 0; i < popManager.populationTypeList.Length; i++)
                    {
                        mediumNotation += cityManager.GetHouseNotation(house, popManager.populationTypeList[i]);
                    }

                    mediumNotation = mediumNotation / popManager.populationTypeList.Length;
                    mediumNotation += -cityManager.houseNotation.notEnoughFood;
                    mediumNotation += -cityManager.houseNotation.noOccupations;
                    mediumNotation += -cityManager.houseNotation.wrongPopulationType;

                    if (mediumNotation < -5)
                    {
                        blockMat.color = color.Evaluate(0f);
                    }
                    else if (mediumNotation > 0)
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
        public string displayName
        {
            get { return "Population density"; }
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

    public void selectOverlay(OverlayType type)
    {
        overlays[type].SetBlocksColor();
        activeOverlay = type;
    }

    public void UpdateOverlay()
    {
        selectOverlay(activeOverlay);
    }

    private void Awake()
    {
        InitOverlays();
    }
}
