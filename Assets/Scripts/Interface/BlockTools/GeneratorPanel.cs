using UnityEngine;
using UnityEngine.UI;

public class GeneratorPanel : FlagPanel 
{
	[Header("Referenciens")]
	public Text text;

		
	public override void ShowFlag(Flag flag)
	{
		Generator g = (Generator)flag;
		text.text = "Generated power " + g.power.ToString();
	}
}
