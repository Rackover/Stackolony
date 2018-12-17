using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryManagement : MonoBehaviour {

    [Header("=== SETTINGS ===")]
    public int complexityMax = 10;

    [Space(2)][Header("=== REFERENCES ===")]
    public Transform shopPanelRegular; //Le panel qui contient les blocs de type normal
    public Transform shopPanelSpecial; //Le panel qui contient les blocs de type spécial
    public Slider complexitySlider;
    public GameObject blockDisplayPrefab;
    public GameObject mainPanel;

    [Space(1)][Header("Script")]
    public StorageBay storageBay;


    [System.NonSerialized] public int complexity;
    [System.NonSerialized] public List<ShopDisplay> shopDisplays;

    Library lib;

    public void Awake()
    {
        lib =  FindObjectOfType<Library>();

        complexity = 0;
        shopDisplays = new List<ShopDisplay>();
        //complexitySlider.value = 0;
        InitShop();
    }

    public void ToggleShop()
    {
        if (mainPanel.activeSelf == true)
        {
            mainPanel.SetActive(false);
        } else
        {
            mainPanel.SetActive(true);
        }
    }

    public void UpdateComplexity(int quantity)
    {
        complexity += quantity;
        complexitySlider.value = (float)complexity / (float)complexityMax;
        foreach (ShopDisplay shopDisplay in shopDisplays)
        {
            shopDisplay.SetPickable(shopDisplay.IsPickable());
        }
    }

    //Genere les blocs achetable
    public void InitShop()
    {
        foreach (Block block in lib.blocks)
        {
            GameObject newBlockDisplay = Instantiate(blockDisplayPrefab, shopPanelRegular);
            newBlockDisplay.name = "Display " + block.title;
            ShopDisplay newBlockSettings = newBlockDisplay.GetComponent<ShopDisplay>();
            shopDisplays.Add(newBlockSettings);
            newBlockSettings.deliveryManager = this;
            newBlockSettings.myBlock = block;
            newBlockSettings.InitShopDisplay();
            newBlockSettings.icon.sprite = block.icon;
        }
    }

    //Livre les blocs choisis au joueur
    public void DeliverBlocks()
    {
        foreach (ShopDisplay blocks in shopDisplays)
        {
            int quantityAsked = blocks.quantityPicked;
            if (quantityAsked > 0) {
                for (int i = 0; i < quantityAsked; i++)
                {
                    storageBay.DeliverBlock(blocks.myBlock);
                    blocks.DecreaseQuantity();
                }
            }
        }
    }
}
