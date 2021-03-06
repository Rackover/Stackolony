﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgePart : MonoBehaviour {

    Animator animator;
    public ParticleSystem ps;
    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetFloat("Speed", 0);
    }
    public void AnimateNextBridgePart()
    {
        animator.SetFloat("Speed", 0);
        if (transform.parent != null && transform.parent.GetComponent<BridgeAnimator>() != null)
        {
            transform.parent.GetComponent<BridgeAnimator>().IncrementAnimation();
        }
    }

    public void EnableParticles()
    {
        ps.Play();
    }
    public void KillParticles()
    {
        ps.Stop();
    }
    private void OnTriggerEnter(Collider other)
    {
        Prop prop = other.GetComponent<Prop>();
        if (prop != null)
        {
            prop.OnCollision();
        }
    }
}
