using UnityEngine;
using System.Collections;

public class Shooter1 : EnemyAI
{
	public string defaultAnimation = "gunner_moving";
	
	public float chargeTo = 10f;
	public float retreatTo = 40f;
	private Vector3 dodgeTo, dodgeDirection;
	public float dodgeDistance = 10f;
	
	Vector3 collisionPoint;
	public float collisionRecoveryTime = 2.0f;
	private float collisionRecoveryTimeRemaining;
	
	public EnemyWeapon weapon;
	
	
	override protected void init()
	{
		this.addState("charging", chargeMovement);
		this.addState("dodging", dodgeMovement);
		this.addState("retreating", retreatMovement);
		this.addState("recovery", collisionRecoveryMovement);
		this.setState("charging");
		
		//currentState = AIState.Charging;
		
		if (animator)
		{
			animator.Play(defaultAnimation);
		}
	}
	
	protected override int getAvoidancePriority (Collider collider)
	{
		if (collider.gameObject.GetComponent<Shooter1>() != null)
			return -1;
		
		if (collider.gameObject.GetComponent<Kamakazi1>() != null)
			return 0;
		
		if (collider.gameObject.GetComponent<EnemyAI>() != null)
			return 1;
		
		// all other obstacles have the highest priority.
		return 2;
	}
	
	override protected void manageWeapons()
	{
		if (playerAngle <= attackAngle)
		{
			//if (currentState == AIState.Charging)
			if (this.isInState("charging"))
			{
				weapon.fire(weapon.transform);
			}
		}
	}
	
	//override protected void defaultMovement() {}
	//override protected void attackMovement() {}
	//override protected void chargeMovement()
	virtual protected void chargeMovement()
	{
		Collider obsticle = getColliderToAvoid(this.getCollidersInSight(), 45f);
		if (obsticle != null)
		{
			Debug.DrawLine(this.transform.position, obsticle.transform.position, Color.red);
			enemyMotor.turnAway(obsticle.transform);
			enemyMotor.moveForward();
		}
		else
		{
			enemyMotor.turnToward(PlayerShip.MainPositionTransform);
			enemyMotor.stopStrafeMovement();
			if (Vector3.Dot(playerDirection, this.transform.right) < 0)
			{
				enemyMotor.stopForwardMovement();
			}
			else
			{
				enemyMotor.moveForward();
			}
		}
		
		if (playerDistance <= chargeTo)
		{
			//currentState = AIState.Dodging;
			dodgeTo = PlayerShip.MainPositionTransform.transform.position;
			dodgeDirection = dodgeTo - this.transform.position;
			dodgeDirection.Normalize();
			if (Random.Range(0, 2) > 0)
				dodgeDirection = Quaternion.Euler(0, 0, 90) * dodgeDirection;
			else
				dodgeDirection = Quaternion.Euler(0, 0, -90) * dodgeDirection;
			dodgeTo += dodgeDirection * dodgeDistance;
			//dodgeTo += this.transform.forward * dodgeDistance;
			
			this.setState("dodging");
		}
	}
	//override protected void dodgeMovement()
	virtual protected void dodgeMovement()
	{
		Debug.DrawLine(this.transform.position, dodgeTo, Color.red);
		
		Collider obsticle = getColliderToAvoid(this.getCollidersInSight(), 45f);
		if (obsticle != null)
		{
			Debug.DrawLine(this.transform.position, obsticle.transform.position, Color.red);
			enemyMotor.turnAway(obsticle.transform);
			enemyMotor.moveForward();
		}
		else
		{
			enemyMotor.turnToward(dodgeTo);
			enemyMotor.moveForward();
			enemyMotor.stopStrafeMovement();
		}
		dodgeDirection = dodgeTo - this.transform.position;
		if (Vector3.Dot(dodgeDirection, this.transform.right) <= 0)
		{
			//currentState = AIState.Retreating;
			
			this.setState("retreating");
		}
	}
	//override protected void retreatMovement()
	virtual protected void retreatMovement()
	{
		Collider obsticle = getColliderToAvoid(this.getCollidersInSight(), 45f);
		if (obsticle != null)
		{
			Debug.DrawLine(this.transform.position, obsticle.transform.position, Color.red);
			enemyMotor.turnAway(obsticle.transform);
			enemyMotor.moveForward();
		}
		else
		{
			enemyMotor.turnAway(PlayerShip.MainPositionTransform);
			enemyMotor.stopStrafeMovement();
			if (Vector3.Dot(playerDirection, this.transform.right) > 0)
			{
				enemyMotor.stopForwardMovement();
			}
			else
			{
				enemyMotor.moveForward();
			}
		}
		
		if (playerDistance >= retreatTo)
		{
			//currentState = AIState.Charging;
			this.setState("charging");
		}
	}
	/*
	override protected void mothershipAssultMovement() {
		enemy.turnToward( MotherShip.main.targetPoint(this.transform.position) );
		if (Vector3.Distance(MotherShip.main.targetPoint(this.transform.position), this.transform.position) > mothershipAttackDistance )
		{
			enemy.moveForward();
		}
		else
		{
			float distanceFromSpawnPoint = Vector3.Distance(PlayerSpawnFX.main.transform.position, this.transform.position);
			if (distanceFromSpawnPoint < PlayerShip.main.respawnDestroyDistance) {
				if (this.transform.position.y > PlayerSpawnFX.main.transform.position.y)
					enemy.strafeRight();
				else
					enemy.strafeLeft();
			}
			else {
				enemy.stopMovement();
			}
		}
	}*/
	
	protected override void collisionResponse (Collision collision)
	{
		if (this.isInState("recovery")) return;
		
		Vector3 collisionDirection = Vector3.zero;
		bool foundCollisionPoint = false;
		foreach (ContactPoint cp in collision.contacts)
		{
			Vector3 cd = cp.point - this.transform.position;
			if (!foundCollisionPoint ||
				(cd.sqrMagnitude < collisionDirection.sqrMagnitude))
			{
				collisionPoint = cp.point;
				collisionDirection = cd;
				foundCollisionPoint = true;
			}
		}
		
		if (!foundCollisionPoint) return;
		if (Vector3.Dot(collisionDirection, this.transform.right) < 0) return;
		
		collisionRecoveryTimeRemaining = collisionRecoveryTime;
		this.setState("recovery");
	}
	
	private void collisionRecoveryMovement()
	{
		Vector3 collisionDirection = collisionPoint - this.transform.position;
		
		enemyMotor.turnAway(collisionPoint, 1.5f);
		if (collisionDirection.magnitude == 0 ||
			Vector3.Dot(collisionDirection, this.transform.right) > 0)
		{
			enemyMotor.moveBackward(0.1f);
		}
		else
		{
			
			enemyMotor.moveForward(0.5f);
		}
		enemyMotor.stopStrafeMovement();
		
		collisionRecoveryTimeRemaining -= Time.deltaTime;
		if (collisionRecoveryTimeRemaining <= 0)
			this.setState("charging");
	}
	
	override protected void hitResponse(float damageTaken) {}
	
} // end of class