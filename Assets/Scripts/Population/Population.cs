using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Population", menuName = "Population", order = 1)]
public class Population : ScriptableObject 
{
    public int ID;
    public string codeName; //Nom utilisé dans les CSV
    public Texture2D[] moodTextures;
    public Sprite[] moodSprites;
    public Color color;
    public GameObject prefab;
    public AudioClip voice;
    public float baseFoodConsumption = 1;
}
