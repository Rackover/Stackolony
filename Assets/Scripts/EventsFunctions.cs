using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// POPULATION ID
//Eari = 0
//Kavga = 1
//Senuth = 2
//Covridians = 3
//Krowsers = 4

public class EventsFunctions : MonoBehaviour {


    PopulationManager populationManager = GameManager.instance.populationManager;

    // List qui associe un int à une Action qui prend un int en argument
    Dictionary<int, Action<int>> events = new Dictionary<int, Action<int>>();

    private void Start()
    {
        events.Add(
            0,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 1:

                        default:
                            break;
                    }
                }
             )
        );

        events[1].Invoke(6);
    }
}
