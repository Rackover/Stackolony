﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TemporalityInterface : MonoBehaviour {

    [Header("=== REFERENCES ===")]
    [Space(1)]
    public Text timeText;
    public GameObject timescaleButtonHolder;
    public Button pauseButton;
    public Button playButton;
    public Button playFasterButton;

    int currentSpeed;

    public void UpdateCycleText(int minutes, int hours, int currentCycle, int currentYear)
    {
        timeText.text = string.Format("{0:00}", hours) + ":" + string.Format("{0:00}", minutes) + " - " + currentCycle + "." + currentYear;
        Tooltip tt = timeText.GetComponent<Tooltip>();
        tt.ClearLines();
        tt.AddLocalizedLine(new Localization.Line("hud", "temporality", currentCycle.ToString(), currentYear.ToString()));
    }

    void PlayTimeSound(int speed)
    {
        if(speed < currentSpeed)
        {
            GameManager.instance.soundManager.Play("DecreaseSpeed");
        }
        else if(speed > currentSpeed)
        {
            GameManager.instance.soundManager.Play("IncreaseSpeed");
        }

        currentSpeed = speed;
    }

    public void EnableButtonsExcept(Button button)
    {
        foreach (Transform child in timescaleButtonHolder.transform)
        {
            Button timeButton = child.GetComponent<Button>();
            timeButton.interactable = true;
        }
        button.interactable = false;
    }

    public void PauseTime()
    {
        Temporality temporality = GameManager.instance.temporality;
        temporality.SetTimeScale(0);
        EnableButtonsExcept(pauseButton);

        PlayTimeSound(0);
    }

    public void PlayTime()
    {
        Temporality temporality = GameManager.instance.temporality;
        temporality.SetTimeScale(1);
        EnableButtonsExcept(playButton);

        PlayTimeSound(1);
    }

    public void PlayTimeFaster()
    {
        Temporality temporality = GameManager.instance.temporality;
        temporality.SetTimeScale(2);
        EnableButtonsExcept(playFasterButton);

        PlayTimeSound(2);
    }

    // Makes sure the temporality interface looks normal by disabling
    // one button (the one of the current temporality) : play, pause or playfaster
    void DisableOneButton()
    {
        Temporality temporality = GameManager.instance.temporality;
        bool oneAtLeastIsDisabled = false;
        foreach (Transform child in timescaleButtonHolder.transform) {
            Button timeButton = child.GetComponent<Button>();
            if (!timeButton.interactable) {
                oneAtLeastIsDisabled = true;
            }
        }
        if (!oneAtLeastIsDisabled) {
            switch (temporality.timeScale) {
                default: PlayTimeFaster(); break;
                case 0: PauseTime(); break;
                case 1: PlayTime(); break;
            }
        }
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

        DisableOneButton();

        yield return new WaitForSeconds(FindObjectOfType<Interface>().refreshRate);
        yield return StartCoroutine(UpdateInterface());
    }
}
