﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Temporality : MonoBehaviour {

    [Header("=== SETTINGS ===")][Space(1)]
    public int cycleDuration; //Durée d'un cycle en secondes
    public int yearDuration; //Durée d'une année en cycle
    public int initialTimeScale; //Coefficient de vitesse d'écoulement du temps
    public float timeBetweenUpdateForSkybox; //Combien de secondes entre chaque mise à jour visuelle de la skybox
    public float timeBetweenUpdateForLights; //Combien de secondes entre chaque mise à jour visuelle des lumières
    public float timeBetweenUpdateForDayNightDisplay; //Combien de secondes entre chaque mise à jour visuelle de l'afficheur "jour / nuit"

    public Gradient skyboxVariation; //Variations de couleur de la skybox au fil du temps

    [Header("=== REFERENCES ===")][Space(1)]
    public Text cycleNumberText;
    public Text yearNumberText;
    public GameObject directionalLight;
    public Material skyboxMaterial;
    public GameObject dayNightDisplay;
    public GameObject timescaleButtonHolder;


    [Header("=== DEBUG VALUES ===")][Space(1)]
    private int cycleNumber; //Combien de cycles se sont ecoulés en tout
    private int yearNumber; //Combien d'années se sont ecoulées en tout
    public float cycleProgression; //Combien de secondes se sont ecoulées dans le cycle actuel
    public int timeScale; //Coefficient de vitesse d'écoulement du temps
    //private Coroutine timeCoroutine; //Coroutine pour gérer la progression des cycles, obsoléte

    private float timeBetweenUpdateForSkyboxCount = 0;
    private float timeBetweenUpdateForLightsCount = 0;
    private float timeBetweenUpdateForDayNightDisplayCount = 0;

    private float recurence = 0; //Correspond au temps entre chaques fois ou on verifiera pour faire une mise à jour visuelle

    private float counter;

    public void Awake()
    {
        counter = 0;

        timeScale = initialTimeScale;
        recurence = Mathf.Min(timeBetweenUpdateForDayNightDisplay, timeBetweenUpdateForLights, timeBetweenUpdateForSkybox);
        //timeCoroutine = StartCoroutine(updateCycleProgression(recurence));

        cycleNumber = 0;
        yearNumber = 0;

        cycleNumberText.text = "CYCLE : " + cycleNumber;
        yearNumberText.text = "YEAR : " + yearNumber;
    }

    public void Update() {
        counter += Time.deltaTime * timeScale;
        if (counter >= recurence) {
            counter = 0;
            TimeUpdate();
        }
    }

    //Met à jour le temps et les visuels
    public void TimeUpdate() {
        if (cycleProgression < cycleDuration)
        {
            cycleProgression+= recurence;
        } else
        {
            cycleProgression = 0;
            AddCycle();
        }

        //Update des timers pour les mises à jours de visuels
        timeBetweenUpdateForSkyboxCount+= recurence;
        timeBetweenUpdateForLightsCount+= recurence;
        timeBetweenUpdateForDayNightDisplayCount+= recurence;

        //Mises à jour des visuels
        float cycleProgressionInPercent = (cycleProgression / (float)cycleDuration) * 100f;
        if (timeBetweenUpdateForDayNightDisplayCount >= timeBetweenUpdateForDayNightDisplay)
        {
            UpdateDayNightDisplay(cycleProgressionInPercent);
            timeBetweenUpdateForDayNightDisplayCount = 1;
        }

        if (timeBetweenUpdateForLightsCount > timeBetweenUpdateForLights)
        {
            UpdateLights(cycleProgressionInPercent);
            timeBetweenUpdateForLightsCount = 1;
        }

        if (timeBetweenUpdateForSkyboxCount > timeBetweenUpdateForSkybox)
        {
            UpdateSkybox(cycleProgressionInPercent);
            timeBetweenUpdateForSkyboxCount = 1;
        }
    }

    public void ChangeTimeScale(int newTimeScaleCoef)
    {
        timeScale = newTimeScaleCoef;
    }

    public void EnableButton(Image button)
    {
        foreach (Transform child in timescaleButtonHolder.transform)
        {
            child.GetComponent<Image>().color = new Color32(255, 255, 255, 55);
        }
        button.color = new Color32(255, 255, 255, 255);
    }

    public void AddCycle() //Ajoute un cycle au compteur
    {
        cycleNumber++;
        cycleNumberText.text = "CYCLE : " + cycleNumber;
        if (cycleNumber%yearDuration == 0) 
            AddYear();
    }

    public void AddYear() //Ajoute un an au compteur
    {
        yearNumber++;
        yearNumberText.text = "YEAR : " + yearNumber;
    }

    //Met à jour les lumières en fonction de l'avancement du cycle
    public void UpdateLights(float cycleProgressionInPercent)
    {
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3(90f+((360f * (float)cycleProgressionInPercent) /100f),30,0));
    }

    //Met à jour la skybox en fonction de l'avancement du cycle
    public void UpdateSkybox(float cycleProgressionInPercent)
    {
        skyboxMaterial.SetColor("_Tint", skyboxVariation.Evaluate(cycleProgressionInPercent/100f));
        //skyboxMaterial.color = skyboxVariation.Evaluate(cycleProgressionInPercent);
        RenderSettings.skybox = skyboxMaterial;
        DynamicGI.UpdateEnvironment();
    }

    //Met à jour le compteur de temps heures:minutes en fonction 
    public void UpdateTimeDisplay(float cycleProgressionInPercent)
    {
        
    }

    //Met à jour l'afficheur jour/nuit 
    public void UpdateDayNightDisplay(float cycleProgressionInPercent)
    {
        dayNightDisplay.transform.rotation = Quaternion.Euler(new Vector3(0,0,90f-(cycleProgressionInPercent*3.6f)));
    }


/* Systeme obsoléte de coroutine, il déforme un peu le temps mais est plus optimisé
    IEnumerator updateCycleProgression(float recurence)
    {
        yield return new WaitForSeconds(recurence);

        if (cycleProgression < cycleDuration)
        {
            cycleProgression+= recurence;
        } else
        {
            cycleProgression = 0;
            AddCycle();
        }

        //Update des timers pour les mises à jours de visuels
        timeBetweenUpdateForSkyboxCount+= recurence;
        timeBetweenUpdateForLightsCount+= recurence;
        timeBetweenUpdateForDayNightDisplayCount+= recurence;

        //Mises à jour des visuels
        float cycleProgressionInPercent = (cycleProgression / (float)cycleDuration) * 100f;
        if (timeBetweenUpdateForDayNightDisplayCount >= timeBetweenUpdateForDayNightDisplay)
        {
            UpdateDayNightDisplay(cycleProgressionInPercent);
            timeBetweenUpdateForDayNightDisplayCount = 1;
        }

        if (timeBetweenUpdateForLightsCount > timeBetweenUpdateForLights)
        {
            UpdateLights(cycleProgressionInPercent);
            timeBetweenUpdateForLightsCount = 1;
        }

        if (timeBetweenUpdateForSkyboxCount > timeBetweenUpdateForSkybox)
        {
            UpdateSkybox(cycleProgressionInPercent);
            timeBetweenUpdateForSkyboxCount = 1;
        }

        timeCoroutine = StartCoroutine(updateCycleProgression(recurence));
        yield return null;
    }
    */
}
