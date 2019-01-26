using System.Collections;
using System.Collections.Generic;
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

    public Color from = Color.green;
    public Color to = Color.red;
    public float colorBlendAmount = 0.5f;
    public bool isHidden = false;

    public void InitializeForPopulation(Population pop)
    {
        population = pop;
        Displayer preview = GameManager.instance.displayerManager.SetRotationFeed(population.prefab, face, 200f, 0, 1.5f, 30, 128);
        preview.cam.backgroundColor = new Color(0, 0, 0, 0);
    }

    public void UpdateDisplay()
    {
        PopulationManager popMan = GameManager.instance.populationManager;

        float moodValue = popMan.GetAverageMood(population);
        gauge.fillAmount = moodValue;
        gauge.color = Color.Lerp(from, to, moodValue);

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
