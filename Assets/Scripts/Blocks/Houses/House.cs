using UnityEngine;

[CreateAssetMenu(fileName = "NewHouse", menuName = "Block/House")]
public class House : Block 
{
	[Header("HOUSE PROPERTIES")]
	public Library.Quality quality;
	public int spotCount;
}
