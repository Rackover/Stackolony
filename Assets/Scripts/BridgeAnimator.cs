using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeAnimator : MonoBehaviour {
    public List<BridgePart> bridgeParts;
    public int bridgeAnimationIndex = 0;

    public void Start()
    {
        bridgeParts = new List<BridgePart>();
        GetBridgeParts();
        StartCoroutine(StartAnimation());
    }

    public IEnumerator StartAnimation()
    {
        yield return new WaitForEndOfFrame();
        IncrementAnimation();
    }

    public void GetBridgeParts()
    {
        bridgeParts.Clear();
        foreach (Transform t in transform)
        {
            if (t.GetComponent<BridgePart>() != null)
            {
                bridgeParts.Add(t.GetComponent<BridgePart>());
            }
        }
    }

    public void IncrementAnimation()
    {
        if (bridgeAnimationIndex < bridgeParts.Count)
        {
            //Joue le son
            GameManager.instance.soundManager.Play("CreateBridge");

            bridgeParts[bridgeAnimationIndex].GetComponent<Animator>().SetFloat("Speed", GameManager.instance.animationManager.bridgeCreationSpeed);
            bridgeAnimationIndex++;
        }
    }
}
