using UnityEngine;

public class Nest : Flag, Flag.IFlag
{
    public System.Type GetFlagType() { return GetType(); }
}