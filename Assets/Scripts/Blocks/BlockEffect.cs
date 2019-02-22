using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEffect : MonoBehaviour
{
	public GameObject holder;

	public class Effect
	{
		public GameObject gameObject;
		public ParticleSystem system;
		public bool active = false;
	}

	Dictionary <int, Effect> effects = new Dictionary <int, Effect>();

 
	public void Hide()
	{
		holder.SetActive(false);
	}

	public void Show()
	{
		holder.SetActive(true);

		foreach(KeyValuePair<int, Effect> e in effects)
		{
			if(e.Value.active && e.Value.system != null)
			{
				e.Value.system.Play();
			}
		}
	}

	public void Activate(GameObject obj)
	{
		int index = obj.GetHashCode();

		if(effects.ContainsKey(index))
		{
			if(effects[index].system != null) effects[index].system.Play();
			else effects[index].gameObject.SetActive(true);

			effects[index].active = true;
		}
		else
		{
			Effect e = new Effect();
			e.gameObject = Instantiate(obj, holder.transform.position, Quaternion.identity, holder.transform);
			e.system = e.gameObject.GetComponent<ParticleSystem>();
			if(e.system != null) e.system.Play();
			e.active = true;

			effects.Add(index, e);

		}
	}

	public void Desactivate(GameObject obj)
	{
		int index = obj.GetHashCode();
		if(effects.ContainsKey(index))
		{
			if(effects[index].system != null) effects[index].system.Stop();
			else effects[index].gameObject.SetActive(false);
			effects[index].active = false;
		}
	}
}
