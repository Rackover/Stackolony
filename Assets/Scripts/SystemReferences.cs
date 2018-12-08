using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemReferences : MonoBehaviour {
    public List<Generator> AllGenerators;
    public List<BlockLink> AllBlocksRequiringPower;
    public List<BlockLink> AllBlockLinks;
    public List<WorkingHours> AllTimeRelatedBlocks;

    public Temporality temporalityManager;

    private void Awake()
    {
        AllGenerators = new List<Generator>();
        AllBlocksRequiringPower = new List<BlockLink>();
    }

    //Met a jour le system de jeu
    public void UpdateSystem()
    {
        Debug.Log("UPDATING SYSTEM");
        StartCoroutine(RecalculatePropagation());
        StartCoroutine(ResetBlocksPower());
    }

    public void UpdateCycle() {
        foreach (BlockLink block in AllBlockLinks) {
            block.NewCycle();
        }
    }

    public void CheckWorkingHours() {
        foreach (WorkingHours workingHour in AllTimeRelatedBlocks) {
            if (temporalityManager.cycleProgressionInPercent > workingHour.startHour && workingHour.hasStarted == false) {
                workingHour.StartWork();
            } else if (temporalityManager.cycleProgressionInPercent > workingHour.endHour && workingHour.hasStarted == true) {
                workingHour.EndWork();
            }
        }
    }

    //Si un block qui requiert du courant n'a pas croisé d'explorer, alors on l'eteint. Sinon on l'allume
    public void UpdateBlocksRequiringPower()
    {
        foreach (BlockLink block in AllBlocksRequiringPower)
        {
            if (block.isConsideredUnpowered == true)
            {
                block.currentPower = 0;
                block.ChangePower(0);
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
