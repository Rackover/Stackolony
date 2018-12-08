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
	public float stateTagShift = 20f;
	public CanvasLineRenderer line;

	
	[Space(1)][Header("States")]
	public RectTransform statesHolder;
	public GameObject stateTagPrefab;

	[Space(1)][Header("Additional Panel")]
	public AdditionalPanel additionalPanel;

	BlockLink currentSelection;

	List<StateTag> stateTags = new List<StateTag>();
	List<AdditionalPanel> additionalPanels = new List<AdditionalPanel>();

	public void LoadBlockValues(BlockLink block)
	{
		currentSelection = block;

		// Drawing line
		line.DrawCanvasLine(Camera.main.WorldToScreenPoint(block.transform.position), self.position, 2f, Color.black);

		// Changing general box values
		generalBox.gameObject.SetActive(true);
		nameText.text = block.myBlock.title;
		descriptionText.text = block.myBlock.description;

		// Changing box size
		generalBox.sizeDelta = new Vector2(generalBox.sizeDelta.x, GetRequiredHeight(descriptionText, generalBox.sizeDelta.x));

		// Hiding stateTags
		foreach(StateTag st in stateTags){st.Hide();}
		// Showing new stateTags
		ShowStatesTags(block.states.ToArray());
		// Showing additional panels
		ShowFlagBoxes(block.activeFlags.ToArray());
	}

	void ShowStatesTags(BlockState[] states)
	{
		float stateShift = 0f;
		for( int i = 0; i < states.Length; i++)
		{
			StateTag newTag = GetAvailableTag();
			if(newTag != null)
			{
				newTag.PrintTag(states[i]);
				newTag.self.localPosition = new Vector3(0f, stateShift, 0f);
			}
			else 
			{
				stateTags.Add(Instantiate(stateTagPrefab, statesHolder.position, Quaternion.identity, statesHolder).GetComponent<StateTag>());
				stateTags[stateTags.Count - 1].PrintTag(states[i]);
				stateTags[stateTags.Count - 1].self.localPosition = new Vector3(0f, stateShift, 0f);
			}
			stateShift -= stateTagShift;
		}
	}

	void ShowFlagBoxes(Flag[] flags)
	{
		float boxShift = 0f;
		for( int i = 0; i < flags.Length; i++)
		{
			Debug.Log(flags[i].GetType().Name);
			switch(flags[i].GetType().Name)
			{	
				case "FiremanStation":

				break;



				default:
				Debug.LogWarning("This flag dosn't need any additional boxes");
				break;
			}
			boxShift -= stateTagShift;
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
			line.DrawCanvasLine(Camera.main.WorldToScreenPoint(currentSelection.transform.position), self.position, 2f, Color.black);
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

		foreach(StateTag st in stateTags)
		{
			st.Hide();
		}
	}

	public void UseBlock()
	{

	}
}
