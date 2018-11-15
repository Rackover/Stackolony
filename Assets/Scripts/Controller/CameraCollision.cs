using UnityEngine;

public class CameraCollision : MonoBehaviour {
	[HideInInspector] public bool colliding;

    void OnTriggerStay(Collider other) 
    {
        colliding = true;
    }
}
