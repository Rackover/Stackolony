using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoodsDisplay : MonoBehaviour {

    public GameObject example;

    GameManager gameManager;
    List<MoodDisplay> moods = new List<MoodDisplay>();
    List<GameObject> displayers = new List<GameObject>();

	void Start () {
        gameManager = GameManager.instance;
        Population[] populations = gameManager.populationManager.populationTypeList;
        Canvas canvas = GetComponentInParent<Canvas>();
        float factor = canvas.scaleFactor;

        InitializeMoods(populations);
    }

    public void InitializeMoods(Population[] populations)
    {
        example.SetActive(true);
        moods.Clear();
        foreach(GameObject displayer in displayers) {
            Destroy(displayer);
        }
        displayers.Clear();

        foreach (Population race in populations) {
            GameObject raceO = Instantiate(example.gameObject, transform);
            MoodDisplay md = raceO.GetComponent<MoodDisplay>();
            md.InitializeForPopulation(race);
            displayers.Add(raceO);
            moods.Add(md);
        }
        example.SetActive(false);
    }

    void UpdateMoodGauges()
    {
        foreach (MoodDisplay md in moods) {
            md.UpdateDisplay();
        }
    }

    void UpdateMoodAnimations()
    {
        foreach (MoodDisplay md in moods) {
            if (gameManager.populationManager.populations[md.population].citizens.Count > 0) {
                if (md.isHidden) {
                    md.Show();
                }
                md.UpdateDisplay();
            }
            else if (!md.isHidden) {
                md.Hide();
            }
        }
    }

    public IEnumerator UpdateInterface()
    {
        UpdateMoodGauges();
        UpdateMoodAnimations();
        yield return new WaitForSeconds(FindObjectOfType<Interface>().refreshRate);
        yield return StartCoroutine(UpdateInterface());
    }


}
