using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingMenuDisplay : MonoBehaviour {

    public GameObject menu;
    public Button button;
    
    public void ToggleMenu()
    {
        menu.SetActive(!menu.activeSelf);
    }


}
