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

    void Start()
    {
        GameManager.instance.eventManager.interpreterError += (x) => {
            errorText.text = x;
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
        errorText.text = "";
        GameManager.instance.eventManager.ReadAndExecute(inputZone.text);
    }
}
