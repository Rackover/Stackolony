using UnityEngine;
using UnityEngine.UI;

public class GeneratorPanel : FlagPanel 
{
	[Header("Referenciens")]
	public Text title;
	public Text text;

		
	public override void ShowFlag(Flag flag)
	{
		Generator g = (Generator)flag;
		Localization loc = GameManager.instance.localization;

		loc.SetCategory("flagName");
		text.text = loc.GetLine("generator");
		
		loc.SetCategory("flagDescription");
		text.text = string.Format(loc.GetLine("powerGeneration"), g.power.ToString());
	}
}
