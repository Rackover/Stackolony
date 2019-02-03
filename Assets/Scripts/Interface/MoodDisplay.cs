using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class MoodDisplay : MonoBehaviour {

    public Population population;
    public RawImage face;
    public Image gauge;
    public Image dragHitbox;
    public Text homeless;
    public Text amount;
    public Animator animator;
    public RectTransform rect;
    public AspectRatioFitter fitter;
    public Tooltip tooltip;

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
    public float angle = 200f;
    public float rotationSpeed = 0f;
    public float cameraDistance = 2f;
    public float FOV = 30f;

    bool isDragging = false;
    float dragMargin;
    Displayer preview;
    Options playerOptions;
    PopulationManager popMan;
    Localization loc;
    string moodString;
    Bystander.Mood currentMood;

    private void Start()
    {
        dragMargin = transform.parent.gameObject.GetComponent<VerticalLayoutGroup>().spacing;
    }

    public void InitializeForPopulation(Population pop)
    {
        population = pop;
        loc = GameManager.instance.localization;
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

    void UpdateMood()
    {
        float moodValue = popMan.GetAverageMood(population) / popMan.maxMood;
        gauge.fillAmount = moodValue;
        gauge.color = Color.Lerp(from, to, moodValue);

        Action<Bystander.Mood> changeMood;

        if (playerOptions.GetBool("animatedCitizens")) {
            if (preview == null) {
                preview = GameManager.instance.displayerManager.SetRotationFeed(population.prefab, face, angle, rotationSpeed, cameraDistance, FOV, 128);
            }
            changeMood = (x) => {
                preview.GetModel().GetComponent<Bystander>().SetEmotion(x);
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
            currentMood = Bystander.Mood.Angry;
        }

        else if (moodValue >= happyThreshold) {
            currentMood = Bystander.Mood.Good;
        }

        else {
            currentMood = Bystander.Mood.Bad;
        }

        changeMood(currentMood);
        moodString = loc.GetLine(currentMood.ToString().ToLower(), "mood");

    }

    void UpdateTexts()
    {
        int inhabitants = popMan.populations[population].citizens.Count;
        int homelessAmount = popMan.GetHomelessCount(population);
        amount.text = inhabitants.ToString();
        homeless.text = homelessAmount.ToString();

        if (homelessAmount <= 0) {
            homeless.color = noHomelessColor;
        }
        else {
            homeless.color = homelessColor;
        }

        float moodValue = popMan.GetAverageMood(population) / popMan.maxMood;
        string popName = loc.GetLine(population.codeName, "populationType");

        tooltip.ClearLines();
        tooltip.AddLocalizedLine(new Localization.Line("populationType", population.codeName));
        tooltip.AddLocalizedLine(
            new Localization.Line(
                "stats",
                "moodValue",
                popName+"s",
                moodString.ToUpper()
            )
        );
    }

    public void Show()
    {
        isHidden = false;
        animator.Play("Unfold");
        dragHitbox.enabled = true;
    }

    public void Hide()
    {
        isHidden = true;
        animator.Play("Fold");
        dragHitbox.enabled = false;
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

    private void Update()
    {
        if (isDragging) {
            if (Input.mousePosition.y + rect.sizeDelta.y + dragMargin < rect.position.y) {
                ChangeChildOrder(+1);
            }
            else if (Input.mousePosition.y - dragMargin > rect.position.y) {
                ChangeChildOrder(-1);
            }
        }

        popMan.ChangePopulationPriority(population, transform.GetSiblingIndex());
    }

    void ChangeChildOrder(int by)
    {
        int index = transform.GetSiblingIndex();
        
        if (by < 0 && index <= 0) return;
        if (by > 0 && index >= transform.parent.childCount) return;

        transform.SetSiblingIndex(index + by);
    }

    public void BeginDrag()
    {
        isDragging = true;
    }

    public void StopDrag()
    {
        isDragging = false;
    }
}
