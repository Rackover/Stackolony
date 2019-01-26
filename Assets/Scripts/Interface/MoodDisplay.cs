using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class MoodDisplay : MonoBehaviour {

    public Population population;
    public RawImage face;
    public Image gauge;
    public Text homeless;
    public Text amount;
    public Animator animator;
    public RectTransform rect;
    public AspectRatioFitter fitter;

    public Color notificationColor = Color.blue;
    public Color from = Color.green;
    public Color to = Color.red;
    public float colorBlendAmount = 0.5f;
    public bool isHidden = false;

    public Color homelessColor = Color.red;
    public Color noHomelessColor = Color.grey;
    public Color blinkColor = Color.grey;

    public float angryThreshold = 0.3f;
    public float happyThreshold = 0.7f;
    public float blinkSpeed = 0.33f;
    public float blinkLength = 2f;

    Displayer preview;
    Options playerOptions;
    PopulationManager popMan;

    public void InitializeForPopulation(Population pop)
    {
        population = pop;
        Localization loc = GameManager.instance.localization;
        popMan = GameManager.instance.populationManager;
        popMan.CitizenArrival += (amount, popType) => {
            if (popType == pop) {
                // Notify
                FindObjectOfType<Notifications>().Notify(
                    new Notifications.Notification(
                        "newPeople",
                        notificationColor,
                        new string[] { amount.ToString(), loc.GetLine(popType.codeName, "populationType") }
                    )
                );

                // Animation
                StartCoroutine(Blink(blinkLength));
                UpdateTexts();
            }
        };
    }

    public void UpdateDisplay()
    {
        playerOptions = GameManager.instance.player.options;

        UpdateMood();
        UpdateTexts();
    }

    public void UpdateMood()
    {
        float moodValue = popMan.GetAverageMood(population) / popMan.maxMood;
        gauge.fillAmount = moodValue;
        gauge.color = Color.Lerp(from, to, moodValue);

        Action<Citizen.Mood> changeMood;

        if (playerOptions.GetBool("animatedCitizens")) {
            if (preview == null) {
                preview = GameManager.instance.displayerManager.SetRotationFeed(population.prefab, face, 200f, 0, 2f, 30, 128);
            }
            changeMood = (x) => {
                preview.GetModel().GetComponent<Citizen>().SetEmotion(x);
                fitter.aspectRatio = 1f;
            };
        }
        else {
            if (preview != null) {
                preview.Unstage();
                preview = null;
            }
            changeMood = (x) => {
                face.texture = population.moodSprites[(int)x].texture;
                fitter.aspectRatio = population.moodSprites[0].rect.width / population.moodSprites[0].rect.height;
            };
        }

        // Emotion based on humor
        if (moodValue <= angryThreshold) {
            changeMood(Citizen.Mood.Angry);
        }

        else if (moodValue >= happyThreshold) {
            changeMood(Citizen.Mood.Good);
        }

        else {
            changeMood(Citizen.Mood.Bad);
        }

    }

    public void UpdateTexts()
    {
        int inhabitants = popMan.populationCitizenList[population].Count;
        int homelessAmount = popMan.GetHomelessCount(population);
        amount.text = inhabitants.ToString();
        homeless.text = homelessAmount.ToString();

        if (homelessAmount <= 0) {
            homeless.color = noHomelessColor;
        }
        else {
            homeless.color = homelessColor;
        }
    }

    public void Show()
    {
        isHidden = false;
        animator.Play("Unfold");
    }

    public void Hide()
    {
        isHidden = true;
        animator.Play("Fold");
    }

    IEnumerator Blink(float seconds)
    {
        if (seconds <= 0f) {
            amount.color = Color.white;
            yield return null;
        }
        else {
            amount.color = amount.color == blinkColor ? Color.white : blinkColor;
            yield return new WaitForSeconds(blinkSpeed);
            yield return StartCoroutine(Blink(seconds - blinkSpeed));
        }
    }


}
