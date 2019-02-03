using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kickstarter : MonoBehaviour {

    public List<GameObject> gameComponents;

    private void Awake()
    {
        foreach(GameObject g in gameComponents) {
            GameObject i = Instantiate<GameObject>(g);
            DontDestroyOnLoad(i);
            Logger.Debug("Kickstarting " + i.name);
        }

        Destroy(gameObject);
    }
}
