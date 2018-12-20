using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Population", menuName = "Population", order = 1)]
public class Population : ScriptableObject {

    public int ID;
    public Image visuals;
    public string displayName; //Nom affiché au joueur
    public string codeName; //Nom utilisé dans le code
    public string description;
}
