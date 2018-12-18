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
    public float timeBetweenUpdateForSystem; //Combien de secondes entre chaque mise à jour du systeme (Utilisé par les flags "WorkingHour"

    public Gradient skyboxVariation; //Variations de couleur de la skybox au fil du temps

    [Header("=== REFERENCES ===")][Space(1)]
    public GameObject directionalLight;
    public Material skyboxMaterial;

    [Header("=== DEBUG VALUES ===")][Space(1)]
    [HideInInspector] public int cycleNumber; //Combien de cycles se sont ecoulés en tout
    [HideInInspector] public int yearNumber; //Combien d'années se sont ecoulées en tout
    public float cycleProgression; //Combien de secondes se sont ecoulées dans le cycle actuel
    public int timeScale; //Coefficient de vitesse d'écoulement du temps
    //private Coroutine timeCoroutine; //Coroutine pour gérer la progression des cycles, obsoléte

    private float timeBetweenUpdateForSkyboxCount = 0;
    private float timeBetweenUpdateForLightsCount = 0;
    private float timeBetweenUpdateForDayNightDisplayCount = 0;
    private float timeBetweenUpdateForSystemCount = 0;

    private float recurence = 0; //Correspond au temps entre chaques fois ou on verifiera pour faire une mise à jour visuelle

    private int savedTimeScale; //Variable utilisée pour redéfinir la vitesse du jeu quand le joueur annule la pause
    private Image savedButton; //Bouton à réactiver quand le joueur annule la pause
    public float cycleProgressionInPercent;

    private float counter;

    public void Start()
    {
        counter = 0;
        timeScale = initialTimeScale;
        recurence = Mathf.Min(timeBetweenUpdateForDayNightDisplay, timeBetweenUpdateForLights, timeBetweenUpdateForSkybox, timeBetweenUpdateForSystem);
        cycleNumber = 0;
        yearNumber = 0;
        GameManager.instance.temporalityInterface.UpdateCycleText(cycleNumber, yearNumber);
        GameManager.instance.temporalityInterface.EnableButton(GameManager.instance.temporalityInterface.defaultButton);
        
        //timeCoroutine = StartCoroutine(updateCycleProgression(recurence));
        // ChangeTimeScale(1);    
        
        PauseGame();
    }

    public void PauseGame()
    {
        if (timeScale == 0) {
            timeScale = savedTimeScale;
            GameManager.instance.temporalityInterface.EnableButton(savedButton);
        }
        else
        {
            GameManager.instance.temporalityInterface.EnableButton(GameManager.instance.temporalityInterface.defaultButton);
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

        if (Input.GetKeyDown(KeyCode.C))
        {
            AddCycle();
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
        cycleProgressionInPercent = (cycleProgression / (float)cycleDuration) * 100f;
        if (timeBetweenUpdateForDayNightDisplayCount >= timeBetweenUpdateForDayNightDisplay)
        {
            GameManager.instance.temporalityInterface.UpdateDayNightDisplay(cycleProgressionInPercent);
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

        if (timeBetweenUpdateForSystemCount > timeBetweenUpdateForSystem) {
             UpdateSystem();
        }
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
        cycleNumber++;
        GameManager.instance.temporalityInterface.UpdateCycleText(cycleNumber, yearNumber);
        if (cycleNumber%yearDuration == 0) 
            AddYear();
        GameManager.instance.deliveryManagement.DeliverBlocks();
        GameManager.instance.systemReferences.UpdateCycle();
    }

    public void AddYear() //Ajoute un an au compteur
    {
        yearNumber++;
        GameManager.instance.temporalityInterface.UpdateCycleText(cycleNumber, yearNumber);
    }

    //Met à jour les lumières en fonction de l'avancement du cycle
    public void UpdateLights(float cycleProgressionInPercent)
    {
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3((3.6f * (float)cycleProgressionInPercent),30,0));
    }

    //Met à jour la skybox en fonction de l'avancement du cycle
    public void UpdateSkybox(float cycleProgressionInPercent)
    {
        skyboxMaterial.SetColor("_Tint", skyboxVariation.Evaluate(cycleProgressionInPercent/100f));
        RenderSettings.skybox = skyboxMaterial;
        DynamicGI.UpdateEnvironment();
    }

    public void UpdateSystem() {
        GameManager.instance.systemReferences.CheckWorkingHours();
    }
}
