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

	[Space(1)][Header("Settings")]
	public float stateTagShift = 5f;
	public float flagPanelShift = 5f;

	[Space(1)][Header("Prefabs")]
	public GameObject stateTagPrefab;
	public GameObject housePanel;
	public GameObject generatorPanel;
	public GameObject firemanStationPanel;

	[Space(1)][Header("Scripts")]
	public CanvasLineRenderer line;
	BlockLink currentSelection;

	List<StateTag> stateTags = new List<StateTag>();
	List<FlagPanel> flagPanels = new List<FlagPanel>();

	public void LoadBlockValues(BlockLink block)
	{
		Hide();
		currentSelection = block;

		Vector2 blockCanvasPosition = Camera.main.WorldToScreenPoint(block.transform.position);
		Vector2 newPos = Vector2.zero;

		if(blockCanvasPosition.x >= Screen.width/2)
			newPos = new Vector2(blockCanvasPosition.x - 200f, blockCanvasPosition.y);
		else
			newPos = new Vector2(blockCanvasPosition.x + 200f, blockCanvasPosition.y);
		
		self.position = newPos;

		// Changing general box values
		generalBox.gameObject.SetActive(true);
		nameText.text = block.block.title;
		descriptionText.text = block.block.description;

		// Changing box size
		generalBox.sizeDelta = new Vector2(generalBox.sizeDelta.x, GetRequiredHeight(descriptionText, generalBox.sizeDelta.x));
		
		ShowStatesTags(block.states.ToArray());
		ShowFlagBoxes(block.activeFlags.ToArray());
	}

	void ShowStatesTags(BlockState[] states)
	{
		float stateShift = -stateTagShift;
		for( int i = 0; i < states.Length; i++)
		{
			StateTag newTag = GetAvailableTag();

			if(newTag != null)
			{
				Vector2 newTagPosition = new Vector2(generalBox.sizeDelta.x/2, generalBox.sizeDelta.y/2 - newTag.self.sizeDelta.y/2 + stateShift);
				newTag.PrintTag(states[i]);
				newTag.self.localPosition = newTagPosition;
			}
			else 
			{
				newTag = Instantiate(stateTagPrefab, self.position, Quaternion.identity, generalBox).GetComponent<StateTag>();
				stateTags.Add(newTag);

				Vector2 newTagPosition = new Vector2(generalBox.sizeDelta.x/2, generalBox.sizeDelta.y/2 - newTag.self.sizeDelta.y/2 + stateShift);

				stateTags[stateTags.Count - 1].PrintTag(states[i]);
				stateTags[stateTags.Count - 1].self.localPosition = newTagPosition;
			}

			stateShift -= newTag.self.sizeDelta.y + stateTagShift;
		}
	}

	void ShowFlagBoxes(Flag[] flags)
	{
		float panelShift = -generalBox.sizeDelta.y/2;
		for( int i = 0; i < flags.Length; i++)
		{
			Vector2 newPos = Vector2.zero;
			switch(flags[i].GetType().Name)
			{	
				case "Generator":
					GeneratorPanel gp = Instantiate(generatorPanel, self.position, Quaternion.identity, generalBox).GetComponent<GeneratorPanel>();

					// Moving Panel
					gp.self.localPosition = new Vector2(0, panelShift - gp.self.sizeDelta.y/2);
					panelShift -= gp.self.sizeDelta.y;

					// Modifying values
					gp.text.text = (flags[i] as Generator).power.ToString();

					flagPanels.Add(gp);
					break;

				case "FiremanStation":	
					FiremanStationPanel fsp = Instantiate(firemanStationPanel, self.position, Quaternion.identity, generalBox).GetComponent<FiremanStationPanel>();
					
					// Moving Panel
					fsp.self.localPosition = new Vector2(0, panelShift - fsp.self.sizeDelta.y/2);
					fsp.button.onClick.AddListener(currentSelection.UseFlags);
					panelShift -= fsp.self.sizeDelta.y;

					// Modifying values

					flagPanels.Add(fsp);
					break;

				case "House":
					HousePanel hp = Instantiate(housePanel, self.position, Quaternion.identity, generalBox).GetComponent<HousePanel>();
					hp.ShowFlag((flags[i] as House));

					hp.self.localPosition = new Vector2(0, panelShift - hp.self.sizeDelta.y/2);

					flagPanels.Add(hp);
					break; 

				default:
					Debug.LogWarning("This flag dosn't need any additional boxes");
					break;
			}
		}
	}

	float GetRequiredHeight(Text text, float width)
	{
		return Mathf.Ceil((text.text.Length * text.fontSize)/width) * text.fontSize;
	}

	void Update()
	{
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
		}
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

		foreach(StateTag st in stateTags){st.Hide();}
		foreach(FlagPanel fp in flagPanels){Destroy(fp.gameObject);}
		flagPanels.Clear();

	}

	public void UseBlock()
	{

	}
}
