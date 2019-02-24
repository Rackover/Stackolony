using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingClouds : MonoBehaviour
{

    public Vector2 cloudsSpeed = new Vector2(10f, 25f);

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float factor = GameManager.instance.temporality.timeScale;
        transform.Translate(Time.deltaTime * factor * cloudsSpeed.x, 0f, Time.deltaTime * factor * cloudsSpeed.y);
    }
}
