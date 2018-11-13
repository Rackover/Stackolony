using UnityEngine;

public class CameraCollision : MonoBehaviour {
	[HideInInspector] public bool colliding;

    void OnTriggerStay(Collider other) 
    {
		Debug.Log("yo");
        colliding = true;
    }
}
