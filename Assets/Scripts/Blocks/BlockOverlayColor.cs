using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockOverlayColor : MonoBehaviour {

    public MeshRenderer[] meshRenderers;
    public Block block;

    public void Activate(Color color)
    {
        foreach(MeshRenderer mesh in meshRenderers) 
        {
            mesh.material.color = color;
            mesh.enabled = true;
        }
        block.ToggleVisuals(false);
    }
    

    public void Deactivate()
    {
        foreach (MeshRenderer mesh in meshRenderers) 
        {
            mesh.enabled = false;
        }
        if(block.isEnabled) block.ToggleVisuals(true);
    }
}
