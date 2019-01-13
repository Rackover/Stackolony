using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    public Button loadButton;
    public GameObject buttonsParent;
    public GameObject loadingObject;

    private void Start()
    {
        if (GameManager.instance.saveManager.SaveExists()) {
            loadButton.interactable = true;
        }
    }

    public void DisplayLoading()
    {
        Instantiate(loadingObject);
    }

    public void HideMenu()
    {
        buttonsParent.SetActive(false);
    }
}
