using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockState{ Powered, OnFire, OnRiot }
public enum Profile{ Scientist, Worker, Military, Artist, Tourist }
public enum Ressource{ Energy, Mood, Food }
public enum Quality{ Low, Medium, High }

[System.Serializable]
public class Occupation 
{
	//public string jobName;
	public int spotCount;
	public Profile profile;
}

[System.Serializable]
public class Generation 
{
	public int range;
	public float power;
	public Ressource ressource;
}

public class BlockLink : MonoBehaviour {

    [Header("Referencies")]
    public BoxCollider collider;
    public Container myContainer;
	public Block myBlock;
	public GameObject myBlockObject;
	public GameObject powerParticule;
    // TEMPORARY ASSET GENERATION
    public Block[] blocks;

    [HideInInspector]   public Vector3Int gridCoordinates = new Vector3Int(0, 0, 0);
    [HideInInspector]   public int positionInTower; //0 = tout en bas
    [HideInInspector]   public GridManagement gridManager;

    [Header("Lists")]
	public List<BlockState> states = new List<BlockState>();
    public List<Generator> generators = new List<Generator>();
    public List<Occupator> occupators = new List<Occupator>();

    [Header("Values")]
	public float currentPower;
	public float currentLife;
	public int currentUsers;

    [Header("System settings")]
    public float refreshCooldown;
    public float timer = 0f;

    public void Awake()
    {
        gridManager = FindObjectOfType<GridManagement>();
        if (collider == null) collider = gameObject.GetComponent<BoxCollider>();
    }

    public void LoadBlock()
    {
        if(myBlock == null)
            myBlock = blocks[Random.Range(0, blocks.Length)];

        if (myBlockObject == null)
        {
            myBlockObject = Instantiate(myBlock.model, transform.position, Quaternion.identity, transform);
            myBlockObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        }
        else if(myBlockObject.activeSelf == false)
        {
            myBlockObject.SetActive(true);
        }

        /*
        if (myBlock.generations.Length > 0)
        {
            for( int i = 0; i < myBlock.generations.Length; i++ )
            {
                generators.Add(gameObject.AddComponent<Generator>());
                generators[generators.Count - 1].link = this;
                generators[generators.Count - 1].myGeneration = myBlock.generations[i];
            }
        }

        if(myBlock.occupations.Length > 0)
        {
            for( int i = 0; i < myBlock.occupations.Length; i++ )
            {
                occupators.Add(gameObject.AddComponent<Occupator>());
                occupators[occupators.Count - 1].link = this;
                occupators[occupators.Count - 1].myOccupation = myBlock.occupations[i];
            }
        }
        */
    }

    public void UnloadBlock()
    {
        if(myBlockObject != null)
        {
            myBlockObject.SetActive(false);
            powerParticule.SetActive(false);
        }
    }

    public void Update()
    {
        timer += Time.deltaTime;
 
        if (timer >= refreshCooldown) 
        {
            timer = 0.0f;
            UpdatePower();
            UpdateGenerators();
        }
    }

    public void UpdateGenerators()
    {
        foreach(Generator g in generators)
        {
            g.RefreshGainers();
        }
    }
    
	// Check if the block is powered
	public bool IsPowered()
	{   
		if(currentPower >= myBlock.powerRequired)
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
					powerParticule.SetActive(false);
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
					powerParticule.SetActive(true);
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
        if (myBlockObject.gameObject.activeSelf == true)
        {
            myBlockObject.gameObject.SetActive(false);
            powerParticule.SetActive(false);
        }
        else
        {
            myBlockObject.gameObject.SetActive(true);
            powerParticule.SetActive(true);
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
        yield return null;
    }
}