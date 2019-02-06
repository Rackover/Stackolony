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

    public Text advisorName;
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
            GetComponent<Image>().enabled = true;
            GameManager.instance.Pause();
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
            displayer.GetModel().GetComponent<Bystander>().SetEmotion(emotion);
            preview.gameObject.GetComponent<AspectRatioFitter>().aspectRatio = 1f;
        }

        // ADvisor name
        PopulationManager popMan = GameManager.instance.populationManager;
        string name = popMan.GetRandomName();
        if (popMan.populations[pop].citizens.Count > 0) {
            name = popMan.populations[pop].citizens[Mathf.FloorToInt(popMan.populations[pop].citizens.Count * Random.value)].name;
        }
        loc.SetCategory("populationTypeDelegate");
        advisorName.text = loc.GetLine(pop.codeName, name);

        title.text = loc.GetLineFromCategory("eventTitle", "event" + gameEvent.id);
        description.text = loc.GetLineFromCategory("eventDescription", "event" + gameEvent.id);
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

            txt.text = loc.GetLineFromCategory("choice", "event" +gameEvent.id + "_" + gameAction.Key);
            tt.isFirstLineBold = false;

            List<EventManager.GameEffect> fxs = gameAction.Value.GetGameEffects();
            foreach (EventManager.GameEffect action in fxs) {
                if (action.intention.Length <= 0) {
                    continue;
                }
                tt.AddLocalizedLine(
                    new Tooltip.TooltipLocalizationEntry(
                        action.intention, 
                        "scriptAction",
                        action.ttColor, 
                        action.parameters
                    )
                );
            }
            if (fxs.Count <= 0) {
                tt.AddLocalizedLine(
                    new Localization.Line(
                        "scriptAction",
                        "none"
                    )
                );
            }

            // On choice click
            buttonComponent.onClick.AddListener(delegate {
                try {
                    gameEvent.ExecuteChoice(gameAction.Key);
                }
                catch (System.Exception e) {
                    Logger.Error("Skipped event error : " + e.Message + ". This should NOT happen. Check the event syntax on.");
                }
                subWindow.SetActive(false);
                if (GameManager.instance.player.options.GetBool(Options.Option.animatedCitizens)) {
                    displayer.Unstage();
                }
                FindObjectOfType<TooltipGO>().Disable();
                GameManager.instance.UnPause();
                GetComponent<Image>().enabled = false;
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
