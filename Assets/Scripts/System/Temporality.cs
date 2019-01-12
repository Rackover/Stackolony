using UnityEngine;
using UnityEngine.UI;

public class Temporality : MonoBehaviour {


    [Header("=== SETTINGS ===")][Space(1)]
    public int cycleDuration; //Durée d'un cycle en secondes
    public int yearDuration; //Durée d'une année en cycle
    public float timeBetweenUpdateForSkybox; //Combien de secondes entre chaque mise à jour visuelle de la skybox
    public float timeBetweenUpdateForLights; //Combien de secondes entre chaque mise à jour visuelle des lumières
    public float timeBetweenUpdateForDayNightDisplay; //Combien de secondes entre chaque mise à jour visuelle de l'afficheur "jour / nuit"
    public float timeBetweenUpdateForSystem; //Combien de secondes entre chaque mise à jour du systeme (Utilisé par les flags "WorkingHour"

    [Header("=== DEBUG VALUES ===")][Space(1)]
    [HideInInspector] public int cycleNumber = 0; //Combien de cycles se sont ecoulés en tout
    public float cycleProgression; //Combien de secondes se sont ecoulées dans le cycle actuel
    public int timeScale; //Coefficient de vitesse d'écoulement du temps

    private float timeBetweenUpdateForSkyboxCount = 0;
    private float timeBetweenUpdateForLightsCount = 0;
    private float timeBetweenUpdateForDayNightDisplayCount = 0;
    private float timeBetweenUpdateForSystemCount = 0;

    private float recurence = 0; //Correspond au temps entre chaques fois ou on verifiera pour faire une mise à jour visuelle

    private int savedTimeScale; //Variable utilisée pour redéfinir la vitesse du jeu quand le joueur annule la pause
    private Image savedButton; //Bouton à réactiver quand le joueur annule la pause

    private float counter;

    public void Start()
    {
        counter = 0;
        recurence = Mathf.Min(timeBetweenUpdateForDayNightDisplay, timeBetweenUpdateForLights, timeBetweenUpdateForSkybox, timeBetweenUpdateForSystem);
    }

    public void PauseGame()
    {
        if (timeScale == 0) {
            timeScale = savedTimeScale;
        }
        else
        {
            savedTimeScale = timeScale;
            timeScale = 0;

            Color32 colorOfEnabledButton = new Color32(255, 255, 255, 255);
            foreach (GameObject child in transform)
            {
                Color32 colorToCompare = child.GetComponent<Image>().color;
                if (colorOfEnabledButton.Equals(colorToCompare))
                {
                    savedButton = child.GetComponent<Image>();
                }
            }
        }
    }

    public void Update() {
        //Debug.Log(Time.deltaTime * 1);
        counter += Time.deltaTime;
        if (counter >= recurence) {
            counter = 0;
            TimeUpdate();
        }
    }

    //Met à jour le temps et les visuels
    public void TimeUpdate() {
        if (cycleProgression < cycleDuration)
        {
            cycleProgression+= recurence * timeScale;
        } else
        {
            cycleProgression = recurence * timeScale; //On ne reset pas à 0 pour éviter une transition sacadée au niveau de l'aperçu du temps passé
            AddCycle();
        }

        //Update des timers pour les mises à jours de visuels
        timeBetweenUpdateForSkyboxCount+= recurence;
        timeBetweenUpdateForLightsCount+= recurence;
        timeBetweenUpdateForDayNightDisplayCount+= recurence;
        timeBetweenUpdateForSystemCount+= recurence;

        //Mises à jour des visuels
        if (timeBetweenUpdateForDayNightDisplayCount >= timeBetweenUpdateForDayNightDisplay)
        {
            timeBetweenUpdateForDayNightDisplayCount = 1;
        }

        if (timeBetweenUpdateForLightsCount > timeBetweenUpdateForLights)
        {
            timeBetweenUpdateForLightsCount = 1;
        }

        if (timeBetweenUpdateForSkyboxCount > timeBetweenUpdateForSkybox)
        {
            timeBetweenUpdateForSkyboxCount = 1;
        }

        if (timeBetweenUpdateForSystemCount > timeBetweenUpdateForSystem) {
             UpdateSystem();
        }
    }

    public float GetCurrentcycleProgression()
    {
        return (cycleProgression / (float)cycleDuration) * 100f;
    }

    public void ChangeTimeScale(int newTimeScaleCoef)
    {
        if (newTimeScaleCoef > timeScale)
        {
            GameManager.instance.sfxManager.PlaySound("IncreaseSpeed");
        } else if (newTimeScaleCoef < timeScale)
        {
            GameManager.instance.sfxManager.PlaySound("DecreaseSpeed");
        }
        timeScale = newTimeScaleCoef;
    }

    public void AddCycle() //Ajoute un cycle au compteur
    {
        if (!GameManager.instance.IsInGame()) { return; };

        GameManager.instance.deliveryManagement.DeliverBlocks();
        GameManager.instance.systemManager.UpdateCycle();
    }

    // Returns current cycle of the current year
    public int GetCycle()
    {
        return (cycleNumber % yearDuration)+1;
    }

    // Returns current year
    public int GetYear()
    {
        return (int)Mathf.Ceil(cycleNumber / yearDuration);
    }

    public void UpdateSystem()
    {
        if (!GameManager.instance.IsInGame()) { return; };
        GameManager.instance.systemManager.CheckWorkingHours();
    }
}
