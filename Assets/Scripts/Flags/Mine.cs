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
            Vector2Int pos = new Vector2Int(block.gridCoordinates.x, block.gridCoordinates.z);
            GameManager.instance.gridManagement.buildablePositions.Add(pos);

            block.Destroy();
        }
    }
    
    public System.Type GetFlagType() { return GetType(); }
    public string GetFlagDatas() { return "Mine"; }
}
