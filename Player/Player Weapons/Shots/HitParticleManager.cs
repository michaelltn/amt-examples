using UnityEngine;
using System.Collections;

public class HitParticleManager : MonoBehaviour
{
	public static HitParticleManager Main;
	
	public ParticleSystem[] particleSystems;
	
	void Start ()
	{
		if (Main == null)
			Main = this;
			
		foreach (ParticleSystem ps in particleSystems)
		{
			ps.playOnAwake = false;
			ps.emissionRate = 0;
			ps.loop = false;
		}
	}
	
	void OnDestroy ()
	{
		if (Main == this)
			Main = null;
	}
	
	public void createHitParticles(int particleSystemIndex, int numberOfParticles, Vector3 position)
	{
		createHitParticles(particleSystemIndex, numberOfParticles, position, Quaternion.identity);
	}
	public void createHitParticles(int particleSystemIndex, int numberOfParticles,
		Vector3 position, Quaternion rotation)
	{
		if (particleSystemIndex < 0)
			return;
		if (particleSystemIndex >= particleSystems.Length)
			return;
		if (particleSystems[particleSystemIndex] == null)
			return;
		
		particleSystems[particleSystemIndex].transform.position = position;
		particleSystems[particleSystemIndex].transform.rotation = rotation;
		particleSystems[particleSystemIndex].Emit(numberOfParticles);
	}
	
}