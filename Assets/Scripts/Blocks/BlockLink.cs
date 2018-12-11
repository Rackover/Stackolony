﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockState{ Powered, OnFire, OnRiot }
public enum Profile{ Scientist, Worker, Military, Artist, Tourist }
public enum Ressource{ Energy, Mood, Food }
public enum Quality{ Low, Medium, High }

public class BlockLink : MonoBehaviour {

    [Header("Referencies")]
    public BoxCollider collider;
    public Container container;
	public Block block;
	public GameObject blockObject;

    [Header("Particules")]
	public GameObject unpoweredEffect;
    public GameObject onFireEffect;
    public GameObject bridge;

    // TEMPORARY ASSET GENERATION
    public Block[] blocks;

    [HideInInspector]   public Vector3Int gridCoordinates = new Vector3Int(0, 0, 0);
    [HideInInspector]   public int positionInTower; //0 = tout en bas
    [HideInInspector]   public GridManagement gridManager;
    [HideInInspector]   public FlagReader flagReader;
    [HideInInspector]   public SystemReferences systemRef;

    [Header("Lists")]
    public List<Flag> activeFlags = new List<Flag>();
	public List<BlockState> states = new List<BlockState>();

    [Header("Values")]
	public int currentPower;
    public bool isConsideredUnpowered;

    [Header("System settings")]
    public float refreshCooldown;
    public float timer = 0f;

    public void Awake()
    {
        isConsideredUnpowered = true;
        flagReader = FindObjectOfType<FlagReader>();
        gridManager = FindObjectOfType<GridManagement>();
        if (collider == null) collider = gameObject.GetComponent<BoxCollider>();
        SystemReferences foundSystemRef = FindObjectOfType<SystemReferences>();
        if (foundSystemRef == null)
        {
            GameObject newSystemRefGO = new GameObject();
            newSystemRefGO.name = "SystemReferences";
            systemRef = newSystemRefGO.AddComponent<SystemReferences>();
        }
        else
        {
            systemRef = foundSystemRef;
        }
    }

    public void NewCycle() {

    }

    //Apelle une fonction à chaque script "flag" attachés
    public void CallFlags(string function)
    {
        foreach (Flag myFlag in activeFlags)
        {
            if (function == "BeforeMovingBlock")
            {
                myFlag.BeforeMovingBlock();
            }
            else
            {
                myFlag.Invoke(function, 1);
            }
        }
    }

    public void ChangePower(int number) {
        currentPower += number;
        UpdatePower();
        if (currentPower > 0) {
            isConsideredUnpowered = false;
        }
    }

    public void LoadBlock()
    {
        if (block == null)
        {
            block = blocks[Random.Range(0, blocks.Length)];
        }
        else
        {
            systemRef.AllBlockLinks.Add(this);
            if (block.consumption > 0)
            {
                systemRef.AllBlocksRequiringPower.Add(this);
            }
        }

        if (blockObject == null)
        {
            blockObject = Instantiate(block.model, transform.position, Quaternion.identity, transform);
            blockObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        }
        else if(blockObject.activeSelf == false)
        {
            blockObject.SetActive(true);
        }
        foreach (string flag in block.flags)
        {
            flagReader.ReadFlag(this, flag);
        }
    }

    public void UnloadBlock()
    {
        if(blockObject != null)
        {
            blockObject.SetActive(false);
            unpoweredEffect.SetActive(false);
        }
    }
    
    public void ReadFlags(string[] flags)
    {
        foreach(string flag in flags)
        {
            
        }
    }

	// Check if the block is powered
	public bool IsPowered()
	{   
		if(currentPower >= block.consumption)
		{
			return true;
		}
		else
			return false;
	} 

    public void UpdatePower()
	{
		if(IsPowered())
		{
			if(!states.Contains(BlockState.Powered))
				AddState(BlockState.Powered);
		}
		else
		{
			if(states.Contains(BlockState.Powered))
				RemoveState(BlockState.Powered);
		}
	}

    public void AddState(BlockState state)
	{
		if(!states.Contains(state))
		{
			switch(state)
			{
				case BlockState.Powered:
					unpoweredEffect.SetActive(false);
					break;

                case BlockState.OnFire:
					onFireEffect.SetActive(true);
                    gridManager.sfxManager.PlaySound("StartingFire");
					break;

				default:
					Debug.Log("Unclear state");
					break;
			}
			states.Add(state);
		}
		else
			Debug.Log("This block already has this state.");
	}

	public void RemoveState(BlockState state)
	{
		if(states.Contains(state))
		{
			switch(state)
			{
				case BlockState.Powered:
					unpoweredEffect.SetActive(true);
					break;
                case BlockState.OnFire:
					onFireEffect.SetActive(false);
                    gridManager.sfxManager.PlaySound("StoppingFire");
					break;

				default:
					Debug.Log("Unclear state");
					break;
			}
			states.Remove(state);
		}
		else
			Debug.Log("This block wasn't on this state. You are doing something wrong ?");
	}

    public void ToggleVisuals()
    {
        if (blockObject.gameObject.activeSelf == true)
        {
            blockObject.gameObject.SetActive(false);
            unpoweredEffect.SetActive(false);
            onFireEffect.SetActive(false);
        }
        else
        {
            blockObject.gameObject.SetActive(true);
            unpoweredEffect.SetActive(true);
        }
    }

    public void MoveToMyPosition() //Deplace le bloc à sa position supposée
    {
        //Déplace aussi le pont qui lui est attaché s'il y en a un
        if (transform.Find("Bridge") != null)
        {
            BridgeInfo bridgeInfo = transform.Find("Bridge").GetComponent<BridgeInfo>();
            if (bridgeInfo != null)
                gridManager.updateBridgePosition(bridgeInfo, gridCoordinates.y);
        }
        StartCoroutine(MoveToPosition(0.1f));
    }

    public IEnumerator MoveToPosition(float time) //Coroutine pour déplacer le cube vers sa position
    {
        float elapsedTime = 0;
        Vector3 startingPos = transform.position;
        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(
                startingPos, 
                new Vector3(
                    gridCoordinates.x * gridManager.cellSize + 0.5f * gridManager.cellSize, 
                    0.5f + gridCoordinates.y, 
                    gridCoordinates.z * gridManager.cellSize + 0.5f * gridManager.cellSize),
                elapsedTime / time
            );
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.position = new Vector3(
        gridCoordinates.x * gridManager.cellSize + 0.5f * gridManager.cellSize,
        0.5f + gridCoordinates.y,
        gridCoordinates.z * gridManager.cellSize + 0.5f * gridManager.cellSize);
        CallFlags("AfterMovingBlock");
        yield return null;
    }
}