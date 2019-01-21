using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseWindow : MonoBehaviour {

    int oldTimescale;

    public void Start()
    {
        GameManager.instance.Pause();
    }

    public void Resume()
    {
        GameManager.instance.UnPause();
        Destroy(gameObject);
    }

    public void Options()
    {
        FindObjectOfType<Interface>().SpawnOptionsWindow();
    }

    public void ExitToMenu()
    {
        GameManager.instance.ExitToMenu();
    }
}
