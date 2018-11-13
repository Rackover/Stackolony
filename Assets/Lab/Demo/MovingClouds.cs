using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingClouds : MonoBehaviour {

    public Vector2 cloudsSpeed = new Vector2(10f, 25f);

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate(Time.deltaTime* cloudsSpeed.x, 0f, Time.deltaTime* cloudsSpeed.y);
	}
}
