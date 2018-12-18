using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemReferences : MonoBehaviour {

    public List<Generator> AllGenerators;
    public List<BlockLink> AllBlocksRequiringPower;
    public List<BlockLink> AllBlockLinks;
    public List<WorkingHours> AllTimeRelatedBlocks;

    private void Awake()
    {
        AllGenerators = new List<Generator>();
        AllBlocksRequiringPower = new List<BlockLink>();
    }

    //Met a jour le system de jeu
    public void UpdateSystem()
    {
        StartCoroutine(ResetBlocksPower());
        StartCoroutine(RecalculatePropagation());
    }

    public void UpdateCycle() {
        foreach (BlockLink block in AllBlockLinks) {
            block.NewCycle();
        }
    }

    public void CheckWorkingHours() {
        foreach (WorkingHours workingHour in AllTimeRelatedBlocks) {
            if (GameManager.instance.temporality.cycleProgressionInPercent > workingHour.startHour && workingHour.hasStarted == false) {
                workingHour.StartWork();
            } else if (GameManager.instance.temporality.cycleProgressionInPercent > workingHour.endHour && workingHour.hasStarted == true) {
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
            generator.Invoke("AfterMovingBlock", 0f);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    IEnumerator ResetBlocksPower()
    {
        foreach (BlockLink block in AllBlocksRequiringPower)
        {
            block.isConsideredUnpowered = true;
            block.currentPower = 0;
        }
        yield return null;
    }
}
