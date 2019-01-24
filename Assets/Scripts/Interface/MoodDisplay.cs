using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MoodDisplay : MonoBehaviour {

    public Population population;
    public Image face;
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
        face.sprite = pop.humorSprite;
        face.color = Color.Lerp(Color.white, pop.color, colorBlendAmount);
        face.color = new Color(face.color.r, face.color.g, face.color.b, 1f);
        population = pop;
    }

    public void UpdateDisplay()
    {
        float moodValue = GameManager.instance.populationManager.GetAverageMood(population);
        gauge.fillAmount = moodValue;
        gauge.color = Color.Lerp(from, to, moodValue);
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
