using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NuisancePanel : FlagPanel 
{
	public Text title;
	public Text range;
	public Text amount;

	public override void ShowFlag(Flag flag)
	{
		NuisanceGenerator ng = (NuisanceGenerator)flag;
		Localization loc = GameManager.instance.localization;
		
		loc.SetCategory("flagName");
		title.text = loc.GetLine("nuisance");
		
		loc.SetCategory("flagDescription");
		range.text = string.Format(loc.GetLine("range"), ng.range.ToString());
		amount.text = string.Format(loc.GetLine("amount"), ng.amount.ToString());
	}
}
