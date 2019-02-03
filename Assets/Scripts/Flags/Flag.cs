using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour {

    [Header("References")]
    public Block block;
    public bool isEnabled = true;


    public interface IFlag {
        System.Type GetFlagType();
    }

    //Fonction appelée lors de la création du script
    virtual public void Awake()
    {
        block = GetComponent<Block>();
    }

    //Alimente les blocs
    virtual public void Enable()
    {
        isEnabled = true;
    }

    //Desalimente les blocs
    virtual public void Disable()
    {
        isEnabled = false;
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
    virtual public void OnDestroy()
    {
    }

    //Fonction appelée à chaque début de cycle
    virtual public void OnNewCycle()
    {
        if(!isEnabled) return;
    }

    //Fonction appelée à chaque début de cycle
    virtual public void OnNewMicrocycle()
    {
        if(!isEnabled) return;
    }

    //Fonction appelée quand la nuit commence
    virtual public void OnNightStart()
    {
    }

    //Fonction appelée quand le jour commence
    virtual public void OnDayStart()
    {
    }

    //Fonction appelée quand la position du bloc se met à jour
    virtual public void OnBlockUpdate()
    {
    }

    virtual public void Update()
    {
        if(!isEnabled) return;
    }

    virtual public void Use()
    {
    }
}
