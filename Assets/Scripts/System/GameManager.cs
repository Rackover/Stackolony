using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    //REFERENCES
    private Temporality temporality;

    private void Start()
    {
        //Initialisation des references
        temporality = FindObjectOfType<Temporality>();
    }
    void Update () {
        CheckInputs();
    }

    void CheckInputs()
    {
        //Pause
        if (Input.GetKeyDown(KeyCode.P))
        {
            temporality.PauseGame();
        }
    }
}
