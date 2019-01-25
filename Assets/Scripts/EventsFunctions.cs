using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// POPULATION ID
//Eari = 0
//Kavga = 1
//Senuth = 2
//Covridians = 3
//Krowsers = 4

public class EventsFunctions : MonoBehaviour {

    PopulationManager populationManager = GameManager.instance.populationManager;

    public void Event1(int playerAnswer)
    {
        switch (playerAnswer)
        {
            case 1:
                StartMission(9);
                break;

            default: //Default is always the last choice possible
                break;
        }
    }

    public void Event2(int playerAnswer)
    {
        switch (playerAnswer)
        {
            case 1:
                StartMission(10);
                break;

            default: //Default is always the last choice possible
                break;
        }
    }

    public void Event3(int playerAnswer)
    {
        switch (playerAnswer)
        {
            case 1:
                StartMission(1);
                break;

            default: //Default is always the last choice possible
                break;
        }
    }

    public void Event4(int playerAnswer)
    {
        switch (playerAnswer)
        {
            case 1:
                populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(0), 0, 1, 2);
                break;

            default: //Default is always the last choice possible
                populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(0), 1, -10, 2);
                break;
        }
    }

    public void Event5(int playerAnswer)
    {
        switch (playerAnswer)
        {
            case 1:
                populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(1), 0, 2, 3);
                break;

            default: //Default is always the last choice possible
                populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(0), 1, -10, 2);
                break;
        }
    }






    void StartMission(int missionIndex)
    {

    }
}
