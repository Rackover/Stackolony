using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEffect : MonoBehaviour
{
	public GameObject holder;

	public class Particle
	{
		public ParticleSystem system;
		public bool active = false;
	}

	Dictionary <int, Particle> effects = new Dictionary <int, Particle>();

 
	public void Hide()
	{
		holder.SetActive(false);
	}

	public void Show()
	{
		holder.SetActive(true);

		foreach(KeyValuePair<int, Particle> e in effects)
		{
			if(e.Value.active)
			{
				e.Value.system.Play();
			}
		}
	}

	public void Activate(GameObject particle)
	{
		int index = particle.GetHashCode();

		if(effects.ContainsKey(index))
		{
			effects[index].system.Play();
			effects[index].active = true;
		}
		else
		{
			Particle p = new Particle();
			p.system = Instantiate(particle, transform.position, Quaternion.identity, holder.transform).GetComponent<ParticleSystem>();
			effects.Add(index, p);

			effects[index].system.Play();
			effects[index].active = true;
		}
	}

	public void Desactivate(GameObject particle)
	{
		int index = particle.GetHashCode();

		if(effects.ContainsKey(index))
		{
			effects[index].system.Stop();
			effects[index].active = false;
		}
	}
}
