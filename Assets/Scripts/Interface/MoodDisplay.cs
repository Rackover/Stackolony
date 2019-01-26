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

    public Color from = Color.green;
    public Color to = Color.red;
    public float colorBlendAmount = 0.5f;
    public bool isHidden = false;

    public float angryThreshold = 0.3f;
    public float happyThreshold = 0.7f;

    Displayer preview;
    Options playerOptions;

    public void InitializeForPopulation(Population pop)
    {
        population = pop;
    }

    public void UpdateDisplay()
    {
        playerOptions = GameManager.instance.player.options;
        PopulationManager popMan = GameManager.instance.populationManager;
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

        amount.text = popMan.populationCitizenList[population].Count.ToString();
        homeless.text = popMan.GetHomelessCount(population).ToString();
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
}
