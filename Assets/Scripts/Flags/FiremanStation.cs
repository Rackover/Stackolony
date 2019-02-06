using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiremanStation : Flag, Flag.IFlag
{
	[Header("Fireman Behavior")]
	public int range;
	public bool selecting;
	public List<Block> targets = new List<Block>();
	public List<Extinctor> extinctors = new List<Extinctor>();
    public int nuisanceImpact;

    override public void Use()
	{
		selecting = true;
	}

    override public void UpdateNuisanceImpact()
    {
        range += nuisanceImpact;
        nuisanceImpact = block.nuisance * block.scheme.sensibility;
        range -= nuisanceImpact;
    }

    override public void OnNewCycle()
    {
        foreach (Block target in targets) {
            target.RemoveState(BlockState.OnFire);
        }
        foreach (Extinctor extinctor in extinctors) {
            Destroy(extinctor.gameObject);
        }
    }


	public override void Enable()
	{
		base.Enable();
	}
	
	public override void OnGridUpdate()
	{
		base.OnGridUpdate();
		if(!isEnabled) return;
		GameManager.instance.missionManager.StartMission(block.gridCoordinates, "Extinguish", range);
	}

   	public System.Type GetFlagType(){ return GetType(); }
    public string GetFlagDatas(){ return "FiremanStation_" + range; }
}
