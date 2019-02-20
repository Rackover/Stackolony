using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatioportSpawner : MonoBehaviour {

    Vector3Int coordinates;
    CameraShake cameraShake;

    public float cameraShakePower = 0.3f;
    public float cameraShakeDuration = 4.5f;
    public float cameraRecoverTime = 2f;
    private GameObject shipGameObject;
    public GameObject smokeParticle;
    public int spatioportID = 0;
    public GameObject cameraDummy;

    CinematicManager cineMan;
    CameraController camController;
    Vector3 endPosition;
    Quaternion endRotation;

    private void Start()
    {
        cineMan = GameManager.instance.cinematicManager;
        GetCoordinates();

        if (!cineMan.areCinematicsEnabled) {
            GameManager.instance.gridManagement.SpawnBlock(spatioportID, coordinates);
            DestroyImmediate(gameObject);
            return;
        }

        camController = FindObjectOfType<CameraController>();
        cameraShake = cineMan.gameObject.GetComponent<CameraShake>();

        endPosition = Camera.main.transform.position;
        endRotation = Camera.main.transform.rotation;

        camController.FreezeCameraPosition();
        camController.SetCameraPositionAndRotation(cameraDummy.transform.position, cameraDummy.transform.rotation);
        cineMan.SetCinematicMode(true);

        GetComponent<Animator>().SetTrigger("StartAnimation");

        transform.position = GameManager.instance.gridManagement.IndexToWorldPosition(coordinates);
        shipGameObject = transform.Find("Ship").gameObject;
    }

    private void Update()
    {
        camController.SetCameraPositionAndRotation(cameraDummy.transform.position, cameraDummy.transform.rotation);
    }

    private void GetCoordinates()
    {
        GridManagement gridManager = GameManager.instance.gridManagement;
        coordinates = new Vector3Int(gridManager.spatioportSpawnPoint.x, 0, gridManager.spatioportSpawnPoint.y);
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
        GameManager.instance.gridManagement.SpawnBlock(spatioportID, coordinates);
    }

    //Fonction apellée à la fin de l'animation
    private void TriggerEndAnimation()
    {
        StartCoroutine(TransitionToMainCamera());
        GetComponent<Animator>().enabled = false;
    }

    private void SpawnFinished()
    {
        cineMan.SetCinematicMode(false);
        camController.FreeCameraPosition();
        Destroy(this.gameObject);
    }


    private void SetShakeStrength(float strength)
    {
        cameraShake.shakeAmount = strength;
    }

    private IEnumerator TransitionToMainCamera()
    {
        yield return StartCoroutine(LerpAnimCamToGameCam(endPosition, endRotation, cameraRecoverTime));
        SpawnFinished();
    }

    private IEnumerator LerpAnimCamToGameCam(Vector3 newPosition, Quaternion newRotation, float time)
    {
        float elapsedTime = 0;
        // Correction to counter weird time flow in coroutines
        float timeToReach = time / 3;
        while (elapsedTime < timeToReach)
        {
            cameraDummy.transform.rotation = Quaternion.Lerp(cameraDummy.transform.rotation, newRotation, (elapsedTime / time));
            cameraDummy.transform.position = Vector3.Lerp(cameraDummy.transform.position, newPosition, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ShakeCameraLerpCoroutine(float duration, float shakeEndStrength)
    {
        float elapsedTime = 0;
        Vector3 originalPos = cameraDummy.transform.localPosition;
        while (elapsedTime < duration)
        {
            float shakeAmount = shakeEndStrength * elapsedTime / duration;
            cameraDummy.transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        cameraDummy.transform.localPosition = originalPos;
        yield return true;
    }

    
    public void SoundArrival()
    {
        GameManager.instance.soundManager.Play("ShipArrival");
    }

    public void SoundSteam()
    {
        GameManager.instance.soundManager.Play("ShipSteam");
    }

    public void SoundExplosion()
    {
        GameManager.instance.soundManager.Play("ShipExplosion");
    }
}
