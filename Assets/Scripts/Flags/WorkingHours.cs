using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkingHours : Flag, Flag.IFlag
{
	public float startHour;
	public float endHour;
    public System.Type affectedFlag;
	public bool hasStarted;

    public void Update()
    {
        if (isEnabled)
        {
            if (GameManager.instance.temporality.GetCurrentCycleProgression() > startHour && hasStarted == false)
            {
                StartWork();
            }
            else if (GameManager.instance.temporality.GetCurrentCycleProgression() > endHour && hasStarted == true)
            {
                EndWork();
            }
        }
    }

    public void OnDisable()
    {
        StartWork();
    }

    public void StartWork() {
        foreach (Flag flags in gameObject.GetComponents<Flag>()) {
        if (flags != this)
            if (!flags.isEnabled)
            {
                flags.Enable();
            }
		}
		hasStarted = true;
	}

	public void EndWork() {
        foreach (Flag flags in gameObject.GetComponents<Flag>()) {
			if (flags != this)
            if (flags.isEnabled)
            {
                flags.Disable();
            }
        }
		hasStarted = false;
	}

    public System.Type GetFlagType()
    {
        return GetType();
    }

    public string GetFlagDatas()
    {
        return "WorkingHours_" + startHour + "_" + endHour;
    }
}
