using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockInfobox : MonoBehaviour 
{
	public GameObject box;
	public Text nameText;
	public Text descriptionText;
	public float stateTagShift = 20f;
	
	public RectTransform statesHolder;

	public GameObject stateTagPrefab;

	List<StateTag> stateTags = new List<StateTag>(); 


	public void LoadBlockValues(BlockLink block)
	{
		foreach(StateTag st in stateTags)
		{
			st.Hide();
		}

		box.SetActive(true);
		nameText.text = block.myBlock.title;
		descriptionText.text = block.myBlock.description;

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

			shift += stateTagShift;
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
		box.SetActive(false);

		foreach(StateTag st in stateTags)
		{
			st.Hide();
		}
	}
}
