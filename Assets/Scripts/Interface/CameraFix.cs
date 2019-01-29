using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFix : MonoBehaviour {

    Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        StartCoroutine(FixClearFlags());
    }
    
    IEnumerator FixClearFlags()
    {
        cam.enabled = false;
        yield return null;
        cam.enabled = true;
        yield return true;
    }
}
