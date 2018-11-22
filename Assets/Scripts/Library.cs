using UnityEngine;

[System.Serializable]
public class Job 
{
	public string jobName;
	public int spotCount;
	public Library.Profile profile;
}

[System.Serializable]
public class Production 
{
	public int range;
	public float power;
	public Library.Ressource ressource;
}

public class Library 
{
    public enum Profile{ Scientist, Worker, Military, Artist, Tourist }
    public enum Ressource{ Energy, Mood, Food }
    public enum Problem{ Riot, Fire }
    public enum Quality{ Low, Medium, High }
}
