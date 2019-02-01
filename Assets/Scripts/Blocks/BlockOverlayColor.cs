using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockOverlayColor : MonoBehaviour {

    public MeshRenderer[] meshRenderers;
    public BlockVisual blockVisual;
    public BlockEffect blockFx;

    public void Activate(Color color)
    {
        foreach(MeshRenderer mesh in meshRenderers) {
            mesh.material.color = color;
            mesh.enabled = true;
        }
        blockVisual.Hide();
        blockFx.Hide();
    }

    public void Deactivate()
    {
        foreach (MeshRenderer mesh in meshRenderers) {
            mesh.enabled = false;
        }
        blockVisual.Show();
        blockFx.Show();
    }
}
