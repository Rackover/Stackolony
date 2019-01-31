﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : Flag
{
    //House Datas
    public int slotAmount;
    public Population[] acceptedPop;
    public float foodConsumptionPerHabitant;
    public List<PopulationManager.Citizen> affectedCitizen = new List<PopulationManager.Citizen>();
    public int standingLevel = 1;
    public int notationModifier = 0;

    //Variables
    public List<Occupator> occupatorsInRange = new List<Occupator>();
    public List<FoodProvider> foodProvidersInRange = new List<FoodProvider>();
    public bool powered;
    public float foodReceived;
    public float distanceToGround;
    public float foodConsumption;

    ParticleSystem citizenIn;
    ParticleSystem citizenOut;

    public void UpdateHouseInformations()
    {
        foodConsumption = foodConsumptionPerHabitant * affectedCitizen.Count;
        if (block.currentPower >= block.scheme.consumption)
            powered = true;
        else
            powered = false;
        GetDistanceFromGround();
    }

    public void GetDistanceFromGround()
    {
        distanceToGround = GameManager.instance.gridManagement.GetDistanceFromGround(block.gridCoordinates) - 0.5f;
    }

    public void InitCitizensSlots()
    {
        affectedCitizen = new List<PopulationManager.Citizen>();
    }

    public void FillWithCitizen(PopulationManager.Citizen citizen)
    {
        if (affectedCitizen.Count < slotAmount)
        {
            //Retire le citoyen de son ancienne habitation
            if (citizen.habitation != null)
                citizen.habitation.affectedCitizen.Remove(citizen);

            CitizenInFX();
            //Ajoute le citoyen dans l'habitation
            affectedCitizen.Add(citizen);

            //Ecrit sur la carte d'identité du citoyen qu'il habite ici désormais
            citizen.habitation = this;

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

    void CitizenInFX()
    {
        if(citizenIn == null)
        {
            citizenIn = Instantiate(GameManager.instance.library.citizenInParticle, transform.position, Quaternion.identity, transform).GetComponent<ParticleSystem>();
            citizenIn.maxParticles = 1;
        }
        citizenIn.Play();
    }

    void CitizenOutFX()
    {
        if(citizenOut == null)
        {
            citizenOut = Instantiate(GameManager.instance.library.citizenOutParticle, transform.position, Quaternion.identity, transform).GetComponent<ParticleSystem>();
            citizenOut.maxParticles = 1;
        }
        citizenOut.Play();  
    }
}
