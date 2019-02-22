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

    public List<string> skippedActions = new List<string>() { "DECLARE_EVENT" };

    public Color goodColor = Color.green;
    public Color badColor = Color.red;
    public float typewriterRate = 0.3f;

    public Text advisorName;
    public Text title;
    public Text description;
    public RawImage preview;
    public Transform buttonsContainer;

    public GameObject buttonExample;
    public GameObject subWindow;

    private void Start()
    {
        GameManager.instance.eventManager.NewEvent += (x) => {
            gameEvent = x;
            subWindow.SetActive(true);
            InitializeButtons();
            InitializeWindow();
            GetComponent<Image>().enabled = true;
            GameManager.instance.Pause();
        };
    }

    private void Update()
    {
        if (dragging) 
        {
            transform.position = new Vector2(
                Mathf.Clamp(Input.mousePosition.x, 0, Screen.width),
                Mathf.Clamp(Input.mousePosition.y, 0, Screen.height)
                ) + shift;
        }
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


        description.text = "";
        StartTypewriting(description, loc.GetLineFromCategory("eventDescription", "event" + gameEvent.id));
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

            // Remove untranslatable actions like DECLARE_EVENT
            fxs.RemoveAll(o => skippedActions.Contains(o.intention));

            foreach (EventManager.GameEffect action in fxs) {
                if (action.intention.Length <= 0) {
                    continue;
                }
                tt.AddLocalizedLine(
                    new Tooltip.Entry(
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
                    Logger.Error("Skipped event error : " + e.ToString() + ". This should NOT happen. Check the event syntax on.");
                }
                subWindow.SetActive(false);
                if (GameManager.instance.player.options.GetBool(Options.Option.animatedCitizens)) {
                    displayer.Unstage();
                }
                FindObjectOfType<TooltipGO>().Disable();
                GameManager.instance.UnPause();
                GetComponent<Image>().enabled = false;
                StopAllCoroutines();
            });

            instantiatedButtons.Add(button);

            button.SetActive(true);
        }
    }

    public void SetEvent(EventManager.GameEvent _gameEvent)
    {
        gameEvent = _gameEvent;
    }

    public void StartTypewriting(Text target, string content)
    {
        StartCoroutine(Typewrite(target, content, 0, typewriterRate));
    }

    IEnumerator Typewrite(Text target, string content, int index, float delayBeforeNextChar, List<string> activeTags=null)
    {
        if (activeTags == null) {
            activeTags = new List<string>();
        }

        // Play sound
        GameManager.instance.soundManager.voicePlayer.ContinuePlaying(gameEvent.instigator);

        // Typewrite
        string txt = content.Substring(0, index);
        if (content[index] == '<') {
            if (content[index+1] == '/'){
                // Closing tag
                string tag = content[index].ToString();
                for (int i = index+1; i < content.Length; i++) {
                    tag += content[i];
                    if (content[i] == '>') {
                        index = i+1;
                        break;
                    }
                }
                txt = content.Substring(0, index);
                string realTag = tag.Replace("</", "").Replace(">", "");
                activeTags.RemoveAll(o => o == realTag);
            }
            else{
                // Opening tag
                string tag = content[index].ToString();
                for (int i = index + 1; i < content.Length; i++) {
                    tag += content[i];
                    if (content[i] == '>') {
                        index = i+1;
                        break;
                    }
                }
                txt = content.Substring(0, index);
                string realTag = tag.Replace("<", "").Replace(">", "");
                activeTags.Add(realTag);
            }
        }

        // Adding closing tags
        foreach(string tag in activeTags) {
            txt += "</"+tag.Split('=')[0]+">";
        }

        // Displaying text
        target.text = txt;

        // Advance
        if (index+1 < content.Length) {
            float delay = delayBeforeNextChar;

            // Special characters pause
            if (index > 0) {
                switch (content[index - 1]) {
                    case ' ': delay = 0f; break;
                    case '!':
                    case '?':
                    case '.':
                    case ':':
                        GameManager.instance.soundManager.voicePlayer.Stop();
                        delay = 1f;
                        break;
                }
                if (content[index - 1] == ' ') {
                    delay = 0f;
                }
            }

            if (delay > 0f) {
                yield return new WaitForSeconds(delay);
            }
            yield return Typewrite(target, content, index+1, delayBeforeNextChar, activeTags);
        }
        else {
            GameManager.instance.soundManager.voicePlayer.Stop();
            yield return true;
        }
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
