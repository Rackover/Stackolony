using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseWindow : MonoBehaviour {

    int oldTimescale;

    public void Start()
    {
        oldTimescale = GameManager.instance.temporality.timeScale;
        GameManager.instance.temporality.SetTimeScale(0);
    }

    public void Resume()
    {
        Destroy(gameObject);
        GameManager.instance.temporality.SetTimeScale(oldTimescale);
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
