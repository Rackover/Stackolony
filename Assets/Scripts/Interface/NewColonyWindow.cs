using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewColonyWindow : MonoBehaviour {

    public int maxCityNameLength = 32;
    public int maxGovernorNameLength = 24;

    public GameObject parentGameObject;
    public Text cityName;
    public Text governorName;
    public Toggle enableTutorial;
    public Text errorText;
    public RawImage popImage;

    public InputField cityNameInput;
    public InputField governorNameInput;

    Displayer preview;

    void Start()
    {
        cityNameInput.characterLimit = maxCityNameLength;
        governorNameInput.characterLimit = maxGovernorNameLength;

        Population pop = GameManager.instance.populationManager.GetRandomPopulation();
        if (!GameManager.instance.player.options.GetBool("animatedCitizens")) {
            float ratio = pop.moodSprites[0].rect.width / pop.moodSprites[0].rect.height;
            popImage.texture = pop.moodSprites[(int)Citizen.Mood.Good].texture;
            popImage.transform.parent.gameObject.GetComponent<AspectRatioFitter>().aspectRatio = ratio;
        }
        else {
            preview = GameManager.instance.displayerManager.SetRotationFeed(pop.prefab, popImage, 180, 0, 3, 30, 512);
            preview.GetModel().transform.GetChild(0).gameObject.GetComponent<Animator>().Play("LookAround");
        }
    }

    public void DestroyWindow()
    {
        if (preview != null) {
            preview.Unstage();
        }
        Destroy(parentGameObject);
    }

    public void CreateColony()
    {
        if (cityName.text.Length <= 0) {
            errorText.text = GameManager.instance.localization.GetLine("cityNameTooShort", "newColony");
        }
        else if (governorName.text.Length <= 0) {
            errorText.text = GameManager.instance.localization.GetLine("governorNameTooShort", "newColony");
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
