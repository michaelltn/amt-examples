using UnityEngine;
using System.Collections;

public class EnemyProjectile: EnemyShot {

	public float movementSpeed = 100f;
	public float initialMovementSpeed = 10f;
	public float initialMovementTime = 1f;
	private float initialMovementTimePassed;
	private Vector3 newPosition;
	
	public float rotationSpeed;
	private Vector2 targetDirection;
	private float currentRotation, newRotation;
	
	public float flyZoneBuffer = 20f;
	
	
	public int hitParticleIndex = -1;
	public int numberOfHitParticles = 10;

	// Use this for initialization
	override protected void init()
	{
		targetDirection = new Vector2(0, 0);
		initialMovementTimePassed = 0;
		newPosition = new Vector3(0, 0, 0);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!GameState.IsPaused)
		{
			if (this.transform.position.x < FlyZone.barrier.xMin - flyZoneBuffer ||
			    this.transform.position.x > FlyZone.barrier.xMax + flyZoneBuffer ||
			    this.transform.position.y < FlyZone.barrier.yMin - flyZoneBuffer ||
			    this.transform.position.y > FlyZone.barrier.yMax + flyZoneBuffer)
			{
				Destroy(this.gameObject);
			}
			else
			{
				// turn toward target
				if (targetTransform != null && initialMovementTimePassed <= initialMovementTime) {
					targetDirection.x = targetTransform.position.x - this.transform.position.x;
					targetDirection.y = targetTransform.position.y - this.transform.position.y;
					currentRotation = this.transform.eulerAngles.z;
					newRotation = Mathm.Rotation2D(currentRotation, targetDirection, rotationSpeed * Time.deltaTime);
					this.transform.eulerAngles = new Vector3(0, 0, newRotation);
					
				}
				
				initialMovementTimePassed += Time.deltaTime;
				if (initialMovementTimePassed <= initialMovementTime)
					newPosition = this.transform.position + (this.transform.right * initialMovementSpeed * Time.deltaTime);
				else
					newPosition = this.transform.position + (this.transform.right * movementSpeed * Time.deltaTime);
				newPosition.z = 0;
				this.transform.position = newPosition;
				if (this.transform.position.z != 0)
					Debug.Log("blargh!!!!!!  This should never be!  I told it to be at z=0.");
				
			}
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		PlayerShip playerShip = other.gameObject.GetComponent<PlayerShip>();
		if (playerShip)
		{
			playerShip.applyDamage(damage);
			
			if (HitParticleManager.Main != null)
				HitParticleManager.Main.createHitParticles(hitParticleIndex, numberOfHitParticles, this.transform.position);
			Destroy(this.gameObject);
			return;
		}
	}
	
} // end of class