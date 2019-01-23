using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NuisancePanel : FlagPanel 
{
	public Text range;
	public Text amount;

	public override void ShowFlag(Flag flag)
	{
		NuisanceGenerator ng = (NuisanceGenerator)flag;
		
		range.text = "Range " + ng.range.ToString();
		amount.text = "Amount " + ng.amount.ToString();
	}
}
