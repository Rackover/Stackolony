using UnityEngine;
using UnityEngine.UI;

public class TemporalityInterface : MonoBehaviour {

    [Header("=== REFERENCES ===")]
    [Space(1)]
    public Text cycleNumberText;
    public Text yearNumberText;
    public GameObject dayNightDisplay;
    public GameObject timescaleButtonHolder;
    public Image pauseButton;
    public Image defaultButton;

    public void UpdateCycleText(int cycleNumber, int yearNumber)
    {
        cycleNumberText.text = "Cycle : " + cycleNumber;
        yearNumberText.text = "Year : " + yearNumber;
    }

    public void EnableButton(Image button)
    {
        foreach (Transform child in timescaleButtonHolder.transform)
        {
            child.GetComponent<Image>().color = new Color32(255, 255, 255, 55);
        }
        button.color = new Color32(255, 255, 255, 255);
    }

    //Met à jour l'afficheur jour/nuit 
    public void UpdateDayNightDisplay(float cycleProgressionInPercent)
    {
        dayNightDisplay.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -(cycleProgressionInPercent * 3.6f)));
    }
}
