using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Population", menuName = "Population", order = 1)]
public class Population : ScriptableObject {

    public int ID;
    public string codeName; //Nom utilisé dans les CSV
    public Sprite humorSprite;
    public Color color;
    public float foodConsumption;
}
