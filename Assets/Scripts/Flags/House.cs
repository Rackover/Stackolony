using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : Flag {
    public int slotAmount;
    public string[] profiles;

    //Liste de chaque occupator disponibles pour les citoyens
    public List<Occupator> occupatorsInRange = new List<Occupator>();



    public override void Awake()
    {
        base.Awake();
        GameManager.instance.systemManager.AllHouses.Add(this);
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemManager.AllHouses.Remove(this);
        base.OnDestroy();
    }
}
