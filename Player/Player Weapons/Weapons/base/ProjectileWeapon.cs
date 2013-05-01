using UnityEngine;
using System.Collections;

public abstract class ProjectileWeapon : Weapon
{
	public Projectile ammo;
	protected Projectile newProjectile;
	
	public float timeBetweenShots;
	public float timeBetweenShotVariance = 0.05f;
	public float minTimeBetweenShots { get { return timeBetweenShots * (1f - timeBetweenShotVariance); } }
	public float maxTimeBetweenShots { get { return timeBetweenShots * (1f + timeBetweenShotVariance); } }
	private float randomTimeBetweenShots { get { return Random.Range(minTimeBetweenShots, maxTimeBetweenShots); } }
	protected float shotTimeRemaining;
	
	public enum ProjectileSpread { Parallel, Spread };
	public ProjectileSpread projectileSpread;
	
	public int numberOfShots = 1;
	public float shotSeparation = 5.0f;
	
	private float offset;
	
	public float pitchVariance = 0.1f;
	
	override public void init()
	{
		shotTimeRemaining = 0;
		timeBetweenShotVariance = Mathf.Clamp01(timeBetweenShotVariance);
		if (this.audio)
		{
			this.audio.loop = false;
			if (this.audio.isPlaying)
				this.audio.Stop();
		}
	}
	
	override public void update()
	{
		if (!GameState.IsPaused)
			if (shotTimeRemaining > 0)
				shotTimeRemaining -= Time.deltaTime;
	}
	
	override public void fire(Transform barrel)
	{
		base.fire(barrel);
		/*
		if (!firing)
		{
			base.fire(barrel);
			//if (randomFiringDelay)
			//shotTimeRemaining = Random.Range(0f, timeBetweenShots * randomFiringDelay);
			shotTimeRemaining = 0;
		}
		*/
		
		if (firing && ammo != null)
		{
			if (shotTimeRemaining <= 0)
			{
				for (int i = 0; i < numberOfShots; i++)
				{
					newProjectile = Instantiate(ammo, barrel.position, barrel.rotation ) as Projectile;
					newProjectile.assignSourceWeapon(this);
					newProjectile.setDamageMultiplier(this.damageMultiplier);
					
					if (numberOfShots > 1)
					{
						switch (projectileSpread)
						{
						case ProjectileSpread.Parallel:
							offset = -((numberOfShots-1) * shotSeparation / 2f) + (i * shotSeparation);
							newProjectile.transform.position += (offset * barrel.up);
							break;
						case ProjectileSpread.Spread:
							offset = -((numberOfShots-1) * shotSeparation / 2f) + (i * shotSeparation);
							newProjectile.transform.Rotate(0, 0, offset);
							break;
						}
					}
					
					if (this.audio)
					{
						if (this.audio.isPlaying)
							this.audio.Stop();
						this.audio.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
						this.audio.Play();
					}
					
					newProjectile.init();
					shotTimeRemaining = randomTimeBetweenShots;
				}
			}
		}
	}
}