using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentalFX : MonoBehaviour {


    [Header("=== SETTINGS ===")]
    [Space(1)]
    public float refreshRate = 0.1f;

    [Header("=== REFERENCES ===")]
    [Space(1)]
    public GameObject directionalLight;
    public Material skyboxMaterial;
    public Gradient skyboxVariation; //Variations de couleur de la skybox au fil du temps

    private void Awake()
    {
        skyboxMaterial = Instantiate(skyboxMaterial);
        RenderSettings.skybox = skyboxMaterial;

        if (directionalLight == null) {
            bool found = FindDirectionalLight();
            RenderSettings.sun = directionalLight.GetComponent<Light>();

            if (!found) {
                Logger.Throw("Could not find a directional light for the temporality. Aborting.");
            }
        }
    }

    private void Start()
    {
        StartCoroutine(UpdateEnvironmentalFX());
    }

    bool FindDirectionalLight()
    {
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights) {
            if (light.type == LightType.Directional) {
                directionalLight = light.gameObject;
                return true;
            }
        }
        return false;
    }

    //Met à jour les lumières en fonction de l'avancement du cycle
    public void UpdateLights(float cycleProgressionInPercent)
    {
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3((3.6f * (float)cycleProgressionInPercent), 30, 0));
        directionalLight.GetComponent<Light>().color = skyboxVariation.Evaluate(cycleProgressionInPercent / 100f);
    }

    //Met à jour la skybox en fonction de l'avancement du cycle
    public void UpdateSkybox(float cycleProgressionInPercent)
    {
        skyboxMaterial.SetColor("_Tint", skyboxVariation.Evaluate(cycleProgressionInPercent / 100f));
    }

    //Met à jour la fog selon la couleur du ciel
    public void UpdateFog(float cycleProgressionInPercent)
    {
        RenderSettings.fogColor = skyboxVariation.Evaluate(cycleProgressionInPercent / 100f);
        skyboxVariation.Evaluate(cycleProgressionInPercent / 100f);
    }

    IEnumerator UpdateEnvironmentalFX()
    {
        if (directionalLight == null) {
            if (!FindDirectionalLight()) {
                yield return false;
            }
        }

        Temporality temp = GameManager.instance.temporality;
        UpdateLights(temp.GetCurrentCycleProgression());
        UpdateFog(temp.GetCurrentCycleProgression());
        UpdateSkybox(temp.GetCurrentCycleProgression());
        DynamicGI.UpdateEnvironment();
        yield return new WaitForSeconds(refreshRate);
        yield return UpdateEnvironmentalFX();
    }
}
