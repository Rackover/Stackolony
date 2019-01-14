using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatioportSpawner : MonoBehaviour {

    Vector3Int coordinates;
    CameraShake cameraShake;

    public float cameraShakePower = 0.3f;
    public float cameraShakeDuration = 4.5f;
    private GameObject shipGameObject;
    private Camera animCam;
    private Camera gameCam;
    public GameObject smokeParticle;


    private void Start()
    {
        GetCoordinates();
        cameraShake = GetComponent<CameraShake>();
        transform.position = GameManager.instance.gridManagement.IndexToWorldPosition(coordinates);
        shipGameObject = transform.Find("Ship").gameObject;
        animCam = transform.Find("AnimatorCameraPivot").transform.Find("Camera").GetComponent<Camera>();
        animCam.tag = "Untagged";
        animCam.targetDisplay = 0;
        gameCam = Camera.main;
        SwitchToCinematicCamera();
    }

    void SwitchToCinematicCamera()
    {
        Camera.main.targetDisplay = 1;
        animCam.targetDisplay = 0;
    }

    void SwitchToGameCamera()
    {
        Camera.main.targetDisplay = 0;
        animCam.targetDisplay = 1;
    }

    private void GetCoordinates()
    {
        GridManagement gridManager = GameManager.instance.gridManagement;
        coordinates = new Vector3Int(GameManager.instance.gridManagement.spatioportSpawnPoint.x, 0, GameManager.instance.gridManagement.spatioportSpawnPoint.y);
        float worldY = gridManager.myTerrain.SampleHeight(gridManager.IndexToWorldPosition(new Vector3Int(coordinates.x, 0, coordinates.z))) + (gridManager.cellSize.y/2); //+( GameManager.instance.gridManagement.cellSize.y/2); //Les blocs sont placés à 0.5case de haut par rapport au sol, on augmente donc la valeur y de cette taille
        int y = gridManager.WorldPositionToIndex(new Vector3(coordinates.x, worldY, coordinates.z)).y;
        coordinates.y = y;
    }

    private void ShakeCameraLerp()
    {
        StartCoroutine(ShakeCameraLerpCoroutine(cameraShakeDuration,cameraShakePower));
    }

    //Fonction apellée quand le vaisseau se crash
    private void TriggerCrash()
    {
        //Spawn de particules d'explosion
        smokeParticle.SetActive(true);
        smokeParticle.transform.SetParent(this.transform.parent);

        //Destruction du ship
        Destroy(shipGameObject);

        //Spawn du block
        GameManager.instance.gridManagement.SpawnBlock(0, coordinates);
    }

    private void TriggerStartAnimation()
    {
        GameManager.instance.cinematicManager.SetCinematicMode(true);
    }

    //Fonction apellée à la fin de l'animation
    private void TriggerEndAnimation()
    {
        StartCoroutine(TransitionToMainCamera());
        GetComponent<Animator>().enabled = false;
    }

    private void SpawnFinished()
    {
        GameManager.instance.cinematicManager.SetCinematicMode(false);
        SwitchToGameCamera();
        Destroy(this.gameObject);
    }


    private void SetShakeStrength(float strength)
    {
        cameraShake.shakeAmount = strength;
    }

    private IEnumerator TransitionToMainCamera()
    {
        yield return StartCoroutine(LerpAnimCamToGameCam(gameCam.transform.position, gameCam.transform.rotation, 2));
        SpawnFinished();
    }

    private IEnumerator LerpAnimCamToGameCam(Vector3 newPosition, Quaternion newRotation, float time)
    {
        float elapsedTime = 0;
        Vector3 startingPos = animCam.transform.position;
        Quaternion startingRot = animCam.transform.rotation;
        while (elapsedTime < time)
        {
            animCam.transform.rotation = Quaternion.Slerp(startingRot, newRotation, (elapsedTime / time));
            animCam.transform.position = Vector3.Slerp(startingPos, newPosition, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ShakeCameraLerpCoroutine(float duration, float shakeEndStrength)
    {
        cameraShake.shakeDuration = duration;
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            cameraShake.shakeAmount = shakeEndStrength * elapsedTime / duration;
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
