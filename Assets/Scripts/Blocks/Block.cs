using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockState{ Powered, OnFire, OnRiot }
public enum Profile{ Scientist, Worker, Military, Artist, Tourist }
public enum Ressource{ Energy, Mood, Food }
public enum Quality{ Low, Medium, High }

public class Block : MonoBehaviour {

    [Header("Referencies")]
    public BoxCollider collider;
    public Container container;
	public BlockScheme block;
	public GameObject blockObject;
    public GameObject particleVisuals;

    [Header("Particules")]
	public GameObject unpoweredEffect;
    public GameObject onFireEffect;
    public GameObject bridge;

    // TEMPORARY ASSET GENERATION

    [HideInInspector]   public Vector3Int gridCoordinates = new Vector3Int(0, 0, 0);
    [HideInInspector]   public int positionInTower; //0 = tout en bas
    [HideInInspector]   public GridManagement gridManager;
    [HideInInspector]   public FlagReader flagReader;
    [HideInInspector]   public SystemManager systemRef;
    [HideInInspector]   public Library lib;
    public GameManager gameManager;

    [Header("Lists")]
    public List<Flag> activeFlags = new List<Flag>();
	public List<BlockState> states = new List<BlockState>();

    [Header("Values")]
	public int currentPower;
    public bool isConsideredUnpowered; //Used when updating energy system
    public bool isConsideredDisabled; //Used when updating spatioport

    public void Awake()
    {
        isConsideredUnpowered = false;
        flagReader = FindObjectOfType<FlagReader>();
        gridManager = FindObjectOfType<GridManagement>();
        if (collider == null) collider = gameObject.GetComponent<BoxCollider>();

        SystemManager foundSystemRef = FindObjectOfType<SystemManager>();
        lib = FindObjectOfType<Library>();

        if (foundSystemRef == null)
        {
            GameObject newSystemRefGO = new GameObject();
            newSystemRefGO.name = "SystemReferences";
            systemRef = newSystemRefGO.AddComponent<SystemManager>();
        }
        else
        {
            systemRef = foundSystemRef;
        }

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
        systemRef.AllBlockLinks.Add(this);
        if (block.consumption > 0)
        {
            systemRef.AllBlocksRequiringPower.Add(this);
        }

        if (blockObject == null)
        {
            blockObject = Instantiate(block.model, transform.position, Quaternion.identity, transform);
            blockObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
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
                    Debug.Log("Removing powered particles");
                    break;

                case BlockState.OnFire:
					onFireEffect.SetActive(true);
                    GameManager.instance.sfxManager.PlaySound("StartingFire");
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
                    Debug.Log("adding powered particles");
                    break;
                case BlockState.OnFire:
					onFireEffect.SetActive(false);
                    GameManager.instance.sfxManager.PlaySound("StoppingFire");
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
        if(blockObject != null) {
            if (!on) {
                blockObject.gameObject.SetActive(false);
                particleVisuals.SetActive(false);
            }
            else
            {
                blockObject.gameObject.SetActive(true);
                particleVisuals.SetActive(true);
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
                gridManager.UpdateBridgePosition(bridgeInfo, gridCoordinates.y);
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
                gridManager.IndexToWorldPosition(gridCoordinates),
                elapsedTime / time
            );
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.position = gridManager.IndexToWorldPosition(gridCoordinates);
        yield return null;
    }

    public void UpdateFlags()
    {
        if(states.Contains(BlockState.Powered))
        {
            foreach (Flag flag in activeFlags)
            {
                flag.UpdateFlag();
            }
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