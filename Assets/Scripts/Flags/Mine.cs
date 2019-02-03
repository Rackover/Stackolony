using UnityEngine;

public class Mine : Flag, Flag.IFlag
{
    public System.Type GetFlagType()
    {
        return GetType();
    }


    public string GetFlagDatas()
    {
        return "Mine";
    }
}
