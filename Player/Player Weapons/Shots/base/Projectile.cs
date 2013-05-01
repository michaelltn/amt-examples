using UnityEngine;
using System.Collections;

public abstract class Projectile: Shot
{
	private const int obstacleLayerInt = 19;
	
	public enum HitDetection { RayCastCheck, ColliderTrigger }
	public HitDetection hitDetection = HitDetection.RayCastCheck;
	
	private Vector3 pushDirection, pushVector;
	
	public float movementSpeed = 100f;
	public float initialMovementSpeed = 25f;
	public float initialMovementTime = 1f;
	private float initialMovementTimeRemaining;
	private Vector3 newPosition;

	public bool initialMovement {
		get {
			return initialMovementTimeRemaining > 0;
		}
	}
	
	public int survivableCollisions = 0;
	private int collisionsRemaining;
	
	private float _distanceTravelled, _deltaDistance;
	public float distanceTravelled { get { return _distanceTravelled; } }
	public float deltaDistance { get { return _deltaDistance; } }
	
	public float flyZoneBuffer = 40f;
	private RaycastHit[] hits;
	private RaycastHit[] validHits;
	private int hitCount;
	private RaycastHit temp;
	private float shortestDistance;
	private float checkDistance;
	private int firstTarget;
	
	public int hitParticleIndex = -1;
	public int numberOfHitParticles = 10;
	//public Transform hitParticlePrefab;
	//private Transform hitParticle;
	private Weapon sourceWeapon;

	public void assignSourceWeapon(Weapon w)
	{
		sourceWeapon = w;
	}

	public Weapon getSourceWeapon()
	{
		return sourceWeapon;
	}
	
	protected override void start()
	{
		newPosition = new Vector3(0, 0, 0);
		_distanceTravelled = 0;
		_deltaDistance = 0;
		collisionsRemaining = survivableCollisions;
		initialMovementTimeRemaining = initialMovementTime;
		if (hitDetection == HitDetection.ColliderTrigger && this.collider)
			this.collider.isTrigger = true;
	}
	
	virtual protected void manageDirection() {}
	virtual protected void postUpdate() {}
	virtual public void Remove()
	{
		Destroy(this.gameObject);
	}
	
	private void removeCheck()
	{
		if (collisionsRemaining-- <= 0)
			Remove();
	}
	
	virtual protected void hitEnemy(Enemy e)
	{
		Hull targetHull = e.gameObject.GetComponent ("Hull") as Hull;
		if (targetHull) {
			targetHull.applyDamage(getHypometricDamage(), getSuperluminalDamage(), getFusionDamage(), getUntypedDamage());
		}
		if (pushBackForce > 0 && e.motor)
		{
			pushDirection = (e.transform.position - this.transform.position).normalized;
			pushVector = Vector3.Project(pushDirection, this.transform.right);
			e.motor.rigidbody.AddForce(pushVector * pushBackForce, ForceMode.Impulse);
		}
	}
	
	virtual protected void hitObstacle() {}
	
	override protected void update()
	{
		if (!GameState.IsPaused)
		{
			if (this.transform.position.x < FlyZone.barrier.xMin - flyZoneBuffer ||
			    this.transform.position.x > FlyZone.barrier.xMax + flyZoneBuffer ||
			    this.transform.position.y < FlyZone.barrier.yMin - flyZoneBuffer ||
			    this.transform.position.y > FlyZone.barrier.yMax + flyZoneBuffer)
			{
				Remove ();
			}
			else
			{
				if (initialMovementTimeRemaining > 0)
					initialMovementTimeRemaining -= Time.deltaTime;
				
				manageDirection ();
				
				_deltaDistance = (initialMovement ? initialMovementSpeed : movementSpeed) * Time.deltaTime;
				if (hitDetection == HitDetection.RayCastCheck)
					if (raycastCollisionCheck (_deltaDistance))
						return;
				
				newPosition = this.transform.position + (this.transform.right * _deltaDistance);
				newPosition.z = 0;
				this.transform.position = newPosition;
				_distanceTravelled += _deltaDistance;
				postUpdate();
			}
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (hitDetection == HitDetection.ColliderTrigger)
		{			
			Enemy victim = other.gameObject.GetComponent<Enemy>();
			if (victim != null)
			{
				hitEnemy(victim);
				if (HitParticleManager.Main != null)
					HitParticleManager.Main.createHitParticles(hitParticleIndex, numberOfHitParticles, this.transform.position);
			}
			else
			{
				Hull hull = other.gameObject.GetComponent<Hull>();
				if (hull)
				{
					hull.applyDamage(getHypometricDamage(), getSuperluminalDamage(), getFusionDamage(), getUntypedDamage());
				}
			}
			if(other.gameObject.layer == 19)
			{
				hitObstacle();
				Debug.Log ("Calling hitObstacle");
				if (HitParticleManager.Main != null)
					HitParticleManager.Main.createHitParticles(hitParticleIndex, numberOfHitParticles, this.transform.position);
			}
			removeCheck();
		}
	}
	
	private bool raycastCollisionCheck(float distance)
	{
		hits = Physics.RaycastAll(this.transform.position, this.transform.right, distance, enemyLayer | obstacleLayer);
		
		validHits = new RaycastHit[ hits.Length ];
		hitCount = 0;
		for (int i = 0; i < hits.Length; i++)
		{
			if (hits[i].transform.gameObject.tag == "Enemy" || hits[i].transform.gameObject.layer == obstacleLayerInt)
			{
				validHits[hitCount++] = hits[i];
				Debug.Log(hits[i].transform.gameObject.layer.ToString());
			}
		}
		
		if (hitCount > 0) {
			// determine the first hit by the furthest hit point from the current position
			firstTarget = 0;
			shortestDistance = Vector3.Distance (this.transform.position, validHits [0].point);
			for (int h = 1; h < hitCount; h++)
			{
				checkDistance = Vector3.Distance (this.transform.position, validHits [h].point);
				if (checkDistance < shortestDistance)
				{
					shortestDistance = checkDistance;
					firstTarget = h;
				}
			}
			
			// move the projectile to the position where it hit.
			if(validHits[firstTarget].transform.gameObject.layer != 19)
				this.transform.position = validHits [firstTarget].point;
			
			//if(validHits[firstTarget].transform.gameObject.layer == obstacleLayer)
				//removeCheck();
			
			Enemy victim = validHits [firstTarget].transform.gameObject.GetComponent<Enemy> ();
			if (victim != null)
			{
				hitEnemy(victim);
				if (HitParticleManager.Main != null)
					HitParticleManager.Main.createHitParticles(hitParticleIndex, numberOfHitParticles, this.transform.position);
			}
			else
			{
				Hull hull = validHits [firstTarget].transform.gameObject.GetComponent<Hull> ();
				if (hull)
				{
					hull.applyDamage(getHypometricDamage(), getSuperluminalDamage(), getFusionDamage(), getUntypedDamage());
				}
				if(validHits[firstTarget].transform.gameObject.layer != 19)
				{
					hitObstacle();
					Debug.Log ("Calling hitObstacle");
					if (HitParticleManager.Main != null)
						HitParticleManager.Main.createHitParticles(hitParticleIndex, numberOfHitParticles, this.transform.position);
				}
			}
			
			removeCheck();
			return true;
		}
		return false;
	}
	
} // end of class