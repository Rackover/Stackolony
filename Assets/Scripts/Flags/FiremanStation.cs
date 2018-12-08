using UnityEngine;

public class FiremanStation : Flag 
{
	public int range;
	public BlockLink target;

	public void OnBlockUse(BlockLink newTarget)
	{
		target = newTarget;
	}

	//Fonction appelée à chaque début de cycle
    override public void OnNewCycle()
    {
		target.RemoveState(BlockState.OnFire);
    }
}
