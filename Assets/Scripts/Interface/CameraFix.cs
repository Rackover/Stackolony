using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFix : MonoBehaviour {

    Camera cam;
    public enum fix { clearFlags, depth};
    public fix type;

    private void Start()
    {
        cam = GetComponent<Camera>();
        switch (type) {
            case fix.clearFlags: StartCoroutine(FixClearFlags()); break;
            case fix.depth: StartCoroutine(FixCameraDepth()); break;
        }
        
    }
    
    IEnumerator FixClearFlags()
    {
        yield return new WaitForEndOfFrame();
        cam.enabled = false;
        yield return null;
        cam.enabled = true;
        yield return true;
    }

    IEnumerator FixCameraDepth()
    {
        yield return new WaitForEndOfFrame();
        cam.clearFlags = CameraClearFlags.Nothing;
        yield return null;
        cam.clearFlags = CameraClearFlags.Depth;
        yield return true;
    }
}
