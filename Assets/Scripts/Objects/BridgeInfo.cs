using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeInfo : MonoBehaviour {

    public Vector3Int origin = new Vector3Int();
    public Vector3Int destination = new Vector3Int();
    public Vector3Int[] allBridgePositions = new Vector3Int[0]; //Correspond aux positions de chaque bloc de pont dans la grille
}
