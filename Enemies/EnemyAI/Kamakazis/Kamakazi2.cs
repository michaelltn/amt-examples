using UnityEngine;
using System.Collections;

public class Kamakazi2 : EnemyAI
{
	public string chargeAnimation = "kamakazi2_charge";
	public string chargeToAimAnimation = "kamakazi2_chargeToAim";
	public string aimAnimation = "kamakazi2_aim";
	public string aimToChargeAnimation = "kamakazi2_aimToCharge";
	
	public float maxChargeAngle = 2f;
	private Vector3 chargePoint;
	public float collisionChargeDistance = 20f;
	
	Vector3 collisionPoint;
	
	override protected void init()
	{
		if (animator)
		{
			animator.Play(aimAnimation);
		}
		
		this.addState("charge", chargeMovement);
		this.addState("aim", aimMovement);
		this.setState("aim");
	}
	
	protected override void collisionResponse (Collision collision)
	{
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
		
		if (animator && !animator.IsPlaying(chargeAnimation))
		{
			animator.CrossFade(aimToChargeAnimation);
		}
		chargePoint = this.transform.position + (collisionDirection.normalized * -collisionChargeDistance);
		setState("charge");
	}
	
	private void chargeMovement()
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
			enemyMotor.turnToward( chargePoint );
			enemyMotor.moveForward();
		}
		enemyMotor.stopStrafeMovement();
		
		if (Vector3.Dot(chargePoint - this.transform.position, this.transform.forward) > 0)
		{
			if (animator && !animator.IsPlaying(aimAnimation))
			{
				animator.Play(aimAnimation);
			}
		}
		else
		{
			if (animator && !animator.IsPlaying(aimAnimation))
			{
				animator.CrossFade(chargeToAimAnimation);
				setState("aim");
			}
		}
	}
	
	private void aimMovement()
	{
		enemyMotor.turnToward( PlayerShip.MainPositionTransform );
		enemyMotor.stopMovement();
		
		if (playerAngle > maxChargeAngle)
		{
			if (animator && !animator.IsPlaying(chargeAnimation))
			{
				animator.Play(chargeAnimation);
			}
		}
		else
		{
			if (animator && !animator.IsPlaying(chargeAnimation))
			{
				animator.CrossFade(aimToChargeAnimation);
			}
			chargePoint = PlayerShip.MainPositionTransform.position;
			setState("charge");
		}
	}
		
} // end of class