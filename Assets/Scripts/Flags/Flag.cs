using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour {

    //Alimente les blocs
    virtual public void Enable()
    {

    }

    //Desalimente les blocs
    virtual public void Disable()
    {

    }

    //Fonction appelée juste avant de déplacer le bloc
    virtual public void BeforeBlockMove()
    {
    }

    //Fonction appelée juste après avoir déplacé le bloc
    virtual public void AfterBlockMove()
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

    //Fonction appelée lors de la création du script
    virtual public void Awake()
    {
        
    }
}
