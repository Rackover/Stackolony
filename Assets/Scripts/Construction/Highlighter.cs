using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlighter : MonoBehaviour {

    public MeshRenderer myMeshRenderer;
    public Material greenMat;
    public Material defaultMat;

    public void SetGreenHighlighter()
    {
        myMeshRenderer.material = greenMat;
    }

    public void SetDefaultHighlighter()
    {
        myMeshRenderer.material = defaultMat;
    }

}
