using UnityEngine;

public class Mine : Flag, Flag.IFlag
{
    [Header("Mine")]
    public float health;
    float sHealth;

    bool soonDestroyed = false;

    public override void Awake()
    {
        sHealth = health;
    }

    public override void OnNewMicrocycle()
    {

        if(health/sHealth < 0.05f && !soonDestroyed)
        {
            GameManager.instance.cityManager.SpawnMine();
            soonDestroyed = true;
        }

        if(health <= 0)
        {
            block.Destroy();
        }
    }
    
    public System.Type GetFlagType() { return GetType(); }
    public string GetFlagDatas() { return "Mine"; }
}
