using UnityEngine;

public class Nest : Flag, Flag.IFlag
{    
    [Header("Nest")]
    public float health = 100f;

    public override void OnNewMicrocycle()
    {
        if(health <= 0)
        {
            Vector2Int pos = new Vector2Int(block.gridCoordinates.x, block.gridCoordinates.z);
            GameManager.instance.gridManagement.buildablePositions.Add(pos);
            
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