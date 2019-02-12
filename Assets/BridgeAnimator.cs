﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeAnimator : MonoBehaviour {
    public List<BridgePart> bridgeParts;
    public int bridgeAnimationIndex = 0;

    public void Start()
    {
        bridgeParts = new List<BridgePart>();
        GetBridgeParts();
        Invoke("IncrementAnimation", 0.1f);
    }

    public void GetBridgeParts()
    {
        bridgeParts.Clear();
        Debug.Log("Getting bridge parts");
        foreach (Transform t in transform)
        {
            if (t.GetComponent<BridgePart>() != null)
            {
                Debug.Log("Adding a bridge parts");
                bridgeParts.Add(t.GetComponent<BridgePart>());
            }
        }
    }

    public void IncrementAnimation()
    {
        if (bridgeAnimationIndex < bridgeParts.Count)
        {
            Debug.Log("Setting animation");
            bridgeParts[bridgeAnimationIndex].GetComponent<Animator>().SetFloat("Speed", GameManager.instance.animationManager.bridgeCreationSpeed);
            bridgeAnimationIndex++;
        }
    }
}
