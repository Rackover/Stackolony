using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingMenuDisplay : MonoBehaviour {

    public GameObject menu;


    public void ToggleMenu()
    {
        menu.SetActive(!menu.activeSelf);
    }
}
