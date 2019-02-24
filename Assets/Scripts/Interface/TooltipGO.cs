using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipGO : MonoBehaviour {

    public enum horizontalAlignment { RIGHT, LEFT};
    public enum verticalAlignment { TOP, BOTTOM };

    [Header("=== REFERENCES ===")]
    public TextMeshProUGUI myText;
    public RectTransform self;
    public RectTransform canvas;
    public Image background;

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

    void Start()
    {
        background = GetComponent<Image>();
    }

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
        background.enabled = false;
    }

    public void Enable()
    {
        myText.enabled = true;
        background.enabled = true;
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
    }

    public void UpdateToolTip(Vector2 pos)
    {
        // Default alignment
        SetAlignment(horizontalAlignment.LEFT, verticalAlignment.TOP);
        Vector2Int shiftDirection = new Vector2Int(1, 1);

        //UPDATE POSITION
        if (self.position.x >= canvas.sizeDelta.x/2)
        {
            if (self.position.y >= canvas.sizeDelta.y/2)
            {
                SetAlignment(horizontalAlignment.RIGHT, verticalAlignment.TOP);
                shiftDirection.x = -1;
                shiftDirection.y = -1;
            }
            else
            {
                SetAlignment(horizontalAlignment.RIGHT, verticalAlignment.BOTTOM);
                shiftDirection.x = -1;
                shiftDirection.y = 1;
            }
        }
        else
        {
            if (self.position.y >= canvas.sizeDelta.y/2)
            {
                SetAlignment(horizontalAlignment.LEFT, verticalAlignment.TOP);
                shiftDirection.x = 1;
                shiftDirection.y = -1;
            }
            else
            {
                SetAlignment(horizontalAlignment.LEFT, verticalAlignment.BOTTOM);
                shiftDirection.x = 1;
                shiftDirection.y = 1;
            }
        }      
        transform.position = new Vector2(pos.x + (shift.x * shiftDirection.x), pos.y + (shift.y * shiftDirection.y));
    }


    //Aligne le texte sur la droite ou la gauche (Change ses points de pivots et l'alignement du texte)
    public void SetAlignment(horizontalAlignment alignmentX,verticalAlignment alignmentY = verticalAlignment.TOP)
    {
        RectTransform parentT = myText.GetComponentInParent<RectTransform>();

        switch (alignmentX)
        {
            case horizontalAlignment.RIGHT:
                if (alignmentY == verticalAlignment.TOP)
                {
                   // myText.alignment = (TextAlignmentOptions) TextAnchor.UpperRight;
                    parentT.anchorMin = new Vector2(0.5f, 1);
                    parentT.anchorMax = new Vector2(0.5f, 1);
                    parentT.pivot = new Vector2(0.5f, 1);
                    self.pivot = new Vector2(1, 1);
                } else
                {
                  //  myText.alignment = (TextAlignmentOptions)TextAnchor.LowerRight;
                    parentT.anchorMin = new Vector2(0.5f, 0);
                    parentT.anchorMax = new Vector2(0.5f, 0);
                    parentT.pivot = new Vector2(0.5f, 0);
                    self.pivot = new Vector2(1, 0);
                }
                break;

            case horizontalAlignment.LEFT:
                self.pivot = new Vector2(0, 0);
                if (alignmentY == verticalAlignment.TOP)
                {
                    //myText.alignment = (TextAlignmentOptions)TextAnchor.UpperLeft;
                    parentT.anchorMin = new Vector2(0.5f, 1);
                    parentT.anchorMax = new Vector2(0.5f, 1);
                    parentT.pivot = new Vector2(0.5f, 1);
                    self.pivot = new Vector2(0, 1);
                }
                else
                {
                   // myText.alignment = (TextAlignmentOptions)TextAnchor.LowerLeft;
                    parentT.anchorMin = new Vector2(0.5f, 0);
                    parentT.anchorMax = new Vector2(0.5f, 0);
                    parentT.pivot = new Vector2(0.5f, 0);
                    self.pivot = new Vector2(0, 0);
                }
                break;
        }
    }
}
