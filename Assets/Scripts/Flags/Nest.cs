using UnityEngine;

public class Nest : Flag, Flag.IFlag
{    
    [Header("Nest")]
    public float health = 100f;

    public override void OnNewMicrocycle()
    {
        if(health <= 0)
        {
            block.Destroy();
        }
    }

    public override void OnNewCycle()
    {
        health += 10f;
    }

    public System.Type GetFlagType() { return GetType(); }
    public string GetFlagDatas(){ return "Nest"; }
}