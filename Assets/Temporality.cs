using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Temporality : MonoBehaviour {

    [Header("=== SETTINGS ===")][Space(1)]
    public int cycleDuration; //Durée d'un cycle en secondes
    public int yearDuration; //Durée d'une année en cycle
    public int initialTimeScale; //Coefficient de vitesse d'écoulement du temps
    public int timeBetweenUpdateForSkybox; //Combien de secondes entre chaque mise à jour visuelle de la skybox
    public int timeBetweenUpdateForLights; //Combien de secondes entre chaque mise à jour visuelle des lumières
    public int timeBetweenUpdateForDayNightDisplay; //Combien de secondes entre chaque mise à jour visuelle de l'afficheur "jour / nuit"

    public Gradient skyboxVariation; //Variations de couleur de la skybox au fil du temps

    [Header("=== REFERENCES ===")][Space(1)]
    public Text cycleNumberText;
    public Text yearNumberText;
    public GameObject directionalLight;


    [Header("=== DEBUG VALUES ===")][Space(1)]
    private int cycleNumber; //Combien de cycles se sont ecoulés en tout
    private int yearNumber; //Combien d'années se sont ecoulées en tout
    public int cycleProgression; //Combien de secondes se sont ecoulées dans le cycle actuel
    public static int timeScale; //Coefficient de vitesse d'écoulement du temps
    private Coroutine timeCoroutine; //Coroutine pour gérer la progression des cycles

    private int timeBetweenUpdateForSkyboxCount = 0;
    private int timeBetweenUpdateForLightsCount = 0;
    private int timeBetweenUpdateForDayNightDisplayCount = 0;

    public void Awake()
    {
        timeScale = initialTimeScale;
      //  timeCoroutine = StartCoroutine(updateCycleProgression());
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
    public void UpdateLights(int cycleProgressionInPercent)
    { 
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3(90+(cycleProgressionInPercent/360),30,0));
    }

    //Met à jour la skybox en fonction de l'avancement du cycle
    public void UpdateSkybox(int cycleProgressionInPercent)
    {
        RenderSettings.skybox.color = skyboxVariation.Evaluate(cycleProgressionInPercent);
    }

    //Met à jour le compteur de temps heures:minutes en fonction 
    public void UpdateTimeDisplay(int cycleProgressionInPercent)
    {

    }

    //Met à jour l'afficheur jour/nuit 
    public void UpdateDayNightDisplay(int cycleProgressionInPercent)
    {

    }

    IEnumerator updateCycleProgression()
    {
        yield return new WaitForSeconds(1 * timeScale);


        if (cycleProgression < cycleDuration)
        {
            cycleProgression++;
        } else
        {
            cycleProgression = 0;
            AddCycle();
        }

        //Update des timers pour les mises à jours de visuels
        timeBetweenUpdateForSkyboxCount++;
        timeBetweenUpdateForLightsCount++;
        timeBetweenUpdateForDayNightDisplayCount++;

        //Mises à jour des visuels
        if (timeBetweenUpdateForDayNightDisplayCount >= timeBetweenUpdateForDayNightDisplay)
        {
            UpdateDayNightDisplay(cycleDuration / cycleProgression);
            timeBetweenUpdateForDayNightDisplayCount = 0;
        }

        if (timeBetweenUpdateForLightsCount > timeBetweenUpdateForLights)
        {
            UpdateLights(cycleDuration / cycleProgression);
            timeBetweenUpdateForLightsCount = 0;
        }

        if (timeBetweenUpdateForSkyboxCount > timeBetweenUpdateForSkybox)
        {
            UpdateSkybox(cycleDuration / cycleProgression);
            timeBetweenUpdateForSkyboxCount = 0;
        }

        timeCoroutine = StartCoroutine(updateCycleProgression());
        yield return null;
    }
}
