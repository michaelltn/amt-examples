using UnityEngine;
using System.Collections;

public class Shooter2 : EnemyAI
{
	public float minPlayerDistance = 25f;
	public float maxPlayerDistance = 30f;
	
	public float minStrafeTime = 0.5f;
	public float maxStrafeTime = 3f;
	
	public float minPauseTime = 0.25f;
	public float maxPauseTime = 0.25f;
	
	private float stateTime;
	
	
	public string idleAnim = "Shooter2_Idle";
	public string weaponFireAnim = "Shooter2_WeaponFire";
	public string weaponIdleAnim = "Shooter2_WeaponIdle";
	
	public EnemyWeapon weapon;
	public Transform weaponBarrel;
	
	
	protected override void init ()
	{
		base.init ();
		
		if (weaponBarrel == null && weapon != null)
			weaponBarrel = weapon.transform;
		
		addState("pause", pauseMovement);
		addState("strafeLeft", strafeLeftMovement);
		addState("strafeRight", strafeRightMovement);
		setState("pause");
		stateTime = Random.Range(minPauseTime, maxPauseTime);
	}
	
	
	
	
	void pauseMovement()
	{
		if (animator != null && !animator.isPlaying)
			animator.Play(idleAnim);
		
		if (PlayerShip.Main)
		{
			enemyMotor.turnToward(PlayerShip.Main.transform);
			enemyMotor.stopStrafeMovement();
			if (playerDistance > maxPlayerDistance)
				enemyMotor.moveForward();
			else if (playerDistance < minPlayerDistance)
				enemyMotor.moveBackward();
			else
				enemyMotor.stopForwardMovement();
		}
		else
		{
			enemyMotor.stopStrafeMovement();
		}
		
		stateTime -= Time.deltaTime;
		if (stateTime <= 0)
		{
			if (Random.Range(0,2) == 0)
				setState("strafeLeft");
			else
				setState("strafeRight");
				
			stateTime = Random.Range(minStrafeTime, maxStrafeTime);
		}
	}
	
	void strafeLeftMovement()
	{
		if (animator != null && !animator.isPlaying)
			animator.Play(idleAnim);
		
		if (PlayerShip.Main)
		{
			enemyMotor.turnToward(PlayerShip.Main.transform);
			enemyMotor.strafeLeft();
			if (playerDistance > maxPlayerDistance)
				enemyMotor.moveForward();
			else if (playerDistance < minPlayerDistance)
				enemyMotor.moveBackward();
			else
				enemyMotor.stopForwardMovement();
		}
		else
		{
			enemyMotor.stopMovement();
		}
		
		stateTime -= Time.deltaTime;
		if (stateTime <= 0)
		{
			setState("pause");
			stateTime = Random.Range(minPauseTime, maxPauseTime);
		}
	}
	
	void strafeRightMovement()
	{
		if (animator != null && !animator.isPlaying)
			animator.Play(idleAnim);
		
		if (PlayerShip.Main)
		{
			enemyMotor.turnToward(PlayerShip.Main.transform);
			enemyMotor.strafeRight();
			if (playerDistance > maxPlayerDistance)
				enemyMotor.moveForward();
			else if (playerDistance < minPlayerDistance)
				enemyMotor.moveBackward();
			else
				enemyMotor.stopForwardMovement();
		}
		else
		{
			enemyMotor.stopMovement();
		}
		
		stateTime -= Time.deltaTime;
		if (stateTime <= 0)
		{
			setState("pause");
			stateTime = Random.Range(minPauseTime, maxPauseTime);
		}
	}
	
	
	
	protected override void manageWeapons()
	{
		base.manageWeapons ();
		
		if (weapon == null) return;
		
		if (playerAngle <= attackAngle)
		{
			if (weapon.fire(weaponBarrel))
			{
				if (weapon.animation != null)
					weapon.animation.Play(weaponFireAnim);
			}
		}
		
		if (weapon.animation != null && !weapon.animation.isPlaying)
		{
			weapon.animation.Play(weaponIdleAnim);
		}
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
