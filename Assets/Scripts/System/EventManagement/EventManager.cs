using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {

    EventInterpreter interpreter = new EventInterpreter();

    public System.Action<string> interpreterError;

    public class GameEvent
    {
        List<System.Action> actions;
        int id;

        public GameEvent(int _id, List<System.Action> _actions)
        {
            id = _id;
            actions = _actions;
        }

        public void Execute()
        {
            foreach(System.Action action in actions) {
                action.Invoke();
            }
        }
    }

    public void ReadAndExecute(string evnt)
    {
        interpreter.MakeEvent(0, evnt).Execute();
    }
    

}
