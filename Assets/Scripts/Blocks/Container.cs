using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour 
{
    [Header("Referencies")]
    public GameObject visual;
    public Animator myAnimator;
    public MeshRenderer[] meshRenderers;
    public MeshRenderer[] iconRenderers;
    [Header("States")]
    public bool closed;
    public bool isFalling;

    public Block linkedBlock;

    Material containerMat;
    Material iconMat;
    Color oColor; // Original color;
    Color cColor; // Current color;

    void Awake()
    {
        if(linkedBlock == null) linkedBlock = transform.parent.GetComponent<Block>();
        if(myAnimator == null) myAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        // BOX
        containerMat = Instantiate(meshRenderers[0].material);
        containerMat.color = GameManager.instance.library.blockContainerColors[linkedBlock.block.ID];
        foreach(MeshRenderer mr in meshRenderers)
        { 
            mr.material = containerMat;
        }
        oColor = containerMat.color;

        // ICON MESH PLANE
        iconMat = Instantiate(iconRenderers[0].material);
        foreach(MeshRenderer ic in iconRenderers){ ic.material = iconMat; }
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
        
        isFalling = false;

        CloseContainer();
    }

    void Update()
    {
        if(!closed)
        {
            if (cColor.a <= 0 && visual.activeSelf) 
            {
                cColor.a = 0f;
                visual.SetActive(false);
            }
            else
            {
                cColor.a -= 0.05f;
                containerMat.color = cColor;
            }
        } 
    }

    public void DropSound()
    {
        GameManager.instance.sfxManager.PlaySoundAtPosition("DropContainer",this.transform.position);
        //isFalling = false;
    }

    public void ShipSound()
    {
        GameManager.instance.sfxManager.PlaySoundAtPosition("Ship", this.transform.position + new Vector3(0, 10, 0)); //Joue le son un peu plus haut pour comprendre que le vaisseau se trouve au dessus
    }

    public void DropBlock()
    {
        //isFalling = true;
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
        closed = false;
        myAnimator.SetBool("Closed", closed);
    }

    public void CloseContainer()
    {
        closed = true;
        cColor = oColor;

        if(containerMat != null) containerMat.color = cColor;

        visual.SetActive(true);

        linkedBlock.ToggleVisuals(false);
        myAnimator.SetBool("Closed", closed);

/*
        if (!isFalling)
        {
            GameManager.instance.sfxManager.PlaySoundLinked("CloseContainer", this.gameObject);
        }
*/
    }
}
