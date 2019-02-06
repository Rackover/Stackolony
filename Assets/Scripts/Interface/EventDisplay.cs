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
    Displayer displayer;

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
            InitializeButtons();
            InitializeWindow();
        };
    }

    public void InitializeWindow()
    {
        Population pop = gameEvent.instigator;
        Bystander.Mood emotion = gameEvent.mood;
        Localization loc = GameManager.instance.localization;

        if (!GameManager.instance.player.options.GetBool(Options.Option.animatedCitizens)) {
            float ratio = pop.moodSprites[0].rect.width / pop.moodSprites[0].rect.height;
            preview.texture = pop.moodSprites[(int)emotion].texture;
            preview.gameObject.GetComponent<AspectRatioFitter>().aspectRatio = ratio;
        }
        else {
            displayer = GameManager.instance.displayerManager.SetRotationFeed(pop.prefab, preview, 180, 0, 3, 30, 512);
            displayer.GetModel().transform.GetChild(0).gameObject.GetComponent<Animator>().Play("LookAround");
            preview.gameObject.GetComponent<AspectRatioFitter>().aspectRatio = 1f;
        }

        title.text = loc.GetLine("event" + gameEvent.id, "eventTitle");
        description.text = loc.GetLine("event" + gameEvent.id, "eventDescription");
    }

    public void InitializeButtons()
    {
        Localization loc = GameManager.instance.localization;
        
        foreach(GameObject b in instantiatedButtons) {
            Destroy(b);
        }

        foreach (KeyValuePair<int, EventManager.GameAction> gameAction in gameEvent.choices) {
            GameObject button = Instantiate(buttonExample, buttonExample.transform.parent);
            Button buttonComponent = button.GetComponentInChildren<Button>();
            Text txt = button.GetComponentInChildren<Text>();
            Tooltip tt = button.GetComponentInChildren<Tooltip>();

            txt.text = loc.GetLine(gameAction.Key + ":" + gameEvent.id, "choice");
            tt.isFirstLineBold = false;
            foreach (EventManager.GameEffect action in gameAction.Value.GetGameEffects()) {
                if (action.intention.Length <= 0) {
                    continue;
                }
                tt.AddLocalizedLine(
                    new Localization.Line(
                        new Tooltip.TooltipLocalizationEntry(
                            action.intention, 
                            "scriptAction",
                            action.ttColor, 
                            action.parameters
                        )
                    )
                );
            }

            buttonComponent.onClick.AddListener(delegate {
                gameEvent.ExecuteChoice(gameAction.Key);
                subWindow.SetActive(false);
                if (GameManager.instance.player.options.GetBool(Options.Option.animatedCitizens)) {
                    displayer.Unstage();
                }
            });

            instantiatedButtons.Add(button);

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
