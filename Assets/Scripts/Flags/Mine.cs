using UnityEngine;

public class Mine : Flag, Flag.IFlag
{
    [Header("Mine")]
    public float health;

    public override void OnNewMicrocycle()
    {
        if(health <= 0)
        {
            block.Destroy();
        }
    }
    
    public System.Type GetFlagType() { return GetType(); }
    public string GetFlagDatas() { return "Mine"; }
}
