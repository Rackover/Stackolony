using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewColonyWindow : MonoBehaviour {

    public int maxCityNameLength = 32;
    public int maxGovernorNameLength = 24;

    public Text cityName;
    public Text governorName;
    public Toggle enableTutorial;
    public Text errorText;
    public Image popImage;

    public InputField cityNameInput;
    public InputField governorNameInput;

    void Start()
    {
        cityNameInput.characterLimit = maxCityNameLength;
        governorNameInput.characterLimit = maxGovernorNameLength;
        popImage.sprite = GameManager.instance.populationManager.GetRandomPopulation().moodSprites[(int)Citizen.Mood.Good];
    }

    public void DestroyWindow()
    {
        Destroy(gameObject);
    }

    public void CreateColony()
    {
        if (cityName.text.Length <= 0) {
            errorText.text = GameManager.instance.localization.GetLine("cityNameTooShort", "newColony");
        }
        else if (governorName.text.Length <= 0) {
            errorText.text = GameManager.instance.localization.GetLine("governorTooshort", "newColony");
        }
        else if (cityName.text.Length > maxCityNameLength) {
            errorText.text = GameManager.instance.localization.GetLine("cityNameTooLong", "newColony");
        }
        else if (governorName.text.Length > maxGovernorNameLength) {
            errorText.text = GameManager.instance.localization.GetLine("governorNameTooLong", "newColony");
        }
        else {
            DestroyWindow();
            GameManager.instance.cityManager.cityName = cityName.text;
            GameManager.instance.cityManager.isTutorialRun = enableTutorial.isOn;
            GameManager.instance.player.playerName = governorName.text;
            FindObjectOfType<MainMenu>().StartNewGame();
        }
    }
}
