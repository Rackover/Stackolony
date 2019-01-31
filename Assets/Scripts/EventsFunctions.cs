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
//TODO : Change "GetPopulationByID" by "GetPopulationByCodename"

public class EventsFunctions : MonoBehaviour {


    PopulationManager populationManager = GameManager.instance.populationManager;

    // List qui associe un int à une Action qui prend un int en argument
    Dictionary<int, Action<int>> events = new Dictionary<int, Action<int>>();

    private void Start()
    {
        //Event 1
        events.Add(
            0,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            //Relaxing Lounge "1" sees its energy increased by 1 during 3 cycles
                            Population affectedPop = populationManager.GetPopulationByID(2);
                            populationManager.GenerateMoodModifier(affectedPop, ModifierReason.NoCandyMachine, 1, 3);
                            populationManager.GenerateFoodModifier(affectedPop, ModifierReason.NoCandyMachine, 0.5f, 3);
                            break;
                        default:
                            break;
                    }
                }
             )
        );

        //Event 2
        events.Add(
            1,
            new Action<int>(
                (eventOutput) => {
                    Population affectedPop = populationManager.GetPopulationByID(0);
                    switch (eventOutput)
                    {
                        case 0:
                            //+1 Random housing notation containing a eari
                            //This house make nuisance during 1 cycle
                            populationManager.GenerateMoodModifier(affectedPop, ModifierReason.PaintedHisHouse, 1, 2);
                            break;
                        default:
                            populationManager.GenerateMoodModifier(affectedPop, ModifierReason.CouldntPaintHisHouse, -1, 2);
                            break;
                    }
                }
             )
        );

        //Event 3
        events.Add(
            2,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            Population affectedPop = populationManager.GetPopulationByID(1);
                            populationManager.GenerateMoodModifier(affectedPop, ModifierReason.GotExtraSpinach, 2, 3);
                            populationManager.GenerateFoodModifier(affectedPop, ModifierReason.GotExtraSpinach, 0.5f, 3);
                            break;
                        default:
                            break;
                    }
                }
             )
        );

        //Event 4
        events.Add(
            3,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            //Energy plant "2" increased by 5
                            //Range of the nuisance increased by 2
                            break;
                        default:
                            break;
                    }
                }
             )
        );

        //Event 5
        events.Add(
            4,
            new Action<int>(
                (eventOutput) => {
                    Population affectedPop = populationManager.GetPopulationByID(4);
                    switch (eventOutput)
                    {
                        case 0:
                            //Maximum use reduced by 3 in the factory '3'
                            populationManager.GenerateMoodModifier(affectedPop, ModifierReason.CleanedFactory, 2, 3);
                            break;
                        default:
                            //Factory '3' goes from "good condition" to "damaged" if it's not already damaged
                            populationManager.GenerateMoodModifier(affectedPop, ModifierReason.UncleanedFactory, -10, 3);
                            break;
                    }
                }
             )
        );

        //Event 6
        events.Add(
            5,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            //Barracks "2" become damaged
                            populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(2), ModifierReason.ThrowedTrashOnKavgas, 3, 3);
                            populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(1), ModifierReason.ThrowedTrashOnKavgas, -2, 3);
                            break;
                        default:
                            populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(2), ModifierReason.CouldntThrowTrashOnKavgas, -10, 3);
                            break;
                    }
                }
             )
        );

        //Event 7
        events.Add(
            6,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            //Homes containing Covridians generate nuisance during 3 cycles
                            populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(3), ModifierReason.AutorizedWiredCommunication, 2, 3);
                            break;
                        default:
                            populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(3), ModifierReason.RefusedWiredCommunication, -2, 3);
                            break;
                    }
                }
             )
        );

        //Event 8
        events.Add(
            7,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            //+3 Krowsers in the next arrival of settlers
                            populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(4), ModifierReason.AcceptedKrowserFamily, 2, 2);
                            break;
                        default:
                            break;
                    }
                }
             )
        );

        //Event 9
        events.Add(
            8,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            int random = UnityEngine.Random.Range(0, 100);
                            if (random <= 40)
                            {
                                populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(3), ModifierReason.AcceptedCrystalResearchs, 2, 3);
                            } else
                            {
                                //Science building "1" destroyed
                            }
                            break;
                        default:
                            break;
                    }
                }
             )
        );

        //Event 10
        events.Add(
            9,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            //The "3" factory is transformed into an exhibition gallery
                            break;
                        default:
                            break;
                    }
                }
             )
        );

        //Event 11
        events.Add(
            10,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            int random = UnityEngine.Random.Range(0, 100);
                            if (random <= 50)
                            {
                                //House "1" goes into "damaged"
                            }
                            else
                            {
                                //House "1" catches fire
                                populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(1), ModifierReason.AccidentallyBurnedHouse, 1, 2);
                            }
                            break;
                        default:
                            break;
                    }
                }
             )
        );

        //Event 12
        events.Add(
            11,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(0), ModifierReason.HadBanquet, 2, 5);
                            populationManager.GenerateFoodModifier(populationManager.GetPopulationByID(0), ModifierReason.HadBanquet, 1, 5);
                            break;
                        default:
                            populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(0), ModifierReason.NoBanquet, -2, 5);
                            break;
                    }
                }
             )
        );

        //Event 13
        events.Add(
            12,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            //House "5" consumes +2 additional energies during 2 cycles
                            populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(4), ModifierReason.GoodLighting, 1, 2);
                            populationManager.GenerateFoodModifier(populationManager.GetPopulationByID(2), ModifierReason.GoodLighting, 1, 2);
                            break;
                        default:
                            break;
                    }
                }
             )
        );

        //Event 14
        events.Add(
            13,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            //Krowser occupancy blocks increase their range by +2, and their maximum occupancy by +5 and create "nuisance" during 3 cycles
                            break;
                        default:
                            break;
                    }
                }
             )
        );

        //Event 15
        events.Add(
            14,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            //House "3" generates nuisance during 3 cycles
                            populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(0), ModifierReason.AllowedConcert, 2, 3);
                            break;
                        default:
                            break;
                    }
                }
             )
        );

        //Event 16
        events.Add(
            15,
            new Action<int>(
                (eventOutput) => {
                    switch (eventOutput)
                    {
                        case 0:
                            //A generate +2 electricity
                            populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(4), ModifierReason.RepairedPowerAccumulator, 1, 3);
                            populationManager.GenerateMoodModifier(populationManager.GetPopulationByID(2), ModifierReason.RepairedPowerAccumulator, -2, 3);
                            break;
                        default:
                            int random = UnityEngine.Random.Range(0, 100);
                            if (random <= 50)
                            {
                                //The energy generator / wind turbine / solar panel "1" catches fire
                            }
                            break;
                    }
                }
             )
        );
    }
}
