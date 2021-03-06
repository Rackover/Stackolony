﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour {

    [Header("=== REFERENCIES ===")] [Space(1)]
    [Header("Camera")] [Space(1)]
    public GameObject cameraModel;
    public GameObject editorOnlyDummy;
    public GameObject gameDummy;
    GameObject cameraInstance;
    [Header("Transforms")] [Space(1)]
    public Transform camCenter;                 // Center of the camera (Used for Lookat + movement)
    public Transform camPivot;                  // Pivot point of the camera (Used for rotation)
    Vector3 startPosition;
    //public Toggle control;

    [Space(2)][Header("=== SETTINGS ===")][Space(1)]
    [Header("Drifting")]
    public float mouseDriftSensibility = 0.1f;     // Set the sensibility of the drift of the camera center
    public Vector2 driftBorder = new Vector2(25f, 25f);
    public float cameraCatchUpSpeed = 5f;       // Speed at which the camera will catch up to its supposed location. Increasing this value decreases the camera lag
    public float baseHeight;
    public float maxDistance = 100f;

    [Header("Rotating")]
    public float rotateSensibility = 1f;
    public float minLookAngle = 10f;
    public float maxLookAngle = 80f;
    [Header("Zoom")]
    public float zoomStep = 1f;
    public float minZoom = 0f;
    public float maxZoom = 10f;
    public LayerMask collideWith;
    public float size = 5f;
    //public float heightTarget;

    private float borderSensibility = 0.5f;
    private float rotationSensibility = 0.5f;
    private float grabSensitivity = 0.5f;

    bool driftEnabled = true;
    bool borderDriftEnabled = true;
    Transform camTransform;              // Transform of the main camera
    Transform cameraTransformObjective;  // The camera will try to have the same transform as this
    Vector3 mouseDelta;
    Vector3 lastMousePosition;
    Vector3 mapCenter;
    bool isFrozen = false;

    void Awake()
    {
        startPosition = transform.position;
        // Spawning camera
        cameraInstance = Instantiate(cameraModel);
        camTransform = cameraInstance.transform;

        cameraInstance.GetComponent<Camera>().targetDisplay = 0;
        cameraInstance.transform.GetChild(0).GetComponent<Camera>().targetDisplay = 0;

        camTransform.rotation = editorOnlyDummy.transform.rotation;
        camTransform.position = editorOnlyDummy.transform.position;
        DestroyImmediate(editorOnlyDummy);
        cameraInstance.tag = "MainCamera";

        // creating two dummies for the camera lerping
        // Camera dummy (zoom)
        gameDummy = new GameObject(".Dummy-Camera-DoNotEditDirectly");
        gameDummy.transform.parent = camPivot;
        gameDummy.transform.SetPositionAndRotation(camTransform.position, camTransform.rotation);
        gameDummy.transform.rotation = camTransform.rotation;
        cameraTransformObjective = gameDummy.transform;
    }

    void Start()
    {
        mapCenter = GameManager.instance.gridManagement.IndexToWorldPosition(
            new Vector3Int(
                GameManager.instance.gridManagement.grid.GetLength(0)/2,
                0,
                GameManager.instance.gridManagement.grid.GetLength(2)/2
            )
        );

        startPosition = mapCenter;
    }

    void Update()
    {
        if (isFrozen) return;

        // Fetch settings from the options
        borderSensibility = GameManager.instance.player.options.GetFloat(Options.Option.borderSensitivity);
        rotationSensibility = GameManager.instance.player.options.GetFloat(Options.Option.rotationSensitivity);
        grabSensitivity = GameManager.instance.player.options.GetFloat(Options.Option.grabSensitivity);

        UpdateCameraCenterHeight();

        // Was intended but dosn't add anything
        //if(!GameManager.instance.cursorManagement.cursorOnUI)

        cameraTransformObjective.LookAt(camCenter.position);

        if (Input.GetButton("MoveCamera"))
        {
            //driftEnabled = false;
            Move();
        }
        if (Input.GetButton("RotateCamera"))
        { 
            //driftEnabled = false;
            Rotation();
        }
        Drift();

        if(!GameManager.instance.cursorManagement.cursorOnUI)
        {
            Zoom();
        }
        // CLAMP CAMERA
        if(transform.position.x > mapCenter.x + maxDistance)
        {
            transform.position = new Vector3(mapCenter.x + maxDistance, transform.position.y, transform.position.z);
        }
        else if(transform.position.x < mapCenter.x - maxDistance)
        {
            transform.position = new Vector3(mapCenter.x - maxDistance, transform.position.y, transform.position.z);
        }

        if(transform.position.z > mapCenter.z + maxDistance)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, mapCenter.z + maxDistance);
        }
        else if(transform.position.z < mapCenter.z - maxDistance)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, mapCenter.z - maxDistance);
        }

        CatchUpCameraObjective();
    }

    public void SetCameraPositionAndRotation(Vector3 p, Quaternion r)
    {
        cameraInstance.transform.SetPositionAndRotation(p, r);
    }

    public void FreezeCameraPosition()
    {
        isFrozen = true;
    }

    public void FreeCameraPosition()
    {
        isFrozen = false;
    }

    void UpdateCameraCenterHeight()
    {
        transform.position = new Vector3(transform.position.x, GetHeightObjective(), transform.position.z);
    }

    float GetHeightObjective()
    {
        if (GameManager.instance.gridManagement.myTerrain == null) {
            return baseHeight;
        }
        float ho = GameManager.instance.gridManagement.myTerrain.SampleHeight(transform.position) + baseHeight;
        return ho; 
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
    }

    void CatchUpCameraObjective() {
        RaycastHit hit;
        Vector3 catchUpPosition = cameraTransformObjective.position;

        if (Physics.Raycast(camCenter.position, (cameraTransformObjective.position- camCenter.position).normalized, out hit, Mathf.Infinity, collideWith))
        {
            catchUpPosition = (camCenter.position - hit.point).normalized* size + hit.point;
        }

        camTransform.position = Vector3.Lerp(camTransform.position, catchUpPosition, cameraCatchUpSpeed * Time.deltaTime);
        camTransform.rotation = Quaternion.Lerp(camTransform.rotation, cameraTransformObjective.rotation, cameraCatchUpSpeed * Time.deltaTime);
    }

    private void Move()
    {
        if (Input.GetButtonDown("MoveCamera")) {
            lastMousePosition = (Vector3)Input.mousePosition;
        }
        mouseDelta = (Vector3)lastMousePosition - (Vector3)Input.mousePosition;
        camCenter.position += mouseDelta.x * cameraTransformObjective.right.normalized * mouseDriftSensibility * grabSensitivity;
        camCenter.position += mouseDelta.y * new Vector3(cameraTransformObjective.forward.x, 0, cameraTransformObjective.forward.z).normalized * mouseDriftSensibility * grabSensitivity;
        lastMousePosition = (Vector3)Input.mousePosition;
    }

    void Rotation()
    {
        Vector2 lookDirection = new Vector2(
            Input.GetAxis("CursorX") * rotateSensibility * rotationSensibility,
            Input.GetAxis("CursorY") * rotateSensibility * rotationSensibility
        );
        camPivot.rotation = Quaternion.Euler(new Vector3(
            Mathf.Clamp(camPivot.eulerAngles.x - lookDirection.y, minLookAngle, maxLookAngle), 
            camPivot.eulerAngles.y+ lookDirection.x, 
            camPivot.eulerAngles.z)
        );
    } 

    void Drift()
    {
        Vector2 drift = Vector2.zero;

        if(GameManager.instance.player.options.GetBool(Options.Option.enableDrifting))
        {
            drift += new Vector2(
                -Mathf.Clamp(driftBorder.x -Input.mousePosition.x, 0, 1) + Mathf.Clamp(Input.mousePosition.x - Screen.width + driftBorder.x, 0, 1),
                -Mathf.Clamp(driftBorder.y - Input.mousePosition.y, 0, 1) + Mathf.Clamp(Input.mousePosition.y - Screen.height + driftBorder.y, 0, 1)
            );
        }

        drift += new Vector2(
            Input.GetAxis("DriftCameraX"), 
            Input.GetAxis("DriftCameraY")
        );

        if (drift.magnitude > 0) {
            if (driftEnabled) {
                float oldY = camCenter.position.y;
                camCenter.position += 
                    (drift.x * borderSensibility) * cameraTransformObjective.right.normalized * Time.deltaTime
                    + (drift.y * borderSensibility) * cameraTransformObjective.forward.normalized * Time.deltaTime;
                camCenter.position = new Vector3(camCenter.position.x, oldY, camCenter.position.z);
            }
        }
        else {
            driftEnabled = true;
        }
    } 

    void Zoom() 
    {
        float o = GetHeightObjective();

        if (Input.GetAxis("ZoomCamera") > 0 && cameraTransformObjective.position.y > minZoom + o ||
            Input.GetAxis("ZoomCamera") < 0 && cameraTransformObjective.position.y < maxZoom + o) {

            // Zoom in
            cameraTransformObjective.transform.position += cameraTransformObjective.transform.forward * Input.GetAxis("ZoomCamera") * zoomStep;
        }
    }
}