using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour {

    private void Start()
    {
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody newRb = gameObject.AddComponent<Rigidbody>();
            newRb.isKinematic = true;
        }
        if (GetComponent<Collider>() == null)
        {
            BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
            newCollider.isTrigger = true;
        }
    }
    public void OnCollision()
    {
        Destroy(this.gameObject);
    }
}
