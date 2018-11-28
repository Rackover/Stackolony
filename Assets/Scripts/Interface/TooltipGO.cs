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


    private alignmentOptionsHorizontal myAlignmentVertical;
    private alignmentOptionsVertical myAlignmentHorizontal;

    [System.Serializable]
    public class TooltipIcon
    {
        public string iconTag; //Tag qui sera remplacé par l'icône
        public Sprite iconSprite; //Icône à utiliser pour remplacer le tag
    }

    public TooltipIcon[] tooltipIcons;


    void Awake()
    {
        SetText();
    }

    public void SetText()
    {
        myText.text = text;

        UpdateTooltipSizeAndPosition();
    }


    //Recupere le texte et transforme l'infobulle pour qu'elle soit visible
    public void UpdateTooltipSizeAndPosition()
    {
        //UPDATE SIZE
        if (myText.preferredWidth > myText.rectTransform.sizeDelta.x)
        {
            myRectTransform.sizeDelta = new Vector2(myText.rectTransform.sizeDelta.x*2, myText.preferredHeight);
            myText.rectTransform.sizeDelta = new Vector2(myText.rectTransform.sizeDelta.x*2, myText.preferredHeight);
        } else
        {
            myRectTransform.sizeDelta = new Vector2(myText.preferredWidth, myText.preferredHeight);
            myText.rectTransform.sizeDelta = new Vector2(myText.preferredWidth, myText.preferredHeight);
        }


        //UPDATE POSITION
        if (myRectTransform.localPosition.x > 0)
        {
            if (myRectTransform.localPosition.y - myRectTransform.sizeDelta.y < -(myCanvasTransform.sizeDelta.y/2))
            {
                //Le texte est trop bas et passe sous l'écran, il faut le monter
                SetAlignment(alignmentOptionsHorizontal.RIGHT, alignmentOptionsVertical.BOTTOM);
            } else
            {
                SetAlignment(alignmentOptionsHorizontal.RIGHT, alignmentOptionsVertical.TOP);
            }
        } else
        {
            if (myRectTransform.localPosition.y - myRectTransform.sizeDelta.y < -(myCanvasTransform.sizeDelta.y / 2))
            {
                //Le texte est trop bas et passe sous l'écran, il faut le monter
                SetAlignment(alignmentOptionsHorizontal.LEFT, alignmentOptionsVertical.BOTTOM);
            }
            else
            {
                SetAlignment(alignmentOptionsHorizontal.LEFT, alignmentOptionsVertical.TOP);
            }
        }
    }


    //Aligne le texte sur la droite ou la gauche (Change ses points de pivots et l'alignement du texte)
    public void SetAlignment(alignmentOptionsHorizontal alignmentX,alignmentOptionsVertical alignmentY = alignmentOptionsVertical.TOP)
    {
        switch (alignmentX)
        {
            case alignmentOptionsHorizontal.RIGHT:
                if (alignmentY == alignmentOptionsVertical.TOP)
                {
                    myText.alignment = TextAlignmentOptions.TopRight;
                    myText.rectTransform.anchorMin = new Vector2(0.5f, 1);
                    myText.rectTransform.anchorMax = new Vector2(0.5f, 1);
                    myText.rectTransform.pivot = new Vector2(0.5f, 1);
                    myRectTransform.pivot = new Vector2(1, 1);
                } else
                {
                    myText.alignment = TextAlignmentOptions.BottomRight;
                    myText.rectTransform.anchorMin = new Vector2(0.5f, 0);
                    myText.rectTransform.anchorMax = new Vector2(0.5f, 0);
                    myText.rectTransform.pivot = new Vector2(0.5f, 0);
                    myRectTransform.pivot = new Vector2(1, 0);
                }
                break;
            case alignmentOptionsHorizontal.LEFT:
                myRectTransform.pivot = new Vector2(0, 0);
                if (alignmentY == alignmentOptionsVertical.TOP)
                {
                    myText.alignment = TextAlignmentOptions.TopLeft;
                    myText.rectTransform.anchorMin = new Vector2(0.5f, 1);
                    myText.rectTransform.anchorMax = new Vector2(0.5f, 1);
                    myText.rectTransform.pivot = new Vector2(0.5f, 1);
                    myRectTransform.pivot = new Vector2(0, 1);
                }
                else
                {
                    myText.alignment = TextAlignmentOptions.BottomLeft;
                    myText.rectTransform.anchorMin = new Vector2(0.5f, 0);
                    myText.rectTransform.anchorMax = new Vector2(0.5f, 0);
                    myText.rectTransform.pivot = new Vector2(0.5f, 0);
                    myRectTransform.pivot = new Vector2(0, 0);
                }
                break;
        }
    }
}
