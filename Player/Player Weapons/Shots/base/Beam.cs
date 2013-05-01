using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Beam : Shot
{
	public float maxLength = 200.0f;
	public int penetration = 0;
	
	private RaycastHit[] beamHits;
	private RaycastHit[] validHits;
		private int hitCount;
		private RaycastHit temp;
		private float shortestDistance;
		private float checkDistance;
		private int closestTarget;
	private float beamLength;
	private int last;
	private Vector3 beamSize;
	
	private Weapon sourceWeapon;
	public void assignSourceWeapon(Weapon w) { sourceWeapon = w; }
	public Weapon getSourceWeapon() { return sourceWeapon; }
	
	public int hitParticleIndex = -1;
	private int numberOfHitParticles;
	public float numberOfHitParticlesPerSecond = 600;
	
	public float lingerTime = 0.1f;
	private float lingerTimeRemaining;
	private bool _lingering = false;
	public bool isLingering { get { return _lingering; } }
	
	override public void enable()
	{
		if (this.isLingering)
		{
			_lingering = false;
			lingerTimeRemaining = 0;
		}
		base.enable();
	}
	
	override public void disable()
	{
		if (lingerTime > 0)
		{
			if (!this.isLingering && this.isEnabled)
			{
				lingerTimeRemaining = lingerTime;
				_lingering = true;
			}
		}
		else
		{
			base.disable();
		}
	}
	
	override public void disableImmediate()
	{
		if (this.isLingering)
		{
			_lingering = false;
			lingerTimeRemaining = 0;
		}
		base.disableImmediate();
	}
	
	override protected void update()
	{
		base.update();
		if (this.isLingering)
		{
			lingerTimeRemaining -= Time.deltaTime;
			if (lingerTimeRemaining <= 0)
			{
				_lingering = false;
				lingerTimeRemaining = 0;
				base.disable();
			}
		}
	}
	
	public void applyDamage(Enemy enemy)
	{
		if (enemy != null)
		{
			Hull hull = enemy.gameObject.GetComponent<Hull>();
			if (hull != null)
			{
				hull.applyDamage(
					getHypometricDamage() * Time.deltaTime,
					getSuperluminalDamage() * Time.deltaTime,
					getFusionDamage() * Time.deltaTime,
					getUntypedDamage() * Time.deltaTime);
			}
			if (pushBackForce > 0 && enemy.motor)
			{
				enemy.motor.rigidbody.AddForce(this.transform.right * pushBackForce * Time.deltaTime, ForceMode.Impulse);
			}
		}
	}
	
	public Enemy[] updateBeam(Transform sourceTransform, out Vector3[] hitPoints)
	{
		return updateBeam(sourceTransform.position, sourceTransform.right, out hitPoints);
	}
	public Enemy[] updateBeam( Vector3 source, Vector3 direction, out Vector3[] hitPoints )
	{
		//validHit = false;
		beamHits = Physics.RaycastAll( source, direction, maxLength, enemyLayer | obstacleLayer );
		
		// remove from the list anything that doesn't match the target tag
		validHits = new RaycastHit[ beamHits.Length ];
		hitCount = 0;
		for (int i = 0; i < beamHits.Length; i++)
		{
			validHits[hitCount++] = beamHits[i];
		}
		
		// sort the beam hits array from closest to furthest
		for (int first = 0; first < hitCount-1; first++)
		{
			closestTarget = first;
			shortestDistance = Vector3.Distance( source, validHits[first].point );
			for (int check = first+1; check < hitCount; check++)
			{
				checkDistance = Vector3.Distance( source, validHits[check].point );
				if ( checkDistance < shortestDistance )
				{
					shortestDistance = checkDistance;
					closestTarget = check;
				}
			}
			if (closestTarget > first)
			{
				temp = validHits[first];
				validHits[first] = validHits[closestTarget];
				validHits[closestTarget] = temp;
			}
		}
		
		// determine at which target the beam must stop
		// penetration values less than 1 represent no penetration limitation
		if ( (penetration < 0) || penetration >= hitCount )
		{
			// since the number of targets in the list is less than or equal to the number the beam can penetrate,
			// the beam will be set to max length and all targets in the list will be affected.
			beamLength = maxLength;
			last = hitCount - 1;
		}
		else
		{
			// Since the number of targets the beam can penetrate is smaller than the number targets in the list,
			// the beam will stop at the nth target, where n is penetration + 1
			beamLength = Vector3.Distance( source, validHits[penetration].transform.position );
			last = penetration;
		}

		// adjust the beam transform based on source, direction and length.
		beamSize = this.transform.localScale;
		beamSize.x = beamLength;
		this.transform.localScale = beamSize;
		
		Vector3 tempRotation = Vector3.zero; //this.transform.rotation.eulerAngles;
		tempRotation.z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		this.transform.rotation = Quaternion.Euler(tempRotation);
		
		this.transform.position = source + (direction * beamLength / 2);
		
		// apply damage to all targets in the list up to the last target determined by penetration
		Enemy[] enemiesHit = new Enemy[last+1];
		hitPoints = new Vector3[last+1];
		for (int t = 0; t <= last; t++)
		{
			Transform hitObject = validHits[t].transform;
			enemiesHit[t] = hitObject.GetComponent<Enemy>();
			hitPoints[t] = validHits[t].point;
			if (HitParticleManager.Main != null)
			{
				numberOfHitParticles = Mathf.CeilToInt(numberOfHitParticlesPerSecond * Time.deltaTime);
				HitParticleManager.Main.createHitParticles(hitParticleIndex, numberOfHitParticles, validHits[t].point);
			}
		}
		
		return enemiesHit;
	}
	
	public void updateBeamWithoutRaycast( Vector3 source, Vector3 destination )
	{
		// adjust the beam transform based on source and destination.
		Vector3 direction = (destination - source).normalized;
		beamSize = this.transform.localScale;
		beamSize.x = Vector3.Distance(source, destination);
		this.transform.localScale = beamSize;
		
		Vector3 tempRotation = Vector3.zero; //this.transform.rotation.eulerAngles;
		tempRotation.z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		this.transform.rotation = Quaternion.Euler(tempRotation);
		
		this.transform.position = (source + destination) / 2;
		
		if (HitParticleManager.Main != null)
		{
			numberOfHitParticles = Mathf.CeilToInt(numberOfHitParticlesPerSecond * Time.deltaTime);
			HitParticleManager.Main.createHitParticles(hitParticleIndex, numberOfHitParticles, destination);
		}
	}

} // end of class