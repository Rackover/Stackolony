using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoodsDisplay : MonoBehaviour {

    public GameObject example;
    public float ySpacing;
    public float colorBlendAmount = 0.5f;

    GameManager gameManager;
    Dictionary<GameObject, Population> moods = new Dictionary<GameObject, Population>();

	void Start () {
        gameManager = GameManager.instance;
        Population[] populations = gameManager.populationManager.populationTypeList;
        Canvas canvas = GetComponentInParent<Canvas>();
        float factor = canvas.scaleFactor;

        float offset = 0;
        foreach(Population race in populations) {
            GameObject raceO = Instantiate(example.gameObject, transform);
            raceO.GetComponent<RectTransform>().position = new Vector3(
                raceO.GetComponent<RectTransform>().position.x,
                raceO.GetComponent<RectTransform>().position.y + offset* factor,
                raceO.GetComponent<RectTransform>().position.z
            );

            Image face = raceO.transform.GetChild(2).gameObject.GetComponent<Image>();
            face.sprite = race.humorSprite;
            face.color = Color.Lerp(Color.white, race.color, colorBlendAmount);
            face.color = new Color(face.color.r, face.color.g, face.color.b, 1f);

            moods[raceO] = race;

            offset -= ySpacing;
        }

        Destroy(example);
    }

    private void Update()
    {

        foreach (KeyValuePair<GameObject, Population> people in moods) {
            GameObject mood = people.Key;
            float moodValue = gameManager.populationManager.GetAverageMood(people.Value);
            Image gauge = mood.transform.GetChild(1).gameObject.GetComponent<Image>();
            gauge.fillAmount = moodValue;
            gauge.color = Color.Lerp(Color.red, Color.green, moodValue);
        }
    }
}
