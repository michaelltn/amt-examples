using UnityEngine;
using System.Collections;

public class FusionMissile: Projectile
{
	/*
	public Transform[] explosionPrefab;
	private Transform newExplosion;
	private GameObject newExplosionObject;
	private FusionExplosion newFusionExplosion;
	private ParticleEmitter expolosionParticleEmitter;
	private AudioSource explosionAudioSource;
	*/
	
	public FusionExplosion explosionPrefab;
	private FusionExplosion newExplosion;
	
	public float maxDistance = 75.0f;
	public bool heatSeeking = true;
	public float heatSeekingAngle = 10f;
	private float currentAngle, currentDistance, minimumDistance;
	public float rotationSpeed;
	private Vector2 targetDirection;
	private float currentRotation, newRotation;
	protected Transform targetTransform = null;
	
	override public void init ()
	{
		base.init ();
		targetDirection = new Vector2 (0, 0);
	}
	
	override protected void hitEnemy (Enemy e)
	{
		base.hitEnemy(e);
		createExplosion ();
	}
	
	override protected void hitObstacle()
	{
		Debug.Log ("Inside hitObstacle");
		createExplosion();
	}
	
	private void createExplosion ()
	{
		if (explosionPrefab != null)
		{
			newExplosion = Instantiate(explosionPrefab, this.transform.position, explosionPrefab.transform.rotation) as FusionExplosion;
			newExplosion.setDamageMultiplier(this.damageMultiplier);
		}
		/*
		newExplosionObject = new GameObject();
		newExplosionObject.transform.position = this.transform.position;
		newExplosionObject.AddComponent<FusionExplosion>();
		newFusionExplosion = newExplosionObject.GetComponent<FusionExplosion>();
		newFusionExplosion.dps = WeaponUpgrades.FusionMissileBlastDPS(projectileLevel);
		newFusionExplosion.duration = WeaponUpgrades.FusionMissileBlastDuratio (projectileLevel);
		newFusionExplosion.radius = WeaponUpgrades.FusionMissileBlastRadius(projectileLevel);

		if (getSourceWeaponLevel () < explosionPrefab.Length)
		{
			if (projectileLevel >= 0 && projectileLevel < explosionPrefab.Length)
			{
				Transform newExplosionPrefab = explosionPrefab[projectileLevel];
				if (newExplosionPrefab)
				{
					newExplosion = Instantiate(newExplosionPrefab, this.transform.position, Quaternion.identity) as Transform;
					expolosionParticleEmitter = newExplosion.gameObject.GetComponent<ParticleEmitter>();
					explosionAudioSource = newExplosion.gameObject.GetComponent<AudioSource>();
				}
				if (newExplosion)
				{
					newExplosion.parent = newExplosionObject.transform;
					newFusionExplosion.assignExplosionEffects(expolosionParticleEmitter, explosionAudioSource);
					if (expolosionParticleEmitter)
						expolosionParticleEmitter.Emit();
				}
			}
		}
		*/
	}
	
	override protected void manageDirection ()
	{
		if (heatSeeking && !this.initialMovement)
		{
			targetTransform = null;
			foreach (Enemy e in Enemy.EnemyList())
			{
				currentAngle = Vector3.Angle (this.transform.right, e.transform.position - this.transform.position);
				if (currentAngle <= heatSeekingAngle)
				{
					currentDistance = Vector3.Distance(this.transform.position, e.transform.position);
					if (!targetTransform || (currentDistance < minimumDistance))
					{
						targetTransform = e.transform;
						minimumDistance = currentDistance;
					}
				}
			}
				
			// turn toward target
			if (targetTransform != null)
			{
				Debug.DrawLine (this.transform.position, targetTransform.position, Color.yellow);
				targetDirection.x = targetTransform.position.x - this.transform.position.x;
				targetDirection.y = targetTransform.position.y - this.transform.position.y;
				currentRotation = this.transform.eulerAngles.z;
				newRotation = Mathm.Rotation2D (currentRotation, targetDirection, rotationSpeed * Time.deltaTime);
				this.transform.eulerAngles = new Vector3 (0, 0, newRotation);
			}
		}
	}
	
	override protected void postUpdate ()
	{
		if (maxDistance > 0 && distanceTravelled >= maxDistance)
		{
			createExplosion();
			Remove();
		}
	}
	
} // end of class