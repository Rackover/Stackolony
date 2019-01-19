using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodProvider: Flag
{
    public int range;
    public float foodTotal;
    public float foodLeft;

    public override void Awake()
    {
        base.Awake();
        GameManager.instance.systemManager.AllFoodProviders.Add(this);
    }

    public override void OnDestroy()
    {
        GameManager.instance.systemManager.AllFoodProviders.Remove(this);
        base.OnDestroy();
    }

    public void GenerateFood()
    {
        if (isEnabled)
        {
            GameManager.instance.missionManager.StartMission(block.gridCoordinates, "DistributeFood", range, 0, typeof(House));
        }
    }
}
