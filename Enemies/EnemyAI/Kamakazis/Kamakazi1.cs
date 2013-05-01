using UnityEngine;
using System.Collections;

public class Kamakazi1 : EnemyAI
{
	public string movingAnimation = "kamakazi_moving";
	Vector3 collisionPoint;
	public float collisionRecoveryTime = 2.0f;
	private float collisionRecoveryTimeRemaining;
	
	override protected void init()
	{
		if (animator)
		{
			animator.Play(movingAnimation);
		}
		
		this.addState("default", defaultMovement);
		this.addState("recovery", collisionRecoveryMovement);
		this.setState("default");
	}
	
	protected override int getAvoidancePriority (Collider collider)
	{
		if (collider.gameObject.GetComponent<Kamakazi1>() != null)
			return -1;
			
		if (collider.gameObject.GetComponent<EnemyAI>() != null)
			return 1;
			
		// all other obstacles have the highest priority.
		return 2;
	}
	
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
	
	//override protected void defaultMovement()
	private void defaultMovement()
	{
		Collider obsticle = getColliderToAvoid(this.getCollidersInSight());
		if (obsticle != null)
		{
			Debug.DrawLine(this.transform.position, obsticle.transform.position, Color.red);
			enemyMotor.turnAway(obsticle.transform, 3f, true);
			enemyMotor.moveForward();
		}
		else
		{
			enemyMotor.turnToward( PlayerShip.MainPositionTransform );
			enemyMotor.moveForward();
		}
		enemyMotor.stopStrafeMovement();
		
		if (animator && !animator.IsPlaying(movingAnimation))
		{
			animator.Play(movingAnimation);
		}
	}
	
	private void collisionRecoveryMovement()
	{
		Vector3 collisionDirection = collisionPoint - this.transform.position;
		
		enemyMotor.turnAway(collisionPoint, 1.5f, true);
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
		
		if (animator && !animator.IsPlaying(movingAnimation))
		{
			animator.Play(movingAnimation);
		}
		
		collisionRecoveryTimeRemaining -= Time.deltaTime;
		if (collisionRecoveryTimeRemaining <= 0)
			this.setState("default");
	}
		
} // end of class