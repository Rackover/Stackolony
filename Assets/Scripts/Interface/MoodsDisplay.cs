using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoodsDisplay : MonoBehaviour {

    public GameObject example;

    GameManager gameManager;
    List<MoodDisplay> moods = new List<MoodDisplay>();

	void Start () {
        gameManager = GameManager.instance;
        Population[] populations = gameManager.populationManager.populationTypeList;
        Canvas canvas = GetComponentInParent<Canvas>();
        float factor = canvas.scaleFactor;
        
        foreach(Population race in populations) {
            GameObject raceO = Instantiate(example.gameObject, transform);
            MoodDisplay md = raceO.GetComponent<MoodDisplay>();
            RectTransform rect = md.rect;
            md.InitializeForPopulation(race);

            moods.Add(md);
        }

        Destroy(example);
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
