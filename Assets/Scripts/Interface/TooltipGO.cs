using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipGO : MonoBehaviour {

    public enum alignmentOptionsHorizontal { RIGHT, LEFT};
    public enum alignmentOptionsVertical { TOP, BOTTOM };

    [Header("=== REFERENCES ===")]
    public TextMeshProUGUI myText;
    public RectTransform myRectTransform;
    public RectTransform myCanvasTransform;

    [Header("Value")]
    public string text;
    public float maxLength; //Taille max pour une ligne (en pixels)

    [Header("Settings")]
    public Vector2 shift;


    [System.Serializable]
    public class TooltipIcon
    {
        public string iconTag; //Tag qui sera remplacé par l'icône
        public Sprite iconSprite; //Icône à utiliser pour remplacer le tag
    }

    public TooltipIcon[] tooltipIcons;

    public IEnumerator RefreshBuildingTooltips()
    {
        foreach (Block block in GameManager.instance.systemManager.AllBlocks) {
            block.UpdateTooltip();
        }
        yield return new WaitForSeconds(FindObjectOfType<Interface>().refreshRate);
        yield return StartCoroutine(RefreshBuildingTooltips());
    }

    public void Disable()
    {
        myText.enabled = false;
        GetComponent<Image>().enabled = false;
    }

    public void Enable()
    {
        myText.enabled = true;
        GetComponent<Image>().enabled = true;
    }

    public void SetText(string textToSet)
    {
        text = textToSet;
        SetText();
    }

    public void SetText(string category, string id)
    {
        Localization loc = GameManager.instance.localization;
        loc.SetCategory(category);
        SetText(loc.GetLine(id));
    }

    public void SetText()
    {
        myText.text = text;
        UpdateTooltipSizeAndPosition();
    }

    public void UpdatePosition()
    {
        // Default alignment
        SetAlignment(alignmentOptionsHorizontal.LEFT, alignmentOptionsVertical.TOP);

        //UPDATE POSITION
        if (myRectTransform.localPosition.x > 0) {
            if (myRectTransform.localPosition.y - myRectTransform.sizeDelta.y < -(myCanvasTransform.sizeDelta.y / 2)) {
                //Le texte est trop bas et passe sous l'écran, il faut le monter
                SetAlignment(alignmentOptionsHorizontal.RIGHT, alignmentOptionsVertical.BOTTOM);
            }
            else {
                SetAlignment(alignmentOptionsHorizontal.RIGHT, alignmentOptionsVertical.TOP);
            }
        }
        else {
            if (myRectTransform.localPosition.y - myRectTransform.sizeDelta.y < -(myCanvasTransform.sizeDelta.y / 2)) {
                //Le texte est trop bas et passe sous l'écran, il faut le monter
                SetAlignment(alignmentOptionsHorizontal.LEFT, alignmentOptionsVertical.BOTTOM);
            }
        }
    }

    //Recupere le texte et transforme l'infobulle pour qu'elle soit visible
    public void UpdateTooltipSizeAndPosition()
    {
        UpdatePosition();
    }


    //Aligne le texte sur la droite ou la gauche (Change ses points de pivots et l'alignement du texte)
    public void SetAlignment(alignmentOptionsHorizontal alignmentX,alignmentOptionsVertical alignmentY = alignmentOptionsVertical.TOP)
    {
        RectTransform parentT = myText.GetComponentInParent<RectTransform>();

        switch (alignmentX)
        {
            case alignmentOptionsHorizontal.RIGHT:
                if (alignmentY == alignmentOptionsVertical.TOP)
                {
                   // myText.alignment = (TextAlignmentOptions) TextAnchor.UpperRight;
                    parentT.anchorMin = new Vector2(0.5f, 1);
                    parentT.anchorMax = new Vector2(0.5f, 1);
                    parentT.pivot = new Vector2(0.5f, 1);
                    myRectTransform.pivot = new Vector2(1, 1);
                } else
                {
                  //  myText.alignment = (TextAlignmentOptions)TextAnchor.LowerRight;
                    parentT.anchorMin = new Vector2(0.5f, 0);
                    parentT.anchorMax = new Vector2(0.5f, 0);
                    parentT.pivot = new Vector2(0.5f, 0);
                    myRectTransform.pivot = new Vector2(1, 0);
                }
                break;

            case alignmentOptionsHorizontal.LEFT:
                myRectTransform.pivot = new Vector2(0, 0);
                if (alignmentY == alignmentOptionsVertical.TOP)
                {
                    //myText.alignment = (TextAlignmentOptions)TextAnchor.UpperLeft;
                    parentT.anchorMin = new Vector2(0.5f, 1);
                    parentT.anchorMax = new Vector2(0.5f, 1);
                    parentT.pivot = new Vector2(0.5f, 1);
                    myRectTransform.pivot = new Vector2(0, 1);
                }
                else
                {
                   // myText.alignment = (TextAlignmentOptions)TextAnchor.LowerLeft;
                    parentT.anchorMin = new Vector2(0.5f, 0);
                    parentT.anchorMax = new Vector2(0.5f, 0);
                    parentT.pivot = new Vector2(0.5f, 0);
                    myRectTransform.pivot = new Vector2(0, 0);
                }
                break;
        }
    }
}
