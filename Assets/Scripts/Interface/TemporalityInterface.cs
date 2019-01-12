using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TemporalityInterface : MonoBehaviour {

    [Header("=== REFERENCES ===")]
    [Space(1)]
    public Text timeText;
    public GameObject dayNightDisplay;
    public GameObject timescaleButtonHolder;
    public Image pauseButton;
    public Image defaultButton;

    public void UpdateCycleText(int currentCycle, int currentYear)
    {
        timeText.text = "Cycle : " + currentCycle + " - Year : " + currentYear;
    }

    public void EnableButton(Image button)
    {
        foreach (Transform child in timescaleButtonHolder.transform)
        {
            child.GetComponent<Image>().color = new Color32(255, 255, 255, 55);
        }
        button.color = new Color32(255, 255, 255, 255);
    }

    public void PauseTime()
    {
        Temporality temporality = GameManager.instance.temporality;
        temporality.ChangeTimeScale(0);
    }

    public void PlayTime()
    {
        Temporality temporality = GameManager.instance.temporality;
        temporality.ChangeTimeScale(1);
    }

    public void PlayTimeFaster()
    {
        Temporality temporality = GameManager.instance.temporality;
        temporality.ChangeTimeScale(2);
    }

    public void PlayTimeFastest()
    {
        Temporality temporality = GameManager.instance.temporality;
        temporality.ChangeTimeScale(4);
    }

    //Met à jour l'afficheur jour/nuit 
    public void UpdateDayNightDisplay(float cycleProgressionInPercent)
    {
        dayNightDisplay.transform.rotation = Quaternion.Euler(new Vector3(0, 0,90 -(cycleProgressionInPercent * 3.6f)));
    }


    public IEnumerator UpdateInterface()
    {
        Temporality temporality = GameManager.instance.temporality;
        UpdateCycleText(temporality.GetCycle(), temporality.GetYear());
        UpdateDayNightDisplay(temporality.cycleProgression);
        yield return new WaitForSeconds(FindObjectOfType<Interface>().refreshRate);
        yield return StartCoroutine(UpdateInterface());
    }
}
