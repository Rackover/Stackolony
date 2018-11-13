using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

Terrain terr; // terrain to modify
 int hmWidth; // heightmap width
 int hmHeight; // heightmap height
 
 public int posXInTerrain; //Position de la souris sur le terrain (axe X)
    public int posZInTerrain; //Position de la souris sur le terrain (axe Z)
 public int posYInTerrain; //Position de la souris sur le terrain en hauteur (axe Y)

    int size = 50; // the diameter of terrain portion that will raise under the game object
 float desiredHeight = 0; // the height we want that portion of terrain to be
 
 void Start () {
 
     terr = Terrain.activeTerrain;
     hmWidth = terr.terrainData.heightmapWidth;
     hmHeight = terr.terrainData.heightmapHeight;
 
 }
 
 void Update () {
        // DEBUG THE POSITION OF THE MOUSE ON THE TERRAIN
        // get the normalized position of this game object relative to the terrain
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.layer == 9)
            {
                Debug.Log(terr.terrainData.size.x);
                Debug.Log(hmWidth);
                Vector3 tempCoord = (hit.point-transform.position);
                Vector3 coord;
                coord.x = tempCoord.x / terr.terrainData.size.x;
                coord.y = tempCoord.y / terr.terrainData.size.y;
                coord.z = tempCoord.z / terr.terrainData.size.z;

                Debug.Log(coord);

                // get the position of the terrain heightmap where this game object is
                posXInTerrain = (int)(coord.x * terr.terrainData.size.x);
                posZInTerrain = (int)(coord.z * terr.terrainData.size.z);
                posYInTerrain = (int)(coord.y * terr.terrainData.size.y);
            }
        }
    }
}
