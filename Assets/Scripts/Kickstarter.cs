using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kickstarter : MonoBehaviour {

    public List<GameObject> gameComponents;

    private void Awake()
    {
        // Game has already been kickstarted
        if (GameManager.instance != null) {
            DestroyImmediate(gameObject);
            return;
        }

        // Kickstart
        foreach(GameObject g in gameComponents) {
            GameObject i = Instantiate<GameObject>(g);
            Logger.Debug("Kickstarting " + i.name);
            DontDestroyOnLoad(i);
        }

        Destroy(gameObject);
    }
}
