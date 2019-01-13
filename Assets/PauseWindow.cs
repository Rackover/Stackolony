using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseWindow : MonoBehaviour {

    int oldTimescale;

    public void Start()
    {
        oldTimescale = GameManager.instance.temporality.timeScale;
    }

    public void Resume()
    {
        Destroy(gameObject);
        GameManager.instance.temporality.ChangeTimeScale(oldTimescale);
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
