using UnityEngine;

[CreateAssetMenu(fileName = "newBlock", menuName = "Block/Basic")]
public class Block : ScriptableObject
{
	[Header("BLOCK PROPERTIES")]
	[Header("Visuals")]
	public string title = "Block Name";
	public string label = "Block Label";
	public string description = "This needs a proper description";
	public GameObject model;

	[Header("Properties")]
	public float lifeSpan = 1;
	public int level = 1;
	public int price = 1;
	public float powerRequired = 1f;

	[Header("BLOCK BEHAVIOR")]
	[Header("If your block create jobs, fill that up.")]
	public Job[] jobs;
	[Header("If your block generated something, fill that up.")]
	public Production[] Productions;

	[HideInInspector] public float currentPower;
	[HideInInspector] public float currentLife;

	[HideInInspector] public int currentUsers;

	[HideInInspector] public bool onFire = false;
	[HideInInspector] public bool onStrike = false;
	[HideInInspector] public BlockBehavior manager;

	virtual public void OnContruct()
	{
		// Play sound construct
	}
	virtual public void OnSelected()
	{
		// Play sound selected
		// Display tipbox with my datas
	}
	virtual public void OnDestroy()
	{

	}

	virtual public void UpdateBlock()
	{
		if(IsPowered() && !onFire && !onStrike)
		{

		}
	}

	virtual public bool IsPowered()
	{
		if(currentPower >= powerRequired){return true;}
		return false;
	} 
}
