using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public string playerName = "Player";
    public Options options;

    private void Start()
    {
        options = new Options();
        options.LoadFromDisk(Paths.GetOptionsFile());
    }
}
