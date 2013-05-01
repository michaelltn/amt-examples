using UnityEngine;
using System.Collections;

public class FusionExplosion : MonoBehaviour
{
	//private ParticleSystem particleSystem;
	private AudioSource audioSource;
	
	public enum ExplosionDamageFallOff { Linear, Constant };
	public ExplosionDamageFallOff damageFallOff;
	public float hypometricDamage = 0;
	public float superluminalDamage = 0;
	public float fusionDamage = 100;
	public float untypedDamage = 0;
	
	public float damageRadius = 20;
	
	public float explosionForce = 50f;
	
	//public float duration;
	//public float radius;
	//public float hypometricDPS = 0;
	//public float superluminalDPS = 0;
	//public float fusionDPS = 100f;
	//public float untypedDPS = 0;
	private float damageMultiplier = 1.0f;
	public void setDamageMultiplier(float newDamageMultiplier)
	{
		damageMultiplier = newDamageMultiplier;
	}
	
	private float hDamage, sDamage, fDamage, uDamage;
	private float dist, damageFactor, forceFactor;
	//private float currentRadius;
	//private float currentDuration;
	
	private Hull h;
	
	//private AudioSource explosionAudio;
	//private ParticleEmitter explosionParticles;
	
	private bool dealingDamage;
	
	/*public void assignExplosionEffects(ParticleEmitter _particleEmitter, AudioSource _audioSource)
	{
		explosionAudio = _audioSource;
		explosionParticles = _particleEmitter;
	}*/
	
	void Start()
	{
		//particleSystem = this.gameObject.GetComponent<ParticleSystem>();
		audioSource = this.gameObject.GetComponent<AudioSource>();
		
		foreach (Enemy e in Enemy.EnemyList())
		{
			dist = Vector3.Distance(this.transform.position, e.transform.position);
			if (dist < damageRadius)
			{
				h = e.gameObject.GetComponent<Hull>();
				
				if (h)
				{
					switch (damageFallOff)
					{
					case ExplosionDamageFallOff.Constant:
						damageFactor = 1f;
						break;
					case ExplosionDamageFallOff.Linear:
						damageFactor = (damageRadius - dist)/damageRadius;
						break;
					}
					hDamage = hypometricDamage * damageMultiplier * damageFactor;
					sDamage = superluminalDamage * damageMultiplier * damageFactor;
					fDamage = fusionDamage * damageMultiplier * damageFactor;
					uDamage = untypedDamage * damageMultiplier * damageFactor;
					
					h.applyDamage(hDamage, sDamage, fDamage, uDamage);
				}
				if (explosionForce > 0 && e.motor)
				{
					forceFactor = (damageRadius - dist)/damageRadius;
					//e.motor.rigidbody.AddExplosionForce(explosionForce, this.transform.position, damageRadius);
					Vector3 explosionDirection = (e.transform.position - this.transform.position).normalized;
					e.motor.rigidbody.AddForce(explosionDirection * explosionForce * forceFactor, ForceMode.Impulse);
				}
			}
		}
		
		//currentRadius = 0;
		//currentDuration = 0;
		//dealingDamage = true;
	}
	
	void Update ()
	{
		if (explosionComplete())
			Destroy(this.gameObject);
		
		/*
		if (dealingDamage)
		{
			currentRadius += (radius / duration) * Time.deltaTime;
			hDamage = hypometricDPS * damageMultiplier * Time.deltaTime;
			sDamage = superluminalDPS * damageMultiplier * Time.deltaTime;
			fDamage = fusionDPS * damageMultiplier * Time.deltaTime;
			damage = untypedDPS * damageMultiplier * Time.deltaTime;
			
			foreach (Enemy e in Enemy.EnemyList())
			{
				if (Vector3.Distance(this.transform.position, e.transform.position) <= currentRadius)
				{
					h = e.gameObject.GetComponent<Hull>();
					if (h)
					{
						h.applyDamage(hDamage, sDamage, fDamage, damage);
					}
				}
			}
			
			currentDuration += Time.deltaTime;
			if (currentDuration > duration)
				dealingDamage = false;
		}
		else
		{
			if (explosionComplete())
				Destroy(this.gameObject);
		}
		*/
	}
	
	private bool explosionComplete()
	{
		//if (dealingDamage) return false;
		//if (explosionParticles && explosionParticles.particles.Length > 0) return false;
		if (particleSystem && particleSystem.IsAlive()) return false;
		if (audioSource && audioSource.isPlaying) return false;
		return true;
	}
	
	void OnDrawGizmos()
	{
		if (damageRadius > 0)
			Gizmos.DrawWireSphere(this.transform.position, damageRadius);
		
		//if (currentRadius > 0)
		//	Gizmos.DrawSphere(this.transform.position, currentRadius);
	}
	
} // end of class