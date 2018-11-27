using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



[CreateAssetMenu(fileName = "newBlock", menuName = "Block/Basic")]
public class Block : ScriptableObject
{
	[HideInInspector] public BlockLink link;

	[Header("BLOCK PROPERTIES")]
	[Header("Datas")]
	public string title = "Block Name";
	[TextArea] public string description = "This needs a proper description";
	public GameObject model;
    public Sprite icon;

	[Header("Properties")]
	public float lifeSpan = 1;
	public int level = 1;
	public int price = 1;
	public float powerRequired = 1f;
    public int complexity;

	[Header("BLOCK FUNCTIONS")]
	public Occupation[] occupations;
	public Generation[] generations;
}
