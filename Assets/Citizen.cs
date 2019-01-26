﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Citizen : MonoBehaviour {

    public Population population;
    public GameObject body;
    public GameObject[] bodyParts;
    public Material mainMaterial;
    public enum Mood { Angry, Bad, Good, Jaded, Tired};
    
    Texture2D face;

    private void Awake()
    {
        UpdateColor();
        SetEmotion(Mood.Jaded);
    }

    void UpdateColor()
    {
        foreach(GameObject bodyPart in bodyParts) {
            if (bodyPart.GetComponent<MeshRenderer>() == null) {
                continue;
            }
            bodyPart.GetComponent<MeshRenderer>().materials[0].SetColor("_Color", population.color);
        }
    }

    public void SetEmotion(Mood mood)
    {
        face = population.humorTextures[(int)mood];
        body.GetComponent<MeshRenderer>().materials[0].mainTexture = face;
    }
}
