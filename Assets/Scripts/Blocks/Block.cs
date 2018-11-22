using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockState{ Powered, OnFire, OnRiot }

[CreateAssetMenu(fileName = "newBlock", menuName = "Block/Basic")]
public class Block : ScriptableObject
{
	public BlockBehaviour behaviour;

	[Header("BLOCK PROPERTIES")]
	[Header("Datas")]
	public string title = "Block Name";
	public string description = "This needs a proper description";
	public GameObject model;

	[Header("Properties")]
	public float lifeSpan = 1;
	public int level = 1;
	public int price = 1;
	public float powerRequired = 1f;

	[Header("States")]
	public List<BlockState> states = new List<BlockState>();
	[Header("Values")]
	public float currentPower;
	public float currentLife;
	public int currentUsers;

	[Header("BLOCK BEHAVIOR")]
	[Header("If your block create jobs, fill that up.")]
	public Job[] jobs;
	[Header("If your block generated something, fill that up.")]
	public Production[] Productions;

	virtual public void LoadBlock()
	{
	}
	virtual public void UnloadBlock()
	{
	}

	virtual public void UpdateBlock()
	{
		UpdatePower();
	}

	public void UpdatePower()
	{
		if(IsPowered())
		{
			if(!states.Contains(BlockState.Powered))
			{
				AddState(BlockState.Powered);
			}
		}
		else
		{
			if(states.Contains(BlockState.Powered))
			{
				RemoveState(BlockState.Powered);
			}
		}
	}
	

	// Check if the block is powered
	virtual public bool IsPowered()
	{
		if(currentPower >= powerRequired)
		{
			return true;
		}
		else
			return false;
	} 

	virtual public void AddState(BlockState state)
	{
		if(!states.Contains(state))
		{
			switch(state)
			{
				case BlockState.Powered:
					behaviour.powerParticule.SetActive(false);
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

	virtual public void RemoveState(BlockState state)
	{
		if(states.Contains(state))
		{
			switch(state)
			{
				case BlockState.Powered:
					behaviour.powerParticule.SetActive(true);
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
}
