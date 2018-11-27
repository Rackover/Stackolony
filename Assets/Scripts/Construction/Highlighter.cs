using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlighter : MonoBehaviour {

    private MeshRenderer myMeshRenderer;
    public Material greenMat;
    public Material defaultMat;

    private void Awake()
    {
        myMeshRenderer = GetComponent<MeshRenderer>();
    }
    public void SetGreenHighlighter()
    {
        myMeshRenderer.material = greenMat;
    }

    public void SetDefaultHighlighter()
    {
        myMeshRenderer.material = defaultMat;
    }

}
