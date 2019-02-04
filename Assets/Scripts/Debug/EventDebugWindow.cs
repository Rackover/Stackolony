using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventDebugWindow : MonoBehaviour {

    bool dragging;
    Vector2 shift = new Vector2();

    public Text errorText;
    public Color goodColor = Color.green;
    public Color badColor = Color.red;
    public InputField inputZone;
    public GameObject errorWindow;

    void Start()
    {
        GameManager.instance.eventManager.interpreterError += (x) => {
            errorWindow.SetActive(true);
            errorText.text = x;
            errorText.color = badColor;
        };
    }

    public void Update()
    {
        if (dragging) {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y) + shift;
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
}
