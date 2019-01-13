using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    public Button loadButton;

    private void Start()
    {
        if (GameManager.instance.saveManager.SaveExists()) {
            loadButton.interactable = true;
        }
    }
}
