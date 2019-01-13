using UnityEngine;
using UnityEngine.UI;

public class Temporality : MonoBehaviour {


    [Header("=== SETTINGS ===")][Space(1)]
    public int cycleDuration; //Durée d'un cycle en secondes
    public int yearDuration; //Durée d'une année en cycle

    [Header("=== DEBUG VALUES ===")][Space(1)]
    [HideInInspector] public int cycleNumber = 0; //Combien de cycles se sont ecoulés en tout
    public float cycleProgression; //Combien de secondes se sont ecoulées dans le cycle actuel
    public int timeScale; //Coefficient de vitesse d'écoulement du temps

    private int savedTimeScale; //Variable utilisée pour redéfinir la vitesse du jeu quand le joueur annule la pause
    private Image savedButton; //Bouton à réactiver quand le joueur annule la pause

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
    
    //Met à jour le temps
    void Update() {
        if (cycleProgression < cycleDuration)
        {
            cycleProgression+= timeScale*Time.deltaTime;
        }
        else
        {
            cycleProgression = 0f; //On ne reset pas à 0 pour éviter une transition sacadée au niveau de l'aperçu du temps passé
            AddCycle();
        }

         UpdateSystem();
    }

    public float GetCurrentCycleProgression()
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
