using UnityEngine;

[CreateAssetMenu(fileName = "NewHouseBlock", menuName = "Block/HouseBlock")]
public class HouseBlock : Block {

	[Header("House settings")]
	public int room;

	override public void OnContruct()
	{
		base.OnContruct();
		
		// do other things
	}
}
