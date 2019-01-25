using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventsManager : MonoBehaviour {
    public enum Type { BlockRequest, ValueModification, DamageBlockRequest, FoodModification, PopulationRequest, MoveBlockRequest, PopulationGreet, RepairBuildingRequest, PowerRequest, RangeModificationRequest}
    public enum Difficulty { Easy, Medium, Hard }

    [System.Serializable]
    public class Consequence
    {
        Type consequenceType;
        int information; //ID of a block for exemple
        int amount;
    }

    [System.Serializable]
    public class Event {
        public int eventID;
        public List<List<Consequence>> consequences; //List of each consequences for every possible choices of the player
        public string eventTitle;
        public Population populationAsking;
        public Difficulty difficulty;
        public string description;
        public UnityEvent relatedFunction;
        public string overlayDescription;
    }

    //Add class consequence + add list of list of consequences for each events
    public List<Event> eventList = new List<Event>();
}
