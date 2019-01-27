using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayManager : MonoBehaviour
{

    [System.Serializable]
    public class Overlay
    {
        public int ID;
        public string name;
        [TextArea(3,5)]
        public string description;
        public Image icon;
        public Gradient color;
    }

    [Header("OverlayMode")]
    public List<Overlay> overlays = new List<Overlay>();
    public Color noDataColor;
    public int activeOverlay;


    private void Start()
    {
        SelectOverlayMode(activeOverlay);
    }

    public void UpdateOverlay()
    {
        SelectOverlayMode(activeOverlay);
    }

    //Overlay "0" means no overlay
    public void SelectOverlayMode(int ID)
    {
        foreach (Overlay overlay in overlays)
        {
            if (ID == overlay.ID)
            {
                activeOverlay = ID;
                foreach (Block block in GameManager.instance.systemManager.AllBlocks)
                {
                    if (ID != 0)
                    {
                        block.overlayVisuals.SetActive(true);
                    }
                    else
                    {
                        block.overlayVisuals.SetActive(false);
                    }
                    SetBlockColor(block, overlay);
                }
            }
        }
    }

    //Get a block and assign its color with the chosen overlay
    public void SetBlockColor(Block block, Overlay overlay)
    {
        Material blockMat = block.overlayVisuals.GetComponent<MeshRenderer>().material;
        House house = block.GetComponent<House>();
        switch (overlay.ID)
        {
            //OVERLAY "TYPE"
            case 1:
                foreach (Flag flag in block.activeFlags)
                {
                    if (flag.GetType() == typeof(House))
                    {
                        blockMat.color = overlay.color.Evaluate(0f);
                    }
                    else if (flag.GetType() == typeof(Occupator))
                    {
                        blockMat.color = overlay.color.Evaluate(0.5f);
                    }
                    else if (flag.GetType() == typeof(FiremanStation) ||
                      flag.GetType() == typeof(Generator) ||
                      flag.GetType() == typeof(FoodProvider) ||
                      flag.GetType() == typeof(PoliceStation) ||
                      flag.GetType() == typeof(Repairer) ||
                      flag.GetType() == typeof(Spatioport))
                    {
                        blockMat.color = overlay.color.Evaluate(1f);
                    }
                    else
                    {
                        blockMat.color = noDataColor;
                    }
                }
                break;
            //OVERLAY "FIRE RISKS"
            case 2:
                blockMat.color = noDataColor;
                break;
            //OVERLAY "POWER"
            case 3:
                if (block.scheme.consumption > 0)
                {
                    if (block.currentPower <= 0)
                    {
                        blockMat.color = overlay.color.Evaluate(0f);
                    }
                    else if (block.currentPower >= block.scheme.consumption)
                    {
                        blockMat.color = overlay.color.Evaluate(1f);
                    }
                    else
                    {
                        blockMat.color = overlay.color.Evaluate(0.5f);
                    }
                }
                else
                {
                    blockMat.color = noDataColor;
                }
                break;
            //OVERLAY "FOOD"
            case 4:
                blockMat.color = noDataColor;
                if (house != null)
                {
                    house.UpdateHouseInformations();
                    if (house.foodReceived <= 0 && house.foodConsumption > 0)
                    {
                        blockMat.color = overlay.color.Evaluate(0f);
                    }
                    else if (house.foodConsumption >= house.foodReceived)
                    {
                        blockMat.color = overlay.color.Evaluate(1f);
                    }
                    else if (house.foodConsumption < house.foodReceived)
                    {
                        blockMat.color = overlay.color.Evaluate(0.5f);
                    }
                }
                break;
            //OVERLAY "HABITATION"
            case 5:
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
                        blockMat.color = overlay.color.Evaluate(0f);
                    } else if (mediumNotation > 0)
                    {
                        blockMat.color = overlay.color.Evaluate(1f);
                    } else
                    {
                        blockMat.color = overlay.color.Evaluate(0.5f);
                    }
                } else
                {
                    blockMat.color = noDataColor;
                }
                break;
            //OVERLAY "CITIZEN COUNT"
            case 6:
                if (house != null)
                {
                    house.UpdateHouseInformations();
                    if (house.affectedCitizen.Count == 0)
                    {
                        blockMat.color = overlay.color.Evaluate(0f);
                    }
                    else if (house.affectedCitizen.Count >= house.slotAmount)
                    {
                        blockMat.color = overlay.color.Evaluate(1f);
                    }
                    else
                    {
                        blockMat.color = overlay.color.Evaluate(0.5f);
                    }
                }
                else
                {
                    blockMat.color = noDataColor;
                }
                break;

            //OVERLAY "NO OVERLAY"
            default:
                blockMat.color = noDataColor;
                block.overlayVisuals.SetActive(false);
                break;
        }
    }
}
