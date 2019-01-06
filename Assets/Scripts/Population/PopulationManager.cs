using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulationManager : MonoBehaviour {

    public Population[] populationTypeList; //Liste de chaques type de population
    public List<Citizen> citizenList = new List<Citizen>(); //Liste de chaque citoyen de la colonie
    public class Citizen
    {
        public string name;
        public Population type;
        public House habitation;
    }


    //Generates some citizens on the colony
    public void SpawnCitizen(int amount, Population type)
    {
        for (int i = 0; i < amount; i++)
        {
            Citizen newCitizen = new Citizen();

            newCitizen.name = ""; //No name right now
            newCitizen.habitation = null;
            citizenList.Add(newCitizen);

            Debug.Log("Citizen " + newCitizen.name + " landed on the spatioport");
        }
    }

    //Kill a citizen
    public void KillCitizen(Citizen citizen)
    {
        if (citizenList.Contains(citizen))
        {
            citizenList.Remove(citizen);
            Debug.Log("Citizen " + citizen.name + " has been killed");
        }
    }
}
