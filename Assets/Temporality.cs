using System.Collections;
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


    [Header("=== DEBUG VALUES ===")][Space(1)]
    private int cycleNumber; //Combien de cycles se sont ecoulés en tout
    private int yearNumber; //Combien d'années se sont ecoulées en tout
    public float cycleProgression; //Combien de secondes se sont ecoulées dans le cycle actuel
    public static int timeScale; //Coefficient de vitesse d'écoulement du temps
    private Coroutine timeCoroutine; //Coroutine pour gérer la progression des cycles

    private float timeBetweenUpdateForSkyboxCount = 0;
    private float timeBetweenUpdateForLightsCount = 0;
    private float timeBetweenUpdateForDayNightDisplayCount = 0;

    public void Awake()
    {
        timeScale = initialTimeScale;
        float recurence = Mathf.Min(timeBetweenUpdateForDayNightDisplay, timeBetweenUpdateForLights, timeBetweenUpdateForSkybox);
        timeCoroutine = StartCoroutine(updateCycleProgression(recurence));
    }

    public void ChangeTimeScale(int newTimeScaleCoef)
    {
        timeScale = newTimeScaleCoef;
    }

    public void AddCycle() //Ajoute un cycle au compteur
    {

    }

    public void AddYear() //Ajoute un an au compteur
    {

    }

    //Met à jour les lumières en fonction de l'avancement du cycle
    public void UpdateLights(float cycleProgressionInPercent)
    {
        Debug.Log((360f * 1 / 100));
        Debug.Log(cycleProgressionInPercent);
        Debug.Log(cycleProgression);
        Debug.Log(((360f * (float)cycleProgressionInPercent) / 100f));
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

    }

    IEnumerator updateCycleProgression(float recurence)
    {
        yield return new WaitForSeconds(recurence);

        if (cycleProgression < cycleDuration)
        {
            cycleProgression+= recurence;
        } else
        {
            cycleProgression = 1;
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
}
