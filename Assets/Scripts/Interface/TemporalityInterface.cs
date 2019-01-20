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

    public void UpdateCycleText(int minutes, int hours, int currentCycle, int currentYear)
    {

        timeText.text = hours + ":" + minutes + " - " + currentCycle + "." + currentYear;
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
        temporality.SetTimeScale(0);
    }

    public void PlayTime()
    {
        Temporality temporality = GameManager.instance.temporality;
        temporality.SetTimeScale(1);
    }

    public void PlayTimeFaster()
    {
        Temporality temporality = GameManager.instance.temporality;
        temporality.SetTimeScale(2);
    }

    public void PlayTimeFastest()
    {
        Temporality temporality = GameManager.instance.temporality;
        temporality.SetTimeScale(4);
    }

    //Met à jour l'afficheur jour/nuit 
    public void UpdateDayNightDisplay(float cycleProgressionInPercent)
    {
        dayNightDisplay.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0, 0, (90 - (cycleProgressionInPercent * 3.6f))%360));
    }


    public IEnumerator UpdateInterface()
    {
        Temporality temporality = GameManager.instance.temporality;

        int minutes = Mathf.FloorToInt(
            ((0.2f+temporality.GetCurrentCycleProgression()/100)%1)
        * 24 * 60);
        int hours = Mathf.FloorToInt(minutes / 60);
        minutes -= hours * 60;

        UpdateCycleText(minutes, hours, temporality.GetCycle(), temporality.GetYear());
       // UpdateDayNightDisplay(temporality.GetCurrentCycleProgression());
        yield return new WaitForSeconds(FindObjectOfType<Interface>().refreshRate);
        yield return StartCoroutine(UpdateInterface());
    }
}
