using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour {

    public Transform target;
    public float speed = 25f;
    


    // Update is called once per frame
    void Update () {
        transform.RotateAround(target.position, Vector3.up, Time.deltaTime * speed);
        transform.LookAt(target.position);
        
    }
}
