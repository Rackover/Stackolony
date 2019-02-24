using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Interface : MonoBehaviour {

    [Header("Settings")][Space(1)]
    public float refreshRate;

	[Header("Cursor")][Space(1)]
	public RectTransform cursorTransform;
	public Image cursorImage;

    [Header("Windows")]
    [Space(1)]
    public GameObject optionsWindow;
    public GameObject creditsWindow;
    public GameObject pauseWindow;
    public GameObject saveWindow;

    public void SpawnOptionsWindow()
    {
        Instantiate(optionsWindow, this.transform);
    }

    public void SpawnCreditsWindow()
    {
        Instantiate(creditsWindow, this.transform);
    }

    public void SpawnPauseWindow()
    {
        Instantiate(pauseWindow, this.transform);
    }

    public void SpawnSaveWindow()
    {
        GameObject saveWin = Instantiate(saveWindow, this.transform);
    }
}