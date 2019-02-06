using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    public Button loadButton;
    public GameObject buttonsParent;
    public GameObject loadingObject;
    public GameObject newColonyWindow;

    private void Start()
    {
        if (GameManager.instance.saveManager.SaveExists()) {
            loadButton.interactable = true;
        }
    }

    public void SpawnNewColonyMenu()
    {
        if (FindObjectsOfType<NewColonyWindow>().Length <= 0) {
            Instantiate(newColonyWindow, transform);
        }
    }

    public void StartNewGame()
    {
        HideMenu();
        DisplayLoading();
        GameManager.instance.NewGame();
    }

    void DisplayLoading()
    {
        Instantiate(loadingObject);
    }

    void HideMenu()
    {
        buttonsParent.SetActive(false);
    }
}
