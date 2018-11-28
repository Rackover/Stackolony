using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour {

    [Header("References")]
    public CityManagement cityManager;
    public MissionManager missionManager;
    public BlockLink myBlockLink;


    //Fonction appelée lors de la création du script
    virtual public void Awake()
    {
        //TEMPORAIRE
        cityManager = FindObjectOfType<CityManagement>();
        missionManager = FindObjectOfType<MissionManager>();
        myBlockLink = GetComponent<BlockLink>();
    }

    //Alimente les blocs
    virtual public void Enable()
    {

    }

    //Desalimente les blocs
    virtual public void Disable()
    {

    }

    //Fonction appelée juste avant de déplacer le bloc
    virtual public void BeforeMovingBlock()
    {
    }

    //Fonction appelée juste après avoir déplacé le bloc
    virtual public void AfterMovingBlock()
    {
    }

    //Fonction appelée avant de détruire le bloc
    virtual public void OnBlockDestroy()
    {
    }

    //Fonction appelée à chaque début de cycle
    virtual public void OnNewCycle()
    {

    }

    //Fonction appelée quand la nuit commence
    virtual public void OnNightStart()
    {

    }

    //Fonction appelée quand le jour commence
    virtual public void OnDayStart()
    {

    }
}
