using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockState{ Powered, OnFire, OnRiot, Damaged }
public enum Profile{ Scientist, Worker, Military, Artist, Tourist }
public enum Ressource{ Energy, Mood, Food }
public enum Quality{ Low, Medium, High }

public class Block : MonoBehaviour {

    [Header("Referencies")]
    public BoxCollider boxCollider;
    public Container container;
	public BlockScheme scheme;
	public BlockVisual visuals;
    public BlockEffect effects;

    public GameObject bridge;

    [HideInInspector]   public Vector3Int gridCoordinates = new Vector3Int(0, 0, 0);
    [HideInInspector]   public int positionInTower; //0 = tout en bas

    [Header("Lists")]
    public List<Flag> activeFlags = new List<Flag>();
	public List<BlockState> states = new List<BlockState>();

    [Header("Values")]
	public int currentPower;
    public int nuisance; //Nuisance received by the block
    public bool isConsideredUnpowered; //Used when updating energy system
    public bool isConsideredDisabled; //Used when updating spatioport
    public bool isLinkedToSpatioport; 

    public void Awake()
    {
        isConsideredUnpowered = false;
        if (boxCollider == null) boxCollider = gameObject.GetComponent<BoxCollider>();
    }

    public void Update()
    {
        UpdateFlags();
    }

    //Called when block isn't in range of a spatioport
    public void Disable()
    {
        //Desactive toutes les fonctionnalités du bloc
        foreach (Flag flag in activeFlags)
        {
            flag.Disable(); 
        }

        //Affiche un feedback pour signaler que le bloc est inactif
        if (container.closed == false)
        {
            container.CloseContainer();
        }
    }

    //Called when block is in range of a spatioport
    public void Enable()
    {
        //Active toutes les fonctionnalités du bloc
        foreach (Flag flag in activeFlags)
        {
            flag.Enable();
        }

        //Affiche un feedback pour signaler que le bloc est inactif
        if (container.closed == true)
        {
            container.OpenContainer();
        }
    }

    public void NewCycle() 
    {
        foreach (Flag flag in activeFlags)
        {
            flag.OnNewCycle();
        }
    }

    //Apelle une fonction à chaque script "flag" attachés
    public void CallFlags(string function)
    {
        foreach (Flag f in activeFlags)
        {
            f.Invoke(function, 0);
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
        GameManager.instance.systemManager.AllBlocks.Add(this);

        if (scheme.consumption > 0)
        {
            GameManager.instance.systemManager.AllBlocksRequiringPower.Add(this);
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

	// Check if the block is powered
	public bool IsPowered()
	{   
		if(currentPower >= scheme.consumption) return true;
		else return false;
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
					effects.Desactivate(GameManager.instance.library.unpoweredParticle);
                    break;

                case BlockState.OnFire:
					effects.Activate(GameManager.instance.library.onFireParticle);
                    //GameManager.instance.sfxManager.PlaySound("StartingFire");
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
					effects.Activate(GameManager.instance.library.unpoweredParticle);
                    break;
                case BlockState.OnFire:
					effects.Desactivate(GameManager.instance.library.onFireParticle);
                    //GameManager.instance.sfxManager.PlaySound("StoppingFire");
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

    public void ToggleVisuals(bool on)
    {
        if(visuals != null) 
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

                if(scheme.consumption != 0)
                {
                    if(currentPower <= scheme.consumption)
                    {
                        effects.Activate(GameManager.instance.library.unpoweredParticle);
                    }
                }
            }
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

    public void UpdateFlags()
    {
        foreach (Flag flag in activeFlags)
        {
            flag.UpdateFlag();
        }
    }

    public void UseFlags()
    {
        if(states.Contains(BlockState.Powered))
        {
            foreach (Flag flag in activeFlags)
            {
                flag.Use();
            }
        }
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