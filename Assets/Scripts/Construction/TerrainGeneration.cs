using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Terrain))]
public class TerrainGeneration : MonoBehaviour {

    Terrain myTerrain;

    void Start()
    {
        myTerrain = GetComponent<Terrain>();
    }
}
