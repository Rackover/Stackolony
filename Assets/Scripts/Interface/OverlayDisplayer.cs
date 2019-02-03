using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayDisplayer : MonoBehaviour {

    public GameObject overlayButtonExample;
    public Animator animator;

    bool isFolded = true;
    OverlayManager overMan;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        overMan = GameManager.instance.overlayManager;
        foreach (KeyValuePair<OverlayType, OverlayManager.IOverlay> overPair in overMan.overlays) {
            OverlayType overType = overPair.Key;
            OverlayManager.IOverlay over = overPair.Value;

            GameObject overlayButton = Instantiate(overlayButtonExample, transform);
            Button overlayButtonComponent = overlayButton.GetComponent<Button>();
            overlayButtonComponent.onClick.AddListener(delegate {
                overMan.SelectOverlay(overType);
            });

            overlayButton.GetComponent<Tooltip>().AddLocalizedLine(
                new Localization.Line("overlay", over.codeName)
             );

            overlayButton.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = over.sprite;
        }

        Destroy(overlayButtonExample);
    }

    public void Unfold()
    {
        animator.Play("Unfold");
        isFolded = false;
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
