using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventDebugWindow : MonoBehaviour {

    bool dragging;
    bool isHovering = false;
    Vector2 shift = new Vector2();

    public Text errorText;
    public Text checkText;
    public Color goodColor = Color.green;
    public Color badColor = Color.red;
    public InputField inputZone;
    public GameObject errorWindow;

    public List<GameObject> partsToHide;

    void Start()
    {
        GameManager.instance.eventManager.InterpreterError += (x) => {
            errorWindow.SetActive(true);
            errorText.text = x;
            errorText.color = badColor;
        };
        GameManager.instance.eventManager.CheckError += (x) => {
            checkText.text = x;
        };
        inputZone.onValueChanged.AddListener(delegate {
            Check();
        });
    }

    public void Update()
    {
        if (dragging) {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y) + shift;
        }

        if (isHovering) {
            if (Input.GetMouseButtonDown(1)) {
                foreach (GameObject toHide in partsToHide) {
                    toHide.SetActive(!toHide.activeSelf);
                }
            }
        }
    }

    public void OnPointerDown()
    {
        if (!dragging) {
            shift = 
                new Vector2(
                    transform.position.x,
                    transform.position.y
                ) - new Vector2(
                    Input.mousePosition.x, 
                    Input.mousePosition.y
            );
        }
        dragging = true;
    }

    public void OnPointerUp()
    {
        dragging = false;
    }

    public void Submit()
    {
        errorWindow.SetActive(true);
        errorText.text = "OK";
        errorText.color = goodColor;
        GameManager.instance.eventManager.ReadAndExecute(inputZone.text);
    }

    public void Check()
    {
        checkText.text = "...";
        GameManager.instance.eventManager.CheckSyntax(inputZone.text);
    }

    public void OnPointerEnter()
    {
        isHovering = true;
    }
    
    public void OnPointerExit()
    {
        isHovering = false;
    }
}
