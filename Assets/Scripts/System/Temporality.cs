﻿using UnityEngine;
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

    float cycleProgression; //Combien de secondes se sont ecoulées dans le cycle actuel
    int savedTimeScale; //Variable utilisée pour redéfinir la vitesse du jeu quand le joueur annule la pause
    Image savedButton; //Bouton à réactiver quand le joueur annule la pause

    public System.Action OnNewCycle;
    public System.Action OnNewMicroCycle;

    void Start()
    {
        GetMicroDuration();
    }

    void GetMicroDuration()
    {
        microDuration = cycleDuration / nbMicroCyclePerCycle;
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
    
    //Met à jour le temps
    void Update() {
        if (cycleProgression < cycleDuration)
        {
            if (cycleProgression >= microDuration)
            {
                AddMicroCycle();
                microDuration+= microDuration;
            }
            cycleProgression+= timeScale*Time.deltaTime;
        }
        else
        {
            cycleProgression = 0f; //On ne reset pas à 0 pour éviter une transition sacadée au niveau de l'aperçu du temps passé
            AddCycle();
            GetMicroDuration();
        }
    }

    public float GetCurrentCycleProgression()
    {
        return (cycleProgression / (float)cycleDuration) * 100f;
    }

    public void SetTimeScale(int newTimeScaleCoef)
    {
        if (newTimeScaleCoef > timeScale)
        {
            GameManager.instance.soundManager.Play("IncreaseSpeed");
        } else if (newTimeScaleCoef < timeScale)
        {
            GameManager.instance.soundManager.Play("DecreaseSpeed");
        }
        timeScale = newTimeScaleCoef;
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
        GameManager.instance.bulletinsManager.Renew(cycleNumber);

        OnNewCycle.Invoke();
    }

    void AddMicroCycle()
    {
        if (!GameManager.instance.IsInGame()) { return; };
        StartCoroutine(GameManager.instance.systemManager.OnNewMicrocycle());

        OnNewMicroCycle.Invoke();
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