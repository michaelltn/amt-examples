using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*	Resistance notes
 * 		Resistance values valied from -1 to 1 inclusive
 * 		[-1, 0) : damage from this type is increased by value * damage
 * 		 0 : no change
 * 		( 0, 1] : damage from this type is by value * damage
 * 
 * 		eg: a resistance of -0.5 means it recieves 150% damage from that type.
 * 			a resistance of 0.1 means it recieves 90% damage from that type.
 * */

public class Hull: MonoBehaviour
{
	
	//public bool immuneToCollisions = false;
	
	public float durability = 100f;
	public float durability01 { get { return maxDurability > 0 ? durability / maxDurability : 0; } }
	private float maxDurability;
	public float getMaxDurability() { return maxDurability; }
	public float resistance = 0f;
	public float hypometricResistance = 0f;
	public float fusionResistance = 0f;
	public float superluminalResistance = 0f;
	
	public delegate void HitFunction(float hitDamage);
	public event HitFunction hitEvent;
	public delegate void KillFunction();
	public event KillFunction killEvent;
	
	public Transform explosionPrefab;
	
	public ParticleSystem lightDamageSmokeTrail;
	public float lightDamageThreshold = 0.5f;
	public ParticleSystem heavyDamageSmokeTrail;
	public float heavyDamageThreshold = 0.25f;
	
	
	bool lightEnabled, heavyEnabled;
	private void updateSmokeTrail()
	{
		
		if (lightDamageSmokeTrail)
		{
			lightEnabled = (this.durability01 <= lightDamageThreshold);
			foreach (ParticleSystem ps in lightDamageSmokeTrail.gameObject.GetComponentsInChildren<ParticleSystem>())
				ps.enableEmission = lightEnabled;
		}
		if (heavyDamageSmokeTrail)
		{
			heavyEnabled = (this.durability01 <= heavyDamageThreshold);
			foreach (ParticleSystem ps in heavyDamageSmokeTrail.gameObject.GetComponentsInChildren<ParticleSystem>())
				ps.enableEmission = heavyEnabled;
		}
	}
	
	
	void Start ()
	{
		maxDurability = durability;
		resistance = Mathf.Clamp(resistance, 0, 1f);
		hypometricResistance = Mathf.Clamp(hypometricResistance, 0, 1f);
		superluminalResistance = Mathf.Clamp(superluminalResistance, 0, 1f);
		fusionResistance = Mathf.Clamp(fusionResistance, 0, 1f);
		
		updateSmokeTrail();
	}
	
	void LateUpdate ()
	{
		if (durability <= 0)
			kill();
	}
	
	public void applyDamage(float hypometricDamage, float superluminalDamage, float fusionDamage, float damage)
	{
		float newDamage = damage + hypometricDamage + superluminalDamage + fusionDamage;
		newDamage -= (newDamage * resistance);
		newDamage -= (hypometricDamage * hypometricResistance);
		newDamage -= (superluminalDamage * superluminalResistance);
		newDamage -= (fusionDamage * fusionResistance);
		
		durability -= newDamage;
		if (hitEvent != null)
			hitEvent(newDamage);
		
		updateSmokeTrail();
		//if (durability <= 0)
		//	kill();
	}
	
	public void kill()
	{
		if (killEvent != null)
			killEvent();
		
		if (explosionPrefab)
			Instantiate(explosionPrefab, this.transform.position, this.transform.rotation);
		
		Destroy(this.gameObject);
	}
	
	public bool isWeakAgainst(Weapon weapon)
	{
		if (weapon is HypometricWeapon)
		{
			return (hypometricResistance < 0);
		}
		else if (weapon is SuperluminalWeapon)
		{
			return (superluminalResistance < 0);
		}
		else if (weapon is FusionWeapon)
		{
			return (fusionResistance < 0);
		}
		return false;
	}

	public bool isStrongAgainst(Weapon weapon)
	{
		if (weapon is HypometricWeapon)
		{
			return (hypometricResistance > 0);
		}
		else if (weapon is SuperluminalWeapon)
		{
			return (superluminalResistance > 0);
		}
		else if (weapon is FusionWeapon)
		{
			return (fusionResistance > 0);
		}
		return false;
	}
	
} // end of class