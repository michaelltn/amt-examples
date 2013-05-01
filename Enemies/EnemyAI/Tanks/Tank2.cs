using UnityEngine;
using System.Collections;

public class Tank2 : EnemyAI
{
	public float minChargeToDistance = 15f;
	public float maxChargeToDistance = 30f;
	public float flyzoneBuffer = 20f;
	private Vector3 chargeTo, chargeDirection;
	
	public int spreadShotCount = 4;
	public float spreadShotAngle = 15f;
	public EnemyWeapon spreadShotWeapon;
	public Transform spreadShotBarrel;
	
	public int rotatingShotCount = 12;
	public float rotatingShotAngle = 30f;
	public EnemyWeapon rotatingShotWeapon;
	public Transform rotatingShotBarrel;
	
	public int burstShotCount = 10;
	public EnemyWeapon burstShotWeapon;
	public Transform burstShotBarrel;
	
	private int shotCounter;
	private float previousShotAngle;
	private bool spinningLeft;

	
	public string chargeAnim = "Tank2_Charge";
	public string chargeToFireAnim = "Tank2_ChargeToFire";
	public string fireToHoldAnim = "Tank2_FireToHold";
	public string holdFireAnim = "Tank2_HoldFire";
	public string reloadAnim = "Tank2_Reload";
	public string fireToForwardAnim = "Tank2_FireToForward";
	
	
	protected override void init ()
	{
		base.init ();
		
		if (spreadShotWeapon && spreadShotBarrel == null)
			spreadShotBarrel = spreadShotWeapon.transform;
		if (rotatingShotWeapon && rotatingShotBarrel == null)
			rotatingShotBarrel = rotatingShotWeapon.transform;
		if (burstShotWeapon && burstShotBarrel == null)
			burstShotBarrel = burstShotWeapon.transform;
		
		
		addState("charging", chargeMovement);
		addState("spreadShot", spreadShotMovement);
		addState("rotatingShot", rotatingShotMovement);
		addState("burstShot", burstShotMovement);
		setState("charging");
		charge();
	}
	
	
	
	void chargeMovement()
	{
		Debug.DrawLine(this.transform.position, chargeTo, Color.red);

		enemyMotor.turnToward(chargeTo);
		enemyMotor.moveForward();
		enemyMotor.stopStrafeMovement();
		
		if (animator && !animator.isPlaying)
			animator.Play(chargeAnim);				
		
		chargeDirection = chargeTo - this.transform.position;
		if (Vector3.Dot(chargeDirection, this.transform.right) <= 0)
		{
			if (FlyZone.ContainsTransform(this.transform, flyzoneBuffer))
				attack();
			else
				charge();
		}
	}
	
	
	void spreadShotMovement()
	{
		enemyMotor.stopMovement();
	}
	
	void rotatingShotMovement()
	{
		enemyMotor.stopMovement();
	}
	
	void burstShotMovement()
	{
		enemyMotor.stopMovement();
	}
	

	
	private void attack()
	{
		shotCounter = 0;
		previousShotAngle = enemyMotor.transform.rotation.eulerAngles.z;
		
		if (animator)
		{
			animator.CrossFade(chargeToFireAnim);
		}
		
		//currentState = AIState.Attacking;
		switch (Random.Range(0, 3))
		{
		case 0:
			this.setState("spreadShot");
			break;
		case 1:
			this.setState("rotatingShot");
			spinningLeft = (Random.Range(0,1) == 0);
			break;
		default:
			this.setState("burstShot");
			break;
		}
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
			animator.CrossFade(fireToForwardAnim);
		}
		
		this.setState("charging");
	}	

	
	protected override void manageWeapons()
	{
		base.manageWeapons ();
		
		if (isInState("spreadShot"))
		{
			if (spreadShotWeapon == null)
			{
				attack();
				return;
			}
			
			if (shotCounter == 0)
			{
				for (int i = 0; i < spreadShotCount; i++)
				{
					float offset = -((spreadShotCount-1) * spreadShotAngle / 2f) + (i * spreadShotAngle);
					spreadShotBarrel.transform.Rotate(0, 0, offset);
					spreadShotWeapon.fire(spreadShotBarrel);
					spreadShotBarrel.transform.Rotate(0, 0, -offset);
					if (animator && !animator.isPlaying)
						animator.Play(fireToHoldAnim);
				}
				shotCounter++;
			}
			else
			{
				if (!animator || !animator.isPlaying)
				{
					attack();
				}
				return;
			}
		}
		else if (isInState("rotatingShot"))
		{
			if (rotatingShotWeapon == null)
			{
				attack();
				return;
			}
			
			if (spinningLeft)
			{
				enemyMotor.transform.RotateAroundLocal(Vector3.forward,
					enemyMotor.maxRotationSpeed * Time.deltaTime);
			}
			else
			{
				enemyMotor.transform.RotateAroundLocal(Vector3.back,
					enemyMotor.maxRotationSpeed * Time.deltaTime);
			}
			
			if (shotCounter < burstShotCount)
			{
				if (Mathf.Abs(Mathm.getDeltaAngle(previousShotAngle, enemyMotor.transform.rotation.eulerAngles.z)) >= rotatingShotAngle &&
					rotatingShotWeapon.fire(rotatingShotBarrel))
				{
					previousShotAngle = enemyMotor.transform.rotation.eulerAngles.z;
					shotCounter++;
					if (animator && !animator.isPlaying)
						animator.Play(fireToHoldAnim);
				}
			}
			else
			{
				if (!animator || !animator.isPlaying)
				{
					attack();
				}
				return;
			}
		}
		else if (isInState("burstShot"))
		{
			if (burstShotWeapon == null)
			{
				attack();
				return;
			}
			
			if (shotCounter < burstShotCount)
			{
				if (burstShotWeapon.fire(burstShotBarrel))
				{
					shotCounter++;
					if (animator && !animator.isPlaying)
						animator.Play(fireToHoldAnim);
				}
			}
			else
			{
				if (!animator || !animator.isPlaying)
				{
					attack();
				}
				return;
			}
		}
		
		if (animator && !animator.isPlaying)
			animator.Play(holdFireAnim);
	}
	
	
	
	protected override void collisionResponse (Collision collision)
	{
		base.collisionResponse (collision);
	}
	
	public override void playerCollisionResponse ()
	{
		base.playerCollisionResponse ();
	}
	
	protected override int getAvoidancePriority (Collider collider)
	{
		return base.getAvoidancePriority (collider);
	}
	
	protected override void hitResponse (float damageTaken)
	{
		base.hitResponse (damageTaken);
	}
}