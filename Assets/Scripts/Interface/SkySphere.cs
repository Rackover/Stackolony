using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkySphere : MonoBehaviour {

    EnvironmentalFX fx;
    Renderer renderer;

	// Use this for initialization
	void Start () {
        fx = GameManager.instance.environmentalFX;
        renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        Color color = fx.skyboxVariation.Evaluate(GameManager.instance.temporality.GetCurrentCycleProgression() / 100f);
        renderer.material.SetColor("_EmissionColor", color);
        renderer.material.SetColor("_Color", color);
    }
}
