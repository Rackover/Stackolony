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
	public int level = 1;
    public int complexity = 1;
	public int consumption = 1;

	[Header("Tags")]
	public string[] flags;
}
