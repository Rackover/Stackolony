using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkingHours : Flag 
{
	public float startHour;
	public float endHour;
	public bool hasStarted;

    public void Update()
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
    public void StartWork() {
        foreach (Flag flags in gameObject.GetComponents<Flag>()) {
			if (flags != this)
			flags.Enable();
		}
		hasStarted = true;
	}

	public void EndWork() {
        foreach (Flag flags in gameObject.GetComponents<Flag>()) {
			if (flags != this)
			flags.Disable();
		}
		hasStarted = false;
	}
}
