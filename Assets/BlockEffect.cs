using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEffect : MonoBehaviour
{
	public GameObject holder;

	public Dictionary<string, ParticleSystem> effects = new Dictionary<string, ParticleSystem>();

	public void Hide()
	{
		holder.SetActive(false);
	}

	public void Show()
	{
		holder.SetActive(true);
	}

	public void Activate(GameObject particle)
	{
		if(effects.ContainsKey(particle.name))
		{
			effects[particle.name].Play();
		}
		else
		{
			effects.Add(
				particle.name,
				Instantiate(particle, transform.position, Quaternion.identity, holder.transform).GetComponent<ParticleSystem>()
			);
		}
	}

	public void Desactivate(GameObject particle)
	{
		if(effects.ContainsKey(particle.name))
		{
			effects[particle.name].Stop();
		}
	}
}
