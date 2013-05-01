using UnityEngine;
using System.Collections;

public class Tank1 : EnemyAI
{
	public float retreatSpeedMultiplier = 0.5f;
	public float retreatDistance = 15f;
	public float minChargeDistance = 20f;
	public float minChargeToDistance = 15f;
	public float maxChargeToDistance = 30f;
	public float flyzoneBuffer = 20f;
	
	public int minAttacksBeforeCharge = 5;
	public int maxAttacksBeforeCharge = 20;
	private int numAttacks;
	public float minDamageBeforeCharge = 100f;
	private float chargeDamageTaken;
	
	private Vector3 chargeTo, chargeDirection;
	private Vector3 retreatFrom;
	public float mothershipAttackDistance = 20f;
	
	public EnemyWeapon weapon;
	
	
	private void retreat()
	{
		retreatFrom = this.transform.position;

		if (animator)
		{
			//if (currentState == AIState.Attacking)
			if (this.getState().Equals("attacking"))
				animator.Play("shootToReverse");
			else
				animator.Play("backwards");
		}
		
		//this.currentState = AIState.Retreating;
		this.setState("retreating");
	}
	
	private void chargeToCenter()
	{
		chargeTo = Vector3.zero; // change to flyzone center...
		if (animator) {
			//if (currentState == AIState.Retreating)
			if (this.getState().Equals("retreating"))
				animator.CrossFade("backwardsToForwards");
			else
				animator.CrossFade("forwards");
		}
		
		//currentState = AIState.Charging;
		this.setState("charging");
		
		//chargeToCenterFlag = true;
	}
		
	
	private void charge()
	{
		// set chargeTo point
		float randomAngle = Random.Range(-90f, 90f);
		float randomDistance = Random.Range(minChargeToDistance, maxChargeToDistance);
		float relativeAngle = Vector3.Angle(playerDirection, Vector3.right);
		if (this.transform.position.y > PlayerShip.MainPositionTransform.position.y)
			relativeAngle = 360f - relativeAngle;
		
		float randomAngleRad = Mathf.Deg2Rad * (randomAngle + relativeAngle);
		
		chargeTo.x = PlayerShip.MainPositionTransform.position.x + (randomDistance * Mathf.Cos(randomAngleRad));
		chargeTo.y = PlayerShip.MainPositionTransform.position.y + (randomDistance * Mathf.Sin(randomAngleRad));
		chargeTo.z = 0;
		
		chargeTo = FlyZone.Clamp(chargeTo);
		
		if (animator)
		{
			//if (currentState == AIState.Retreating)
			if (this.getState().Equals("retreating"))
				animator.CrossFade("backwardsToForwards");
			else
				animator.CrossFade("forwards");
		}
		
		//currentState = AIState.Charging;
		this.setState("charging");
	}
	
	private void attack()
	{
		chargeDamageTaken = 0;
		numAttacks = 0;
		
		if (animator)
		{
			//if (currentState == AIState.Charging)
			if (this.getState().Equals("charging"))
			{
				animator.CrossFade("forwardsToShoot");
			}
			else
				animator.CrossFade("fireHold");
		}
		
		//currentState = AIState.Attacking;
		this.setState("attacking");
	}
	
	override protected void init()
	{
		this.addState("attacking", attackMovement);
		this.addState("charging", chargeMovement);
		this.addState("retreating", retreatMovement);
		
		// do stuff
		charge();
	}
	
	override protected void manageWeapons()
	{
		if (playerAngle <= attackAngle)
		{
			//if (currentState == AIState.Attacking)
			if (this.getState().Equals("attacking"))
			{
				if (animator && animator.IsPlaying("forwardsToShoot"))
				{
					// can't fire
				}
				else
				{
					if (weapon.fire(weapon.transform))
					{
						if (animator)
							animator.Play("fire");
						numAttacks++;
					}
				}
			}
		}
	}
	
	//override protected void defaultMovement() {}
	//override protected void attackMovement()
	protected void attackMovement()
	{
		enemyMotor.turnToward(PlayerShip.MainPositionTransform);
		enemyMotor.stopMovement();
		
		if (animator && !animator.isPlaying)
			animator.Play("fireHold");

		if (canStopAttacking())
		{
			if (playerDistance > minChargeDistance)
				charge();
			else
				retreat();
		}
	}
	//override protected void chargeMovement()
	protected void chargeMovement()
	{
		Debug.DrawLine(this.transform.position, chargeTo, Color.red);

		enemyMotor.turnToward(chargeTo);
		enemyMotor.moveForward();
		enemyMotor.stopStrafeMovement();
		
		if (animator && !animator.isPlaying)
			animator.Play("forwards");				
		
		chargeDirection = chargeTo - this.transform.position;
		if (Vector3.Dot(chargeDirection, this.transform.right) <= 0)
		{
			if (FlyZone.ContainsTransform(this.transform, flyzoneBuffer))
				attack();
			else
				charge();
		}
	}
	//override protected void dodgeMovement() {}
	//override protected void retreatMovement()
	protected void retreatMovement()
	{
		enemyMotor.turnToward(PlayerShip.MainPositionTransform);
		enemyMotor.moveBackward(retreatSpeedMultiplier);
		enemyMotor.stopStrafeMovement();
		
		if (animator && !animator.isPlaying)
			animator.Play("backwards");				
		
		if (Vector3.Distance(this.transform.position, retreatFrom) >= retreatDistance)
		{
			charge();
		}
	}

	private bool canStopAttacking()
	{
		return (numAttacks >= minAttacksBeforeCharge) && (numAttacks >= maxAttacksBeforeCharge || chargeDamageTaken >= minDamageBeforeCharge);
	}
	
	override protected void hitResponse(float damageTaken)
	{
		// do more stuff
		//if (currentState == AIState.Attacking)
		if (this.getState().Equals("attacking"))
		{
			chargeDamageTaken += damageTaken;
		}
	}
	
	override public void playerCollisionResponse()
	{
		//if (currentState == EnemyAI.AIState.Attacking)
		if (this.getState().Equals("attacking"))
		{
			retreat();
		}
	}
	
} // end of class