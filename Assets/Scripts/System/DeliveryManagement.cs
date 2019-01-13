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

    [System.NonSerialized] public int complexity;
    [System.NonSerialized] public List<ShopDisplay> shopDisplays;



    public void Start()
    {
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

    public void LoadShop(List<ShopDisplay> shops)
    {
        foreach (ShopDisplay item in shops)
        {
            foreach(ShopDisplay sd in shopDisplays)
            {
                if(item.myBlock.ID == sd.myBlock.ID)
                {
                    sd.SetQuantity(item.quantityPicked);
                }
            }
        }
    }

    //Genere les blocs achetable
    public void InitShop()
    {
        foreach (BlockScheme block in GameManager.instance.library.blocks)
        {
            GameObject newBlockDisplay = Instantiate(blockDisplayPrefab, shopPanelRegular);
            try {
                newBlockDisplay.name = "Display " + block.title;
            }
            catch(System.NullReferenceException e) {
                Debug.LogError("An error occured while initializing the shop. Check that the GAME MANAGER LIBRARY is loaded correctly.");
                UnityEditor.EditorApplication.isPlaying = false;
            }

            ShopDisplay newBlockSettings = newBlockDisplay.GetComponent<ShopDisplay>();
            shopDisplays.Add(newBlockSettings);
            newBlockSettings.dm = GameManager.instance.deliveryManagement;
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
                    GameManager.instance.storageBay.DeliverBlock(blocks.myBlock);
                    blocks.DecreaseQuantity();
                }
            }
        }
    }
}
