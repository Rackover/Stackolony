using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : Flag {

    //House Datas
    public int slotAmount;
    public Population[] acceptedPop;
    public float foodConsumptionPerHabitant;
    public List<PopulationManager.Citizen> affectedCitizen = new List<PopulationManager.Citizen>();
    public int standingLevel = 1;

    //Variables
    public List<Occupator> occupatorsInRange = new List<Occupator>();
    public List<FoodProvider> foodProvidersInRange = new List<FoodProvider>();
    public bool connectedToSpatioport;
    public bool powered;
    public float foodReceived;
    public float distanceToGround;
    public float foodConsumption;
    public int citizenCount;

    public void UpdateHouseInformations()
    {
        citizenCount = 0;
        foreach (PopulationManager.Citizen citizen in affectedCitizen)
        {
            if (citizen != null)
            {
                citizenCount++;
            }
        }
        foodConsumption = foodConsumptionPerHabitant * citizenCount;
        if (myBlockLink.currentPower >= myBlockLink.block.consumption)
            powered = true;
        else
            powered = false;
        GetDistanceFromGround();
    }

    public void GetDistanceFromGround()
    {
        distanceToGround = GameManager.instance.gridManagement.GetDistanceFromGround(myBlockLink.gridCoordinates) - 0.5f;
    }

    public void InitCitizensSlots()
    {
        affectedCitizen = new List<PopulationManager.Citizen>();
    }

    public void FillWithCitizen(PopulationManager.Citizen citizen)
    {
        if (affectedCitizen.Count < slotAmount)
        {
            affectedCitizen.Add(citizen);
            citizenCount++;
            //Fourni un travail au nouveau citoyen, s'il y en a un de disponible
            GameManager.instance.systemManager.UpdateJobsDistribution();
            return;
        }
    }

    public override void Awake()
    {
        base.Awake();
        GameManager.instance.systemManager.AllHouses.Add(this);
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemManager.AllHouses.Remove(this);
        base.OnDestroy();
    }
}
