using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Profile{ Scientist, Worker, Military, Artist, Tourist }
public enum State{ OnFire, OnRiot, Damaged, Powered, Unpowered }
public enum Ressource{ Energy, Mood, Food }
public enum Quality{ Low, Medium, High }

public class Block : MonoBehaviour
{

    [Header("Referencies")]
    public BoxCollider boxCollider;
    public Container container;
	public BlockScheme scheme;
	public BlockVisual visuals;
    public BlockEffect effects;
    public BlockOverlayColor overlayVisuals;
    public GameObject bridge;
    Library library;

    [HideInInspector]   public Vector3Int gridCoordinates = new Vector3Int(0, 0, 0);
    [HideInInspector]   public int positionInTower; //0 = tout en bas

    [Header("Lists")]
    public List<Flag.IFlag> activeFlags = new List<Flag.IFlag>();
	public Dictionary<State, StateBehavior> states = new Dictionary<State, StateBehavior>();

    [Header("Values")]
	public int currentPower;
    public int nuisance; //Nuisance received by the block
    public bool isConsideredUnpowered; //Used when updating energy system
    public bool isConsideredDisabled; //Used when updating spatioport
    public bool isLinkedToSpatioport; 

    public void Awake()
    {
        isConsideredUnpowered = false;
        if(boxCollider == null) boxCollider = GetComponent<BoxCollider>();
        library = GameManager.instance.library;
    }

    //Called when block isn't in range of a spatioport
    public void Disable()
    {
        RefreshStates();

        if (container.closed == false)
        {
            container.CloseContainer();   
        }

        UpdatePower();
    }

    //Called when block is in range of a spatioport
    public void Enable()
    {
        RefreshStates();

        if (container.closed == true)
        {
            container.OpenContainer();
        }
        
        UpdatePower();
    }

    public void OnGridUpdate()
    {
        foreach (Flag flag in activeFlags)
        {
            flag.OnGridUpdate(); 
        }
        foreach (KeyValuePair<State, StateBehavior> state in new Dictionary<State, StateBehavior>(states))
        {
            state.Value.OnGridUpdate();
        }
    }     

    public void EnableFlags() // Enable all flags of this block
    {
        foreach (Flag flag in activeFlags) { flag.Enable(); }
    }

    public void DisableFlags() // Disable all flags of this block
    {
        foreach (Flag flag in activeFlags) { flag.Disable(); }
    }

    public void RefreshStates() // Refresh the block depending on it's states
    {
        foreach (KeyValuePair<State, StateBehavior> state in new Dictionary<State, StateBehavior>(states))
        {
            if(state.Value.disabler)
            {
                DisableFlags();
                break;
            }
        }
    }
    
    public void AddState(State state)
    {
		Type type = Type.GetType(state.ToString());
        if(!states.ContainsKey(state))
        {
            states.Add(state, (StateBehavior)gameObject.AddComponent(type));
        }
    }

    public void RemoveState(State state)
    {
		if(states.ContainsKey(state))
		{
            states[state].Remove();
        }
    }

    public void Destroy()
    {
        GameManager.instance.gridManagement.DestroyBlock(gridCoordinates);
    }

    public void OnNewCycle()
    {
        foreach (Flag flag in activeFlags.ToArray()){ flag.OnNewCycle(); }
        foreach (KeyValuePair<State, StateBehavior> state in new Dictionary<State, StateBehavior>(states))
        {
            state.Value.OnNewMicrocycle();
        }
    }

    public void OnNewMicroycle()
    {
        foreach (Flag flag in activeFlags.ToArray()){ flag.OnNewCycle(); }
        foreach (KeyValuePair<State, StateBehavior> state in new Dictionary<State, StateBehavior>(states))
        {
            state.Value.OnNewMicrocycle();
        }
    }

    public void ChangePower(int number) 
    {
        currentPower = number;
        UpdatePower();
        if (currentPower > 0) 
        {
            isConsideredUnpowered = false;
        }
    }

    public void AddPower(int number) 
    {
        currentPower += number;
        UpdatePower();
        if (currentPower > 0) {
            isConsideredUnpowered = false;
        }
    }

    public void LoadBlock()
    {
        GameManager.instance.systemManager.AllBlocks.Add(this);
        if (scheme.consumption > 0)
        {
            GameManager.instance.systemManager.AllBlocksRequiringPower.Add(this);
            UpdatePower();
        }

        visuals.NewVisual(scheme.model);

        foreach (string flag in scheme.flags)
        {
            GameManager.instance.flagReader.ReadFlag(this, flag);
        }
    }

    public void UnloadBlock()
    {
        visuals.Hide();
        effects.Hide();
    }

    public void UpdatePower()
	{
		if(currentPower >= scheme.consumption)
		{
			AddState(State.Powered);
		}
		else
		{
			AddState(State.Unpowered);
        }
	}

    public void ToggleVisuals(bool on)
    {
        if(visuals != null && effects != null) 
        {
            if (!on) 
            {
                visuals.Hide();
                effects.Hide();
            }
            else
            {
                visuals.Show();
                effects.Show();
                UpdatePower();
            }
        }
    }

    public void CallFlags(string function)
    {
        foreach (Flag f in activeFlags)
        {
            f.Invoke(function, 0);
        }
    }

    public void MoveToMyPosition() //Deplace le bloc à sa position supposée
    {
        //Déplace aussi le pont qui lui est attaché s'il y en a un
        if (transform.Find("Bridge") != null)
        {
            BridgeInfo bridgeInfo = transform.Find("Bridge").GetComponent<BridgeInfo>();
            if (bridgeInfo != null)
                GameManager.instance.gridManagement.UpdateBridgePosition(bridgeInfo, gridCoordinates.y);
        }
        StartCoroutine(MoveToPosition());
    }

    private IEnumerator MoveToPosition(float time = 0) //Coroutine pour déplacer le cube vers sa position
    {
        float elapsedTime = 0;
        Vector3 startingPos = transform.position;
        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(
                startingPos,
                GameManager.instance.gridManagement.IndexToWorldPosition(gridCoordinates),
                elapsedTime / time
            );
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.position = GameManager.instance.gridManagement.IndexToWorldPosition(gridCoordinates);
        yield return null;
    }

    public void UpdateName()
    {
        gameObject.name = "BlockScheme[" + gridCoordinates.x + ";" + gridCoordinates.y + ";" + gridCoordinates.z + "]";
    }

    /// <summary>
    /// Unsafe function ; only call if the grid has already been updated with the correct coordinates.
    /// </summary>
    /// <param name="coordinates"></param>
    public void MoveTo(Vector3Int coordinates)
    {
        gridCoordinates = coordinates;
        UpdateName();
        MoveToMyPosition();
    }
}