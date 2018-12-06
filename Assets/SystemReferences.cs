using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemReferences : MonoBehaviour {
    public List<Generator> AllGenerators;
    public List<BlockLink> AllBlocksRequiringPower;

    private void Awake()
    {
        AllGenerators = new List<Generator>();
        AllBlocksRequiringPower = new List<BlockLink>();
    }

    //Met a jour le system de jeu
    public void UpdateSystem()
    {
        StartCoroutine(RecalculatePropagation());
        StartCoroutine(ResetBlocksPower());
    }

    //Si un block qui requiert du courant n'a pas croisé d'explorer, alors on l'eteint. Sinon on l'allume
    public void UpdateBlocksRequiringPower()
    {
        foreach (BlockLink block in AllBlocksRequiringPower)
        {
            if (block.isConsideredUnpowered == true)
            {
                block.currentPower = 0;
            }
        }
    }

    IEnumerator RecalculatePropagation()
    {
        foreach (Generator generator in AllGenerators)
        {
            generator.Invoke("OnBlockUpdate",0);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    IEnumerator ResetBlocksPower()
    {
        foreach (BlockLink block in AllBlocksRequiringPower)
        {
            block.isConsideredUnpowered = true;
        }
        yield return null;
    }
}
