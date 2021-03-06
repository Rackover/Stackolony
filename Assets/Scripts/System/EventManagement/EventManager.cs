﻿using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.IO;
using System.Linq;

public class EventManager : MonoBehaviour {

    ScriptInterpreter interpreter = new ScriptInterpreter();

    public System.Action<string> InterpreterError;
    public System.Action<string> CheckError;

    public System.Action<GameEvent> NewEvent;
    public float chanceIncreasePerCycle = 0.33f;
    public float gameOverThreshold = 0.25f;
    public List<EventMarker> eventsPool = new List<EventMarker>();

    Dictionary<int, GameEvent> events = new Dictionary<int, GameEvent>();
    float chances = 0f;
    int nextEvent = 0;
    bool triggerEventWhenPossible = false;
    float triggerTime = 0f;
    int noEventsBefore = 255;

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
            if (choice > choices.Count) {
                return;
            }
            
            choices[choice].Execute();
        }
    }

    public class GameEffect
    {
        public Tooltip.informationType ttColor = Tooltip.informationType.Neutral;
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
            foreach (GameEffect action in actions) {
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

    public class EventMarker
    {
        public int minCycle;
        public int eventId;
        public float time;
    }

    public void ResetChances()
    {
        chances = 0f;
    }

    private void Update()
    {
        if (triggerEventWhenPossible) {
            float timeOfDay = GameManager.instance.temporality.GetCurrentCycleProgression();
            if (timeOfDay  > triggerTime) {
                TriggerEvent(nextEvent);
                triggerEventWhenPossible = false;
                return;
            }
        }
    }

    public void Renew(int currentCycle)
    {
        // Game Over
        float average = 0f;
        foreach(Population pop in GameManager.instance.populationManager.populationTypeList) {
            average += GameManager.instance.populationManager.GetAverageMood(pop);
        }
        average /= GameManager.instance.populationManager.populationTypeList.Length;

        if (average < gameOverThreshold) {
            TriggerEvent(1000, true);
        }

        // No events yet
        if (currentCycle < noEventsBefore) {
            Logger.Debug("Skipping event renewal for cycle " + currentCycle + " : no events before cycle " + noEventsBefore);
            return;
        }

        // Random event per day
        float random = Random.value;
        if (random < chances) {
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
            
            triggerEventWhenPossible = true;
            triggerTime = pick.time;
            nextEvent = pick.eventId;
            eventsPool = newList;
            chances = 0f;
        }
        else {
            chances += chanceIncreasePerCycle;
        }
    }

    public void ReadAndExecute(string eventScript)
    {
        GameAction action = interpreter.MakeEvent(eventScript, GameManager.instance.eventManager.InterpreterError);
        if (action != null) {
            try {
                action.Execute();
            }
            catch (ScriptInterpreter.InterpreterException e) {
                GameManager.instance.eventManager.InterpreterError.Invoke(e.ToString());
            }
            catch (System.Exception e) {
                GameManager.instance.eventManager.InterpreterError.Invoke("Unknown interpreter error - check your script.\n" + e.ToString());
            }
        }
    }

    public void CheckSyntax(string eventScript)
    {
        interpreter.MakeEvent(eventScript, CheckError, true);
    }
    
    public void TriggerEvent(int id, bool forceHappening = false)
    {
        // ID 1000 is Game Over event
        if (id == 1000 && GameManager.instance.DISABLE_GAME_OVER) {
            return;
        }

        StartCoroutine(WaitForEventAndTrigger(id, forceHappening));
    }

    public void TriggerEventImmediatly(int id)
    {
        if (GameManager.instance.cursorManagement.isDragging)
        {
            GameManager.instance.cursorManagement.CancelDrag();
        }
        if (!GameManager.instance.DISABLE_EVENTS) {
            NewEvent.Invoke(events[id]);
        }
    }

    IEnumerator WaitForEventAndTrigger(int id, bool forceHappening = false)
    {
        while (NewEvent == null || !events.ContainsKey(id) || GameManager.instance.cinematicManager.IsInCinematic()) {
            yield return null;
        }

        if (!forceHappening && GameManager.instance.populationManager.populations[events[id].instigator].citizens.Count <= 0) {
            // No one to instigate the event
            yield return false;
        }
        else {
            TriggerEventImmediatly(id);
            yield return true;
        }
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
        Bystander.Mood mood = Bystander.Mood.Good;
        try {
            pop = GameManager.instance.populationManager.GetPopulationByCodename(xEvent.Attributes["population"].Value);
            mood = (Bystander.Mood)System.Enum.Parse(typeof(Bystander.Mood), xEvent.Attributes["emotion"].Value);
        }
        catch { };
        
        foreach(XmlNode xChoice in xEvent.ChildNodes) {
            Choice choice = ReadXChoice(xChoice);
            if (choice != null) {
                // Making even and injecting the event id in it
                choices.Add(choice.id, interpreter.MakeEvent("DECLARE_EVENT(id:"+id.ToString()+");"+choice.script));
            }
        }

        return new GameEvent(id, choices, pop) { mood = mood };
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

                if (marker.minCycle < noEventsBefore) {
                    noEventsBefore = marker.minCycle;
                }

                eventsPool.Add(marker);
            }
        }
        Logger.Debug("Loaded " + eventsPool.Count.ToString() + " events into the pool");

        //Shuffling
        System.Random rng = new System.Random((System.Int32)System.DateTime.Now.TimeOfDay.TotalMilliseconds);
        eventsPool = eventsPool.OrderBy((a => rng.Next())).ToList();

        // Loading first cycle of Events
        if (eventsPool.Count != 0) {
            Renew(0);
        }
        else {
            Logger.Warn("Loaded zero event into the pool (?) - Please check the XML eventPool");
        }
    }

    public void ClearListeners()
    {
        try {
            foreach (System.Delegate d in NewEvent.GetInvocationList()) {
                NewEvent -= (System.Action<EventManager.GameEvent>)d;
            }
        }
        catch {
            // Nothing to do
        }
    }
}
