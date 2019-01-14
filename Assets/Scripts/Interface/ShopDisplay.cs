using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopDisplay : MonoBehaviour {

    [Header("Settings")]
    public BlockScheme myBlock;
    public bool canBePicked;
    public int quantityPicked;

    [Header("References")]
    public Text quantityPickedDisplay;
    public Image icon;
    public Image cantBePickedIcon;

    public DeliveryManagement dm;

    public void InitShopDisplay()
    {
        SetPickable(IsPickable());
        quantityPicked = 0;
        UpdateVisual();
    }

    public void SetQuantity(int quant)
    {
        if (canBePicked) {
            quantityPicked = quant;
            UpdateVisual();
            dm.UpdateComplexity(myBlock.complexity);
        }
    }

    public void IncreaseQuantity()
    {
        if (canBePicked) {
            quantityPicked++;
            UpdateVisual();
            dm.UpdateComplexity(myBlock.complexity);
        }
    }

    public void DecreaseQuantity()
    {
        if (quantityPicked > 0)
        {
            quantityPicked--;
            UpdateVisual();
            dm.UpdateComplexity(-myBlock.complexity);
        }
    }

    void UpdateVisual()
    {
        quantityPickedDisplay.text = "" + quantityPicked;
        Logger.Debug("Accessing library color for block "+ myBlock.name+":"+ myBlock.ID);
        GetComponent<Image>().color = GameManager.instance.library.blockContainerColors[myBlock.ID];
    }

    public bool IsPickable()
    {
        if (dm.complexity + myBlock.complexity > dm.complexityMax)
        {
            return false;
        } else
        {
            return true;
        }
    }
    public void SetPickable(bool pickable)
    {
        if (pickable == true)
        {
            canBePicked = true;
            cantBePickedIcon.enabled = false;
        } else
        {
            canBePicked = false;
            cantBePickedIcon.enabled = true;
        }
    }
}
