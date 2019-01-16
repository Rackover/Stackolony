using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModesToolbar : MonoBehaviour {

    CursorManagement cursorMan;

    private void Start()
    {
        cursorMan = GameManager.instance.cursorManagement;
    }

    /// UI FUNCTIONS
    /// To be fired ONLY from interface buttons
    /// do NOT use in code
    public void SwitchToDragModeFromButton()
    {
        cursorMan.SwitchMode(CursorManagement.cursorMode.Default);
    }
    public void SwitchToBuildModeFromButton()
    {
        cursorMan.SwitchMode(CursorManagement.cursorMode.Build);
    }
    public void SwitchToDeleteModeFromButton()
    {
        cursorMan.SwitchMode(CursorManagement.cursorMode.Delete);
    }
    public void SwitchToBridgeModeFromButton()
    {
        cursorMan.SwitchMode(CursorManagement.cursorMode.Bridge);
    }
}
