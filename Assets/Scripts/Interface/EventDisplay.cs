using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventDisplay : MonoBehaviour {

    bool dragging;
    bool isHovering = false;
    Vector2 shift = new Vector2();
    EventManager.GameEvent gameEvent;
    List<GameObject> instantiatedButtons = new List<GameObject>();

    public Color goodColor = Color.green;
    public Color badColor = Color.red;

    public Text title;
    public Text description;
    public RawImage preview;
    public Transform buttonsContainer;

    public GameObject buttonExample;
    public GameObject subWindow;

    private void Start()
    {
        GameManager.instance.eventManager.newEvent += (x) => {
            gameEvent = x;
            subWindow.SetActive(true);
        };
    }

    public void InitializeButtons()
    {
        Localization loc = GameManager.instance.localization;

        foreach (KeyValuePair<int, EventManager.GameAction> gameAction in gameEvent.choices) {
            GameObject button = Instantiate(buttonExample, transform);
            Button buttonComponent = button.GetComponentInChildren<Button>();
            Text txt = button.GetComponentInChildren<Text>();
            Tooltip tt = button.GetComponentInChildren<Tooltip>();

            txt.text = loc.GetLine(gameAction.Key + ":" + gameEvent.id);
            foreach(EventManager.GameEffect action in gameAction.Value.GetGameEffects()) {
                tt.AddLocalizedLine(new Localization.Line("scriptAction", action.intention, action.parameters));
            }

            buttonComponent.onClick.AddListener(delegate {
                gameEvent.ExecuteChoice(gameAction.Key);
                subWindow.SetActive(false);
            });

            button.SetActive(true);

        }
    }

    public void SetEvent(EventManager.GameEvent _gameEvent)
    {
        gameEvent = _gameEvent;
    }




    /// <summary>
    /// Dragging
    /// </summary>
    public void OnPointerEnter()
    {
        isHovering = true;
    }

    public void OnPointerExit()
    {
        isHovering = false;
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
}
