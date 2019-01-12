using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour 
{
    [Header("Referencies")]
    public Animator myAnimator;
    public MeshRenderer[] meshRenderers;
    public MeshRenderer[] iconRenderers;
    [Header("States")]
    public bool closed;
    public bool isFalling;

    public Material containerMat;
    public Material iconMat;

    public Block linkedBlock;

    void Awake()
    {
        if(linkedBlock == null) linkedBlock = transform.parent.GetComponent<Block>();
        if(myAnimator == null) myAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        containerMat = Instantiate(meshRenderers[0].material);
        foreach(MeshRenderer mr in meshRenderers){ mr.material = containerMat; }

        iconMat = Instantiate(iconRenderers[0].material);
        foreach(MeshRenderer ic in iconRenderers){ ic.material = iconMat; }

        isFalling = false;
        CloseContainer();
        LoadContainerVisual();
    }

    void LoadContainerVisual()
    {
        Debug.Log("LUL");
        containerMat.color = GameManager.instance.library.blockContainerColors[linkedBlock.block.ID];
        //foreach(MeshRenderer mr in meshRenderers){ mr.material = containerMat; }


        Sprite sprite = linkedBlock.block.icon;
        Texture2D croppedTexture = new Texture2D( (int)sprite.rect.width, (int)sprite.rect.height );
        
        Color[] pixels = sprite.texture.GetPixels
        (  
            (int)sprite.rect.x, 
            (int)sprite.rect.y, 
            (int)sprite.rect.width, 
            (int)sprite.rect.height 
        );

        croppedTexture.SetPixels( pixels );
        croppedTexture.Apply();

        iconMat.mainTexture = croppedTexture;

    }

    public void DropSound()
    {
        GameManager.instance.sfxManager.PlaySoundAtPosition("DropContainer",this.transform.position);
        isFalling = false;
        //linkedBlock.collider.enabled = true;
    }

    public void ShipSound()
    {
        GameManager.instance.sfxManager.PlaySoundAtPosition("Ship", this.transform.position + new Vector3(0, 10, 0)); //Joue le son un peu plus haut pour comprendre que le vaisseau se trouve au dessus
    }

    public void FallingSound()
    {
    }

    public void DropBlock()
    {
        isFalling = true;
        //linkedBlock.collider.enabled = false;

        GameManager.instance.sfxManager.PlaySoundWithRandomParameters("FallingContainer",1,1,0.8f,1.2f);
        linkedBlock.ToggleVisuals(false);

        myAnimator.SetTrigger("Drop");

    }

    public void InitializeBlock()
    {
        linkedBlock.LoadBlock();
    }

    public void OpenContainer()
    {
        linkedBlock.ToggleVisuals(true);
        GameManager.instance.sfxManager.PlaySoundLinked("OpenContainer",this.gameObject);

        myAnimator.SetBool("Closed", false);

    }

    public void CloseContainer()
    {
        linkedBlock.ToggleVisuals(false);
        myAnimator.SetBool("Closed", true);

        if (!isFalling)
        {
            GameManager.instance.sfxManager.PlaySoundLinked("CloseContainer", this.gameObject);
        }
    }
}
