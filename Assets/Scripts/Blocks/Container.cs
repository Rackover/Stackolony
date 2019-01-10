using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour {
    [System.NonSerialized]
    public BlockLink linkedBlock;
    public bool isOpened;
    private Animator myAnimator;
    public bool isFalling;
    private SFXManager sfxManager;


    private void Awake()
    {
        sfxManager = FindObjectOfType<SFXManager>();
        isFalling = false;
        myAnimator = GetComponent<Animator>();

        // The container usually starts closed
        CloseContainer();
        linkedBlock = transform.parent.GetComponent<BlockLink>();
    }

    public void DropSound()
    {
        sfxManager.PlaySoundAtPosition("DropContainer",this.transform.position);
        isFalling = false;
        linkedBlock.collider.enabled = true;
    }

    public void ShipSound()
    {
        sfxManager.PlaySoundAtPosition("Ship", this.transform.position + new Vector3(0, 10, 0)); //Joue le son un peu plus haut pour comprendre que le vaisseau se trouve au dessus
    }

    public void FallingSound()
    {
        sfxManager.PlaySoundWithRandomParameters("FallingContainer",1,1,0.8f,1.2f);
    }

    public void DropBlock()
    {
        myAnimator.SetTrigger("DropContainer");
        isFalling = true;
        linkedBlock.collider.enabled = false;
    }

    public void InitializeBlock()
    {
        linkedBlock.LoadBlock();
    }

    public void ToggleBlockVisuals()
    {
        linkedBlock.ToggleVisuals();
    }

    public void OpenContainer()
    {
        isOpened = true;
        myAnimator.SetBool("ContainerOpened", isOpened);
        sfxManager.PlaySoundLinked("OpenContainer",this.gameObject);
    }

    public void CloseContainer()
    {
        isOpened = false;
        myAnimator.SetBool("ContainerOpened", isOpened);
        if (!isFalling)
        {
            sfxManager.PlaySoundLinked("CloseContainer", this.gameObject);
        }
    }
}
