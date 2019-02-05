using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.IO;

public class EventManager : MonoBehaviour {

    EventInterpreter interpreter = new EventInterpreter();

    public System.Action<string> interpreterError;
    public System.Action<GameEvent> newEvent;

    Dictionary<int, GameEvent> events = new Dictionary<int, GameEvent>();

    public class GameEvent
    {
        public int id;
        public Dictionary<int, GameAction> choices;
        int minCycle = 0;

        public GameEvent(int _id, Dictionary<int, GameAction>_choices)
        {
            id = _id;
            choices = _choices;
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
            return localizer.GetLine(intention, "gameEffect", parameters);
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

    

    public void ReadAndExecute(string eventScript)
    {
        interpreter.MakeEvent(eventScript).Execute();
    }
    
    public void LoadEventsDatabase()
    {
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
        
        foreach(XmlNode xChoice in xEvent.ChildNodes) {
            Choice choice = ReadXChoice(xChoice);
            if (choice != null) {
                choices.Add(choice.id, interpreter.MakeEvent(choice.script));
            }
        }

        return new GameEvent(id, choices);
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

}
