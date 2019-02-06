using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.IO;
using System.Linq;

public class EventManager : MonoBehaviour {

    EventInterpreter interpreter = new EventInterpreter();

    public System.Action<string> interpreterError;
    public System.Action<string> checkError;

    public System.Action<GameEvent> newEvent;
    public float chanceIncreasePerCycle = 0.33f;

    Dictionary<int, GameEvent> events = new Dictionary<int, GameEvent>();
    List<EventMarker> eventsPool = new List<EventMarker>();
    float chances = 0f;

    public class GameEvent
    {
        public int id;
        public Dictionary<int, GameAction> choices;
        public Population instigator;
        public Bystander.Mood mood;
        
        public GameEvent(int _id, Dictionary<int, GameAction>_choices, Population _instigator=null)
        {
            id = _id;
            choices = _choices;
            instigator = _instigator;
        }

        public void ExecuteChoice(int choice)
        {
            if (choice >= choices.Count) {
                return;
            }

            choices[choice].Execute();
        }
    }

    public class GameEffect
    {
        public Tooltip.tooltipType ttColor = Tooltip.tooltipType.Neutral;
        public string intention = "";
        public string[] parameters = new string[] { };
        System.Action action;

        public GameEffect(System.Action _action, string _intention, params string[] _parameters)
        {
            action = _action;
            intention = _intention;
            parameters = _parameters;
        }

        public string GetFormattedIntention(Localization localizer)
        {
            return localizer.GetLineFromCategory("gameEffect", intention, parameters);
        }
        
        public System.Action GetAction()
        {
            return action;
        }

        public void SetAction(System.Action _action)
        {
            action = _action;
        }
    }

    public class GameAction
    {
        List<GameEffect> actions;

        public GameAction(List<GameEffect> _actions)
        {
            actions = _actions;
        }

        public List<GameEffect> GetGameEffects()
        {
            return actions;
        }

        public void Execute()
        {
            foreach(GameEffect action in actions) {
                action.GetAction().Invoke();
            }
        }
    }

    class Choice
    {
        public string script;
        public int id;

        public Choice(int _id, string _script)
        {
            id = _id;
            script = _script;
        }
    }

    class EventMarker
    {
        public int minCycle;
        public int eventId;
        public float time;
    }

    public void Renew(int currentCycle)
    {
        if (Random.value < chances) {
            EventMarker pick = null;
            List<EventMarker> newList = new List<EventMarker>();
            foreach (EventMarker candidate in eventsPool) {
                if (pick == null &&
                    candidate.minCycle <= currentCycle) {
                    pick = candidate;
                    continue;
                }
                newList.Add(candidate);
            }

            if (pick == null) {
                // No more bulletins, let's reload
                LoadEventsPool();
                return;
            }

            TriggerEvent(pick.eventId);
            eventsPool = newList;
            chances = 0f;
        }
        else {
            chances += chanceIncreasePerCycle;
        }
    }

    public void ReadAndExecute(string eventScript)
    {
        GameAction action = interpreter.MakeEvent(eventScript, GameManager.instance.eventManager.interpreterError);
        if (action != null) {
            try {
                action.Execute();
            }
            catch (EventInterpreter.InterpreterException e) {
                GameManager.instance.eventManager.interpreterError.Invoke(e.Message);
            }
            catch (System.Exception e) {
                GameManager.instance.eventManager.interpreterError.Invoke("Unknown interpreter error - check your script.\n" + e.Message);
            }
        }
    }

    public void CheckSyntax(string eventScript)
    {
        interpreter.MakeEvent(eventScript, checkError);
    }
    
    public void TriggerEvent(int id)
    {
        newEvent.Invoke(events[id]);
    }

    public void LoadEvents()
    {
        LoadEventsDatabase();
        LoadEventsPool();
    }

    void LoadEventsDatabase()
    {
        events = new Dictionary<int, GameEvent>();
        string path = Paths.GetEventsDatabaseFile();
        XmlDocument eventsDbFile = new XmlDocument();

        try {
            eventsDbFile.Load(path);
        }
        catch (FileNotFoundException e) {
            Logger.Throw("Could not access events database file at path " + path + ". Error : " + e.ToString());
            return;
        }

        XmlNodeList nodeList = eventsDbFile.SelectNodes("eventsDatabase")[0].ChildNodes;
        foreach (XmlNode xEvent in nodeList) {
            // Garbage node
            if (xEvent.Name != "event") {
                continue;
            }

            try {
                GameEvent gameEvent = ReadXEvent(xEvent);
                events.Add(gameEvent.id, gameEvent);
            }
            catch (System.Exception e) {
                Logger.Error("Skipped event because of an error while reading : " + xEvent.InnerText +"\n" + e.ToString());
            }
        }
    }

    GameEvent ReadXEvent(XmlNode xEvent)
    {
        Dictionary<int, GameAction> choices = new Dictionary<int, GameAction>();
        int id;
        try {
            id = System.Convert.ToInt32(xEvent.Attributes["id"].Value);
        }
        catch (System.Exception) {
            Logger.Error("Skipped id-less event : " + xEvent.InnerText);
            return null;
        }

        // Ignoring pop and mood if not specified
        Population pop = null;
        Bystander.Mood mood;
        try {
            pop = GameManager.instance.populationManager.GetPopulationByCodename(xEvent.Attributes["population"].Value);
            mood = (Bystander.Mood)System.Enum.Parse(typeof(Bystander.Mood), xEvent.Attributes["emotion"].Value);
        }
        catch (System.Exception) { };
        
        foreach(XmlNode xChoice in xEvent.ChildNodes) {
            Choice choice = ReadXChoice(xChoice);
            if (choice != null) {
                choices.Add(choice.id, interpreter.MakeEvent(choice.script));
            }
        }

        return new GameEvent(id, choices, pop);
    }

    Choice ReadXChoice(XmlNode xChoice)
    {
        int id;
        try {
            id = System.Convert.ToInt32(xChoice.Attributes["id"].Value);
        }
        catch (System.Exception) {
            Logger.Error("Skipped id-less choice: " +xChoice.InnerText);
            return null;
        }

        return new Choice(id, xChoice.InnerText);
    }

    void LoadEventsPool()
    {
        // XML Loading
        eventsPool = new List<EventMarker>();
        string path = Paths.GetEventsPoolFile();
        XmlDocument poolFile = new XmlDocument();
        try {
            poolFile.Load(path);
        }
        catch (FileNotFoundException e) {
            Logger.Throw("Could not access events pool file at path " + path + ". Error : " + e.ToString());
            return;
        }

        // XML Parsing & List filling
        XmlNodeList pools = poolFile.SelectNodes("events")[0].ChildNodes;
        foreach (XmlNode pool in pools) {
            foreach (XmlNode xEventMarker in pool.ChildNodes) {

                // Most probably a comment - skip
                if (xEventMarker.Name != "event") {
                    continue;
                }

                // Bulletin creation
                EventMarker marker = new EventMarker() {
                    eventId = System.Convert.ToInt32(xEventMarker.InnerText),
                    time = System.Convert.ToSingle(xEventMarker.Attributes["time"].Value),
                    minCycle = System.Convert.ToInt32(pool.Attributes["minCycle"].Value)
                };

                eventsPool.Add(marker);
            }
        }
        Logger.Debug("Loaded " + eventsPool.Count.ToString() + " events into the pool");

        //Shuffling
        System.Random rng = new System.Random((System.Int32)System.DateTime.Now.TimeOfDay.TotalMilliseconds);
        eventsPool = eventsPool.OrderBy((a => rng.Next())).ToList();

        // Loading first cycle of Events
        Renew(0);
    }
}
