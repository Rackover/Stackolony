using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockInfobox : MonoBehaviour 
{
	[Header("Referencies")]
	public RectTransform self;
	public CanvasLineRenderer line;
	public GameObject box;
	public Text nameText;
	public Text descriptionText;
	public float stateTagShift = 20f;
	
	[Space(1)][Header("States")]
	public RectTransform statesHolder;
	public GameObject stateTagPrefab;

	[Space(1)][Header("SpecialBoxes")]
	public RectTransform firemanBox;
	public RectTransform repairerBox;

	BlockLink currentSelection;

	List<StateTag> stateTags = new List<StateTag>();

	public void LoadBlockValues(BlockLink block)
	{
		currentSelection = block;

		// Drawing line
		line.DrawCanvasLine(Camera.main.WorldToScreenPoint(block.transform.position), self.position, 2f, Color.black);

		// Changing general box values
		box.SetActive(true);
		nameText.text = block.myBlock.title;
		descriptionText.text = block.myBlock.description;


		// Printing states tag
		foreach(StateTag st in stateTags)
		{
			st.Hide();
		}
		float shift = 0f;
		for( int i = 0; i < block.states.Count; i++)
		{
			StateTag newTag = GetAvailableTag();
			if(newTag != null)
			{
				newTag.PrintTag(block.states[i]);
				newTag.self.localPosition = new Vector3(0f, shift, 0f);
			}
			else 
			{
				stateTags.Add(Instantiate(stateTagPrefab, statesHolder.position, Quaternion.identity, statesHolder).GetComponent<StateTag>());
				stateTags[stateTags.Count - 1].PrintTag(block.states[i]);
				stateTags[stateTags.Count - 1].self.localPosition = new Vector3(0f, shift, 0f);
			}
			shift -= stateTagShift;
		}

		// Displaying additional special boxes
	}

	void Update()
	{
		if(box.activeSelf && currentSelection != null)
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
		box.SetActive(false);

		foreach(StateTag st in stateTags)
		{
			st.Hide();
		}
	}

	public void UseBlock()
	{

	}
}
