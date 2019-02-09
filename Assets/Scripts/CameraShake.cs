using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    // Transform of the camera to shake. Grabs the gameObject's transform
    // if null.
    Transform camTransform;

    // How long the object should shake for.

    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;

    Vector3 originalPos;
    float shakeDuration = 0f;

    public void SetCamera(Camera cam)
    {
        camTransform = cam.transform;
    }

    public void Shake(float duration)
    {
        shakeDuration = duration;
        originalPos = camTransform.localPosition;
    }
    void Update()
    {
        if (camTransform == null) {
            shakeDuration = 0f;
            return;
        }
        if (shakeDuration > 0)
        {
            camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            shakeDuration -= Time.deltaTime * decreaseFactor;
            if (shakeDuration <= 0) {
                camTransform.localPosition = originalPos;
            }
        }
    }
}