﻿using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour {

    [Header("=== REFERENCIES ===")][Space(1)]
    public Transform camTransform;
    public Transform camCenter;                 // Center of the camera (Used for Lookat + movement)
    public Transform camPivot;                  // Pivot point of the camera (Used for rotation)
    Transform camTarget;              // Current target of the camera (Should be null at start)
    //public Toggle control;

    [Space(2)][Header("=== SETTINGS ===")][Space(1)]
    [Header("Drifting")]
    public float driftSensibility = 0.001f;     // Set the sensibility of the drift of the camera center
    public float maxDrift = 100f;               // Set the maximum drift speed of the camera center
    [Header("Rotating")]
    public float rotateSensibility = 1f;        
    public float minLookAngle = 10f;
    public float maxLookAngle = 80f;
    [Header("Zoom")]
    public float zoomStep = 1f;
    public float minZoom = 0f;
    public float maxZoom = 10f;

    [Space(2)][Header("=== TEMP SETTINGS ===")][Space(1)]
    [Header("Movement feedback")][Space(1)]
    public Transform cursorPoint;               // White dot in the drifting feedback
    public GameObject cursor;                   // Drifting feedback object
    public float cursorSensibility = 1f;
    public float cursorMaxDistance = 10f;

    float xLook;
    float yLook;
    Vector2 lastMousePosition;

    void Update()
    {
        Drift();
        Rotation();
        Zoom();
        DriftFeedback();
    }

    void Rotation()
    {
        if(Input.GetButton("MouseRight"))
        {
            xLook += Input.GetAxis("MouseX") * rotateSensibility;
            yLook += Input.GetAxis("MouseY") * rotateSensibility;
            yLook = Mathf.Clamp(yLook, minLookAngle, maxLookAngle);
            camPivot.rotation = Quaternion.Euler(-yLook, xLook, 0); 
        }	
    } // Rotate the camera with the mouse wheel button around the center point

    float xShift;
    float yShift;
    void Drift()
    {
        // Make the camera look at the target
        camTransform.LookAt(camCenter.position);
        // MOVEMENT
        if (Input.GetButtonDown("MouseMiddle"))
        {
            lastMousePosition = Input.mousePosition;
            camTarget = null;
            camCenter.position = new Vector3(camCenter.position.x, 0f, camCenter.position.z);
        }
        
        // NEW SOLUTION : Using 2 seperate translate forward and right take care of the camera orientation
        if(Input.GetButton("MouseMiddle"))
        {
            Vector3 directionForward = new Vector3(camTransform.forward.x, 0, camTransform.forward.z);
            xShift = lastMousePosition.x - Input.mousePosition.x;
            yShift = lastMousePosition.y - Input.mousePosition.y;

            xShift = Mathf.Clamp(xShift, -maxDrift, maxDrift);
            yShift = Mathf.Clamp(yShift, -maxDrift, maxDrift);

            camCenter.position -= xShift * camTransform.right.normalized * driftSensibility;
            camCenter.position -= yShift * directionForward.normalized * driftSensibility;
        }
    } // Drift the camera center horizontaly on the z and x axis with mouse movement

    void DriftFeedback()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width/2, Screen.height/2));
       
        if (Input.GetButtonDown("MouseRight"))
            cursor.SetActive(true);

        if (Physics.Raycast(ray, out hit)) 
            cursor.transform.position = new Vector3(hit.point.x, hit.point.y + 0.5f, hit.point.z);
 
        cursor.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
       
        Vector3 newCursorPosition = new Vector3(
            xShift / cursorMaxDistance,
            0f,
            yShift / cursorMaxDistance
        );
        cursorPoint.localPosition = -newCursorPosition;

        if (Input.GetButton("MouseRight"))
        {
            cursor.SetActive(false);
        }
    }

    void Zoom()
    {
        if(Input.GetAxis("MouseWheel") > 0 && camTransform.position.y > minZoom)
            camTransform.Translate(Vector3.forward * Input.GetAxis("MouseWheel") * zoomStep);
        else if(Input.GetAxis("MouseWheel") < 0 && camTransform.position.y < maxZoom)
            camTransform.Translate(Vector3.forward * Input.GetAxis("MouseWheel") * zoomStep);   
    }
}
