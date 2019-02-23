using UnityEngine;
using UnityEngine.UI;

public class Temporality : MonoBehaviour {


    [Header("=== SETTINGS ===")][Space(1)]
    public int cycleDuration; //Durée d'un cycle en secondes
    public int yearDuration; //Durée d'une année en cycle

    [Header("=== TIME VALUES ===")][Space(1)]
    public int cycleNumber = 0; //Combien de cycles se sont ecoulés en tout
    public int timeScale; //Coefficient de vitesse d'écoulement du temps
    public int nbMicroCyclePerCycle = 3; //Combien de micro cycles s'écoulent le temps d'un cycle
    private float microDuration;
    private int microCounter;

    public float cycleProgression; //Combien de secondes se sont ecoulées dans le cycle actuel
    int savedTimeScale; //Variable utilisée pour redéfinir la vitesse du jeu quand le joueur annule la pause
    int lastYear = 1;

    void Start()
    {
        GetMicroDuration();
        microCounter = 1;
        SetTimeScale(1);
    }

    void GetMicroDuration()
    {
        microDuration = (float)cycleDuration / (float)nbMicroCyclePerCycle;
    }

    public float GetMicroCoef()
    {
        return 1f / (float)nbMicroCyclePerCycle;
    }

    public void PauseTime()
    {
        if (timeScale == 0) {
            timeScale = savedTimeScale;
        }
        else
        {
            savedTimeScale = timeScale;
            timeScale = 0;
        }
    }
    
    //Met à jour le temps
    void Update() {
        if (cycleProgression < cycleDuration)
        {
            if (cycleProgression >= microDuration*microCounter)
            {
                AddMicroCycle();
            }
            cycleProgression+= timeScale*Time.deltaTime;
        }
        else
        {
            cycleProgression = 0f; 
            AddCycle();
            if (lastYear < GetYear()) {
                lastYear = GetYear();
                GameManager.instance.bulletinsManager.Renew(cycleNumber);
            }
            GetMicroDuration();
        }
    }

    public float GetCurrentCycleProgression()
    {
        return (cycleProgression / (float)cycleDuration) * 100f;
    }

    public void SetTimeScale(int newTimeScaleCoef)
    {
        timeScale = newTimeScaleCoef;
        GameManager.instance.roamerManager.ChangeTimeScale(newTimeScaleCoef);
    }

    public void SetDate(int cycles)
    {
        cycleNumber = cycles;
    }

    public void SetTimeOfDay(float percents)
    {
        cycleProgression = (cycleDuration * percents) / 100;
    }

    public void AddCycle() //Ajoute un cycle au compteur
    {
        if (!GameManager.instance.IsInGame()) { return; };
        StartCoroutine(GameManager.instance.systemManager.OnNewCycle());
        cycleNumber++;
        microCounter = 1;
        GameManager.instance.timelineController.UpdateCycle(cycleNumber);
        GameManager.instance.eventManager.Renew(cycleNumber);
    }

    public void AddMicroCycle()
    {
        if (!GameManager.instance.IsInGame()) { return; };
        microCounter++;
        StartCoroutine(GameManager.instance.systemManager.OnNewMicrocycle());
    }

    // Returns current cycle of the current year
    public int GetCycle()
    {
        return (cycleNumber % yearDuration)+1;
    }

    // Returns current year
    public int GetYear()
    {
        return (int)Mathf.Ceil(cycleNumber / yearDuration)+1;
    }
}