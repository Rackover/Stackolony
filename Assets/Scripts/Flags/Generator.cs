using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Flag {
    public int power;
    public string type;


    public override void Enable()
    {
        base.Enable();
        Debug.Log("Power is " + power + " and type is " + type);
    }
}
