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
        quantityPickedDisplay.text = "" + quantityPicked;
    }

    public void SetQuantity(int quant)
    {
        if (canBePicked) {
            quantityPicked = quant;
            quantityPickedDisplay.text = "" + quantityPicked;
            dm.UpdateComplexity(myBlock.complexity);
        }
    }

    public void IncreaseQuantity()
    {
        if (canBePicked) {
            quantityPicked++;
            quantityPickedDisplay.text = "" + quantityPicked;
            dm.UpdateComplexity(myBlock.complexity);
        }
    }

    public void DecreaseQuantity()
    {
        if (quantityPicked > 0)
        {
            quantityPicked--;
            quantityPickedDisplay.text = "" + quantityPicked;
            dm.UpdateComplexity(-myBlock.complexity);
        }
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
