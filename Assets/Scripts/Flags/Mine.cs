using UnityEngine;

public class Mine : Flag, Flag.IFlag
{
    [Header("Mine")]
    public float health;

    public override void OnNewMicrocycle()
    {
        if(health <= 0)
        {
            Destroy(block);
        }
    }

    public System.Type GetFlagType() { return GetType(); }
}
