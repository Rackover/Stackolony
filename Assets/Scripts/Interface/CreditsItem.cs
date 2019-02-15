using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CreditsItem : MonoBehaviour
     , IPointerDownHandler
     , IPointerUpHandler
     , IPointerEnterHandler
     , IPointerExitHandler
{

    bool isFocused = false;
    bool isDragging = false;
    Vector2 shift = new Vector2();
    Rigidbody2D rigidbody2;

    new string name = "";
    string function = "";

    public float forceFactor = 10f;
    public Text text;
    public Image image;

    private void Start()
    {
        rigidbody2 = GetComponent<Rigidbody2D>();
    }

    public void SetName(string _name)
    {
        name = _name;
        UpdateText();
    }

    public void SetFunction(string _function)
    {
        function = _function;
        UpdateText();
    }

    public void SetLogo(Sprite logo)
    {
        image.sprite = logo;
    }

    public void UpdateText()
    {
        text.text = 
            name + "\n" + 
            function;
    }

    private void Update()
    {
        if (isDragging) {
            Vector2 position2 = new Vector2(transform.position.x, transform.position.y);
            rigidbody2.AddForceAtPosition(
                (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - position2)* forceFactor,
                position2 + shift
            );
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isFocused = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isFocused = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isDragging) {
            shift =
                new Vector2(
                    transform.position.x,
                    transform.position.y
                ) - new Vector2(
                    Input.mousePosition.x,
                    Input.mousePosition.y
            );
        }
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }
}
