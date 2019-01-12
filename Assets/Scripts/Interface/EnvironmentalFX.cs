﻿using System.Collections;
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
        if (directionalLight == null) {
            Light[] lights = FindObjectsOfType<Light>();
            bool found = false;
            foreach (Light light in lights) {
                if (light.type == LightType.Directional) {
                    directionalLight = light.gameObject;
                    found = true;
                    break;
                }
            }
            if (!found) {
                Logger.Throw("Could not find a directional light for the temporality. Aborting.");
            }
        }
    }

    //Met à jour les lumières en fonction de l'avancement du cycle
    public void UpdateLights(float cycleProgressionInPercent)
    {
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3((3.6f * (float)cycleProgressionInPercent), 30, 0));
    }

    //Met à jour la skybox en fonction de l'avancement du cycle
    public void UpdateSkybox(float cycleProgressionInPercent)
    {
        skyboxMaterial.SetColor("_Tint", skyboxVariation.Evaluate(cycleProgressionInPercent / 100f));
        RenderSettings.skybox = skyboxMaterial;
        DynamicGI.UpdateEnvironment();
    }

    IEnumerator UpdateEnvironmentalFX()
    {
        Temporality temp = GameManager.instance.temporality;
        UpdateLights(temp.GetCurrentcycleProgression());
        UpdateSkybox(temp.GetCurrentcycleProgression());
        yield return new WaitForSeconds(refreshRate);
        yield return UpdateEnvironmentalFX();
    }
}
