using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "newBlock", menuName = "BlockScheme/Basic")]
public class BlockScheme : ScriptableObject
{
    [Header("BLOCK PROPERTIES")]
    public bool isMovable = true;
    public bool isDestroyable = true;
    public bool isBuyable = true;
    public bool canBuildAbove = true;
	public bool relyOnSpatioport = true;
	public bool fireProof = false;
	public bool riotProof = false;

	[Header("Datas")]

	[Tooltip("Prefab of the visuals of the block")]
	public GameObject model;
	[Tooltip("Sound linked to this block")]
	public AudioClip sound;
	[Tooltip("The index of the block")]
    public int ID;

	[Header("Properties")]

	[Tooltip("Power usage of the block")]
	public int consumption = 1;
	[Tooltip("Sensibility of the block towards Nuisance")]
	public int sensibility = 1;

	[Header("Tags")]

	[Tooltip("Original flags of the block")]
	public string[] flags;
}
