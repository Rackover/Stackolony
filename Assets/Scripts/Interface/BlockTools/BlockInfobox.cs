using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockInfobox : MonoBehaviour 
{
	[Header("Referencies")]
	public RectTransform self;
	public RectTransform generalBox;
	public Text nameText;
	public Text descriptionText;

	[Header("Tip")]
	public RectTransform descBox;
	public Text descText;

	[Space(1)][Header("Settings")]
	public float stateTagShift = 5f;
	/*
	public float flagPanelShift = 5f;
    public float blockSideShift = 400f;
	*/

	[Space(1)][Header("Prefabs")]
	public GameObject stateTagPrefab;
	public GameObject housePanel;
	public GameObject generatorPanel;
	public GameObject firemanStationPanel;
	public GameObject nuisancePanel;

	[Space(1)][Header("Scripts")]
	public CanvasLineRenderer line;
	Block currentSelection;

	List<StateTag> stateTags = new List<StateTag>();
	List<FlagPanel> flagPanels = new List<FlagPanel>();

	void Update()
	{
		if(GameManager.instance.cursorManagement.selectedBlock != null)
		{
			currentSelection = GameManager.instance.cursorManagement.selectedBlock;
			if(currentSelection != null) LoadBlockValues(currentSelection);
			else Hide();
		}

/* NOT WORKING LINES
		if(generalBox.gameObject.activeSelf && currentSelection != null)
		{
			Vector2 o = Camera.main.WorldToScreenPoint(currentSelection.transform.position);
			Vector2 t = self.position;
			if(o.x >= t.x)
				t = new Vector2(self.position.x + generalBox.sizeDelta.x/2 - 5f, self.position.y);
			else
				t = new Vector2(self.position.x - generalBox.sizeDelta.x/2 + 5f, self.position.y);


			line.DrawCanvasLine(o, t, 2f, Color.grey);
			for(int i = 0; i < currentSelection.activeFlags.Count; i++)
			{
				if(currentSelection.activeFlags[i] is FiremanStation)
				{
					FiremanStation firemanStation = (FiremanStation)currentSelection.activeFlags[i];
					for(int j = 0; j < firemanStation.targets.Count; j++)
					{
						line.DrawCanvasLine(Camera.main.WorldToScreenPoint(currentSelection.transform.position), Camera.main.WorldToScreenPoint(firemanStation.targets[j].transform.position), 1f, Color.blue);
					}
				}
			}
*/
	}

	public void LoadBlockValues(Block block)
	{
		Hide();
		currentSelection = block;

		Vector2 blockCanvasPosition = Camera.main.WorldToScreenPoint(block.transform.position);
		Vector2 newPos = Vector2.zero;
/*
		if(blockCanvasPosition.x >= Screen.width/2)
			newPos = new Vector2(blockCanvasPosition.x - blockSideShift, blockCanvasPosition.y);
		else
			newPos = new Vector2(blockCanvasPosition.x + blockSideShift, blockCanvasPosition.y);
*/
		if(blockCanvasPosition.x <= Screen.width/2)
			newPos = new Vector2(Screen.width - 300, Screen.height/2);
		else
			newPos = new Vector2(300, Screen.height/2);
		
		self.position = newPos;

		// Changing general box values
		generalBox.gameObject.SetActive(true);

		GameManager.instance.localization.SetCategory("blockName");
		nameText.text = GameManager.instance.localization.GetLine("block" + block.scheme.ID);
		GameManager.instance.localization.SetCategory("blockDescription");
		descriptionText.text = GameManager.instance.localization.GetLine("block" + block.scheme.ID);
		
		ShowStatesTags(block.states.ToArray());
		ShowFlagBoxes(block.activeFlags.ToArray());
	}

	void ShowStatesTags(BlockState[] states)
	{
		float stateShift = 0;
		for( int i = 0; i < states.Length; i++)
		{
			StateTag newTag = GetAvailableTag();

			if(newTag != null)
			{
				Vector2 newTagPosition = new Vector2(self.sizeDelta.x/2, self.sizeDelta.y/2 - newTag.self.sizeDelta.y/2 + stateShift - stateTagShift);
				newTag.PrintTag(states[i]);
				newTag.self.localPosition = newTagPosition;
			}
			else 
			{
				newTag = Instantiate(stateTagPrefab, self.position, Quaternion.identity, self).GetComponent<StateTag>();
				stateTags.Add(newTag);

				Vector2 newTagPosition = new Vector2(self.sizeDelta.x/2, self.sizeDelta.y/2 - newTag.self.sizeDelta.y/2 + stateShift - stateTagShift);

				stateTags[stateTags.Count - 1].PrintTag(states[i]);
				stateTags[stateTags.Count - 1].self.localPosition = newTagPosition;
			}

			stateShift -= newTag.self.sizeDelta.y;
		}
	}

	void ShowFlagBoxes(Flag[] flags)
	{
		float panelShift = -generalBox.sizeDelta.y/2;
		for( int i = 0; i < flags.Length; i++)
		{
			Vector2 newPos = Vector2.zero;
			FlagPanel fp = null;

			switch(flags[i].GetType().Name)
			{	
				case "Generator":
					fp = Instantiate(generatorPanel, self.position, Quaternion.identity, generalBox).GetComponent<GeneratorPanel>();
					fp.ShowFlag(flags[i] as Generator);
					break;

				case "FiremanStation":	
					fp = Instantiate(firemanStationPanel, self.position, Quaternion.identity, generalBox).GetComponent<FiremanStationPanel>();
					fp.ShowFlag((flags[i] as FiremanStation));
					break;

				case "House":
					fp = Instantiate(housePanel, self.position, Quaternion.identity, generalBox).GetComponent<HousePanel>();
					fp.ShowFlag((flags[i] as House));
					break;

				case "NuisanceGenerator":
					fp = Instantiate(nuisancePanel, self.position, Quaternion.identity, generalBox).GetComponent<NuisancePanel>();
					fp.ShowFlag((flags[i] as NuisanceGenerator));
					break; 
			}

			if(fp != null)
			{
				fp.self.localPosition = new Vector2(0, panelShift - fp.self.sizeDelta.y/2);
				panelShift -= fp.self.sizeDelta.y;
				flagPanels.Add(fp);
			} 
		}
	}

	float GetRequiredHeight(Text text, float width)
	{
		return Mathf.Ceil((text.text.Length * text.fontSize)/width) * text.fontSize;
	}

	StateTag GetAvailableTag()
	{
		for( int i = 0; i < stateTags.Count; i++)
		{
			if(stateTags[i].available)
			{
				return stateTags[i];
			}
		}
		return null;
	}

	public void Hide()
	{
		currentSelection = null;
		generalBox.gameObject.SetActive(false);
		descBox.gameObject.SetActive(false);

		foreach(StateTag st in stateTags){st.Hide();}
		foreach(FlagPanel fp in flagPanels){Destroy(fp.gameObject);}
		flagPanels.Clear();
	}

	public void ShowBlockSheme(BlockScheme blockScheme, Vector2 where)
	{	
		Hide();
		self.position = new Vector2(where.x, where.y);
		GameManager.instance.localization.SetCategory("blockDescription");
		descText.text = GameManager.instance.localization.GetLine("block" + blockScheme.ID);
		descBox.gameObject.SetActive(true);
	}

	public void UseBlock()
	{

	}
}
