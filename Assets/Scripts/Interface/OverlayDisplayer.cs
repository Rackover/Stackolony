using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayDisplayer : MonoBehaviour {

    public GameObject overlayButtonExample;
    public Animator animator;

    public List<Image[]> buttons = new List<Image[]>();

    bool isFolded = true;
    OverlayManager overMan;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        int index = 0;
        overMan = GameManager.instance.overlayManager;
        foreach (KeyValuePair<OverlayType, OverlayManager.IOverlay> overPair in overMan.overlays)
        {

            OverlayType overType = overPair.Key;
            OverlayManager.IOverlay over = overPair.Value;

            GameObject overlayButton = Instantiate(overlayButtonExample, transform);
            Button overlayButtonComponent = overlayButton.GetComponent<Button>();

            int value = index;
            overlayButtonComponent.onClick.AddListener(delegate {
                overMan.SelectOverlay(overType);
                Select(value);
            });

            overlayButton.GetComponent<Tooltip>().AddLocalizedLine(
                new Localization.Line("overlay", over.codeName)
            );

            buttons.Add(overlayButton.GetComponentsInChildren<Image>());

            overlayButton.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = over.sprite;

            index++;
        }
        Destroy(overlayButtonExample);
        Reset();
    }

    void Reset()
    {
        foreach(Image[] b in buttons)
        {
            foreach(Image i in b) 
            {
                i.color = overMan.OverlayUnselectedColor;
            }
        }
    }

    void Select(int id)
    {
        Reset();
        foreach(Image i in buttons[id]) 
        {
            i.color = overMan.OverlaySelectedColor;
        }
    }

    public void Unfold()
    {
        animator.Play("Unfold");
        isFolded = false;
        Reset();
    }

    public void Fold()
    {
        animator.Play("Fold");
        isFolded = true;
    }

    public void Toggle()
    {
        if (isFolded) {
            Unfold();
        }
        else {
            Fold();
        }
    }

}
