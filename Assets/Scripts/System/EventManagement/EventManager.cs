using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.IO;

public class EventManager : MonoBehaviour {

    EventInterpreter interpreter = new EventInterpreter();

    public System.Action<string> interpreterError;

    Dictionary<int, GameEvent> events = new Dictionary<int, GameEvent>();

    public class GameEvent
    {
        public int id;
        public Dictionary<int, GameAction> choices;

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

    public class GameAction
    {
        List<System.Action> actions;
        int minCycle = 0;

        public GameAction(List<System.Action> _actions)
        {
            actions = _actions;
        }

        public void Execute()
        {
            foreach(System.Action action in actions) {
                action.Invoke();
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
