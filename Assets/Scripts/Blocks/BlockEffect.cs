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

	Dictionary <string, Particle> effects = new Dictionary <string, Particle>();

 
	public void Hide()
	{
		holder.SetActive(false);
	}

	public void Show()
	{
		holder.SetActive(true);

		foreach(KeyValuePair<string, Particle> e in effects)
		{
			if(e.Value.active)
			{
				e.Value.system.Play();
			}
		}
	}

	public void Activate(GameObject particle)
	{
		if(effects.ContainsKey(particle.name))
		{
			effects[particle.name].system.Play();
			effects[particle.name].active = true;
		}
		else
		{
			Particle p = new Particle();
			p.system = Instantiate(particle, transform.position, Quaternion.identity, holder.transform).GetComponent<ParticleSystem>();
			effects.Add(particle.name, p);

			effects[particle.name].system.Play();
			effects[particle.name].active = true;
		}
	}

	public void Desactivate(GameObject particle)
	{
		if(effects.ContainsKey(particle.name))
		{
			effects[particle.name].system.Stop();
			effects[particle.name].active = false;
		}
	}
}
