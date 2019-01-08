using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorOrientation : MonoBehaviour {
    public Material material;
	
	// Update is called once per frame
	void Update () {
        material.SetVector("_ProjectorDir", new Vector4(transform.forward.x, transform.forward.y,transform.forward.z,0.1f));
    }
}
