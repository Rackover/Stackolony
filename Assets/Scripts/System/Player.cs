using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public string playerName = "Player";
    public Options options = new Options();

    private void Awake()
    {
        options.LoadFromDisk(Paths.GetOptionsFile());
    }
}
