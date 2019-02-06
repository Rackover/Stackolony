using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : Flag, Flag.IFlag
{
    //House Datas
    public int slotAmount;
    public Population[] acceptedPop;
    public List<PopulationManager.Citizen> affectedCitizen = new List<PopulationManager.Citizen>();
    public int standingLevel = 1;
    public float nuisanceImpact = 0; //Notation losse of a house caused by nuisance

    //Variables
    public List<Occupator> occupatorsInRange = new List<Occupator>();
    public List<FoodProvider> foodProvidersInRange = new List<FoodProvider>();
    public List<NotationModifier> notationModifiers = new List<NotationModifier>();
    public bool powered;
    public float foodReceived;
    public float distanceToGround;
    public float foodConsumption;

    ParticleSystem citizenIn;
    ParticleSystem citizenOut;

    Light houseLight;

    public void UpdateHouseInformations()
    {
        nuisanceImpact = block.nuisance * block.scheme.sensibility;
        foodConsumption = GetFoodConsumption();
        if (block.currentPower >= block.GetConsumption())
            powered = true;
        else
            powered = false;
        GetDistanceFromGround();
    }

    float GetFoodConsumption()
    {
        float fconsumption = 0;
        foreach (PopulationManager.Citizen citizen in affectedCitizen)
        {
            fconsumption += (GameManager.instance.populationManager.GetFoodConsumption(citizen.type));
        }
        return fconsumption;
    }

    override public void UpdateNuisanceImpact()
    {
        nuisanceImpact = block.nuisance * block.scheme.sensibility;
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

            houseLight.intensity = (affectedCitizen.Count / slotAmount) * 10;

            return;
        }
    }

    public override void Awake()
    {
        base.Awake();
        GameManager.instance.systemManager.AllHouses.Add(this);

        houseLight = block.effects.gameObject.AddComponent<Light>();
        houseLight.range = 1f;
        houseLight.intensity = 0f;
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemManager.AllHouses.Remove(this);
        base.OnDestroy();
    }

    public override void Enable()
    {
        base.Enable();
        GameManager.instance.systemManager.AllHouses.Add(this);
    }

    public override void Disable()
    {
        base.Disable();
        GameManager.instance.systemManager.AllHouses.Remove(this);
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

    public System.Type GetFlagType()
    {
        return GetType();
    }

    public string GetFlagDatas()
    {
        string profiles = "";
        for (int i = 0; i < acceptedPop.Length; i++)
        {
            if (i != 0)
                profiles += "-";
            profiles += acceptedPop[i].codeName;
        }
        return "House_" + slotAmount + "_" + standingLevel + "_" + profiles;
    }
}
