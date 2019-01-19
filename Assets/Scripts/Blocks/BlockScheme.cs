using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "newBlock", menuName = "BlockScheme/Basic")]
public class BlockScheme : ScriptableObject
{
	[HideInInspector] public Block link;

    [Header("BLOCK PROPERTIES")]
    public bool isMovable = true;
    public bool isDestroyable = true;
    public bool isStorable = true;
    public bool isBuyable = true;
    public bool canBuildAbove = true;


	[Header("Datas")]
	public string title = "BlockScheme Name";
	[TextArea] public string description = "This needs a proper description";
	
	public GameObject model;
    public Sprite icon;
    public int ID;

	[Header("Properties")]
	public int level = 1;
    public int complexity = 1;
	public int consumption = 1;

	[Header("Tags")]
	public string[] flags;
}
