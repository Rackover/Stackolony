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

    [HideInInspector]   public Vector3Int gridCoordinates = new Vector3Int(0, 0, 0);
    [HideInInspector]   public int positionInTower; //0 = tout en bas

    [Header("Lists")]
    public List<Flag.IFlag> activeFlags = new List<Flag.IFlag>();
	public Dictionary<State, StateBehavior> states = new Dictionary<State, StateBehavior>();
    public List<ConsumptionModifier> consumptionModifiers = new List<ConsumptionModifier>();
    public List<FlagModifier> flagModifiers = new List<FlagModifier>();
    public List<TempFlag> tempFlags = new List<TempFlag>();

    [Header("Values")]
	public int currentPower;
    public int nuisance; //Nuisance received by the block
    public bool isConsideredUnpowered; //Used when updating energy system
    public bool isConsideredDisabled; //Used when updating spatioport
    public bool isLinkedToSpatioport;
    public bool isEnabled = false;

    public void Awake()
    {
        isConsideredUnpowered = false;
        if(boxCollider == null) boxCollider = GetComponent<BoxCollider>();
    }

    public void Update()
    {
        UpdateFlags();
    }

    public void Pack()
    {
        Disable();
        //Affiche un feedback pour signaler que le bloc est inactif
        if (container.closed == false)
        {
            container.CloseContainer();
        }
    }

    public void UnPack()
    {
        Enable();
        //Affiche un feedback pour signaler que le bloc est inactif
        if (container.closed == true)
        {
            container.OpenContainer();
        }
    }

    //Called when block isn't in range of a spatioport
    public void Disable()
    {
        if (!isEnabled)
        {
            return;
        }
        isEnabled = false;
        //Desactive toutes les fonctionnalités du bloc
        DisableFlags();
    }


    //Called when block is in range of a spatioport
    public void Enable()
    {
        if (isEnabled)
        {
            return;
        }

        isEnabled = true; 

        //Active toutes les fonctionnalités du bloc
        EnableFlags();

        // Check if a state isn't going to disable the states
        RefreshStates();
    }

    public void DisableFlags() // Disable all flags of this block
    {
        foreach (Flag flag in activeFlags)
        {
            flag.Disable(); 
        }
    }

    public void EnableFlags() // Enable all flags of this block
    {
        foreach (Flag flag in activeFlags)
        {
            flag.Enable(); 
        }
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
        // The .ToArray() is used to make a copy of the array to prevent errors
        foreach (Flag flag in activeFlags.ToArray()){ flag.OnNewCycle(); }
        foreach (KeyValuePair<State, StateBehavior> state in new Dictionary<State, StateBehavior>(states))
        {
            state.Value.OnNewCycle();
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

    public int GetConsumption()
    {
        int consumption = scheme.consumption;
        foreach (ConsumptionModifier consumptionModifier in consumptionModifiers)
        {
            consumption += consumptionModifier.amount;
        }
        return consumption;
    }

    public void LoadBlock()
    {
        GameManager.instance.systemManager.AllBlocks.Add(this);
        if (GetConsumption() > 0)
        {
            GameManager.instance.systemManager.AllBlocksRequiringPower.Add(this);
        }
        visuals.NewVisual(scheme.model);

        foreach (string flag in scheme.flags)
        {
            GameManager.instance.flagReader.ReadFlag(this, flag);
        }

        Enable();
    }

    public void UnloadBlock()
    {
        GameManager.instance.systemManager.AllBlocks.Remove(this);
        if (GetConsumption() > 0)
        {
            GameManager.instance.systemManager.AllBlocksRequiringPower.Remove(this);
        }
        visuals.Hide();
        effects.Hide();

        Disable();
    }

    public void UpdatePower()
	{
		if(currentPower >= GetConsumption())
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

    public void UpdateFlags()
    {
        foreach (Flag flag in activeFlags)
        {
            flag.UpdateFlag();
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