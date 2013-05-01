using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(CapsuleCollider))]
public class PlayerMotor : MonoBehaviour
{
	public bool useFixedUpdate = true;
	
	[System.NonSerialized]
	public float speedOfMaxSpeed = 0;
	public float movementSpeed = 30.0f;
	private Vector3 motorVelocity;
	public Vector3 velocity { get { return motorVelocity; } }
	
	public bool dashEnabled = true;
	public PlayerDashBar dashBar;
	public float baseDashDamage = 5f;
	public float scaledDashDamage = 0.05f;
	public float dashSpeed = 120.0f;
	public float dashTime = 0.5f;
	private float dashTimeRemaining;
	public float dashDecellerationTime = 0.1f;
	private float dashDecellerationTimeRemaining;
	public float dashRechargeTime = 30.0f;
	private float dashRechargeTimeRemaining;
	public float allowDashAfter = 15.0f;
	private bool _isDashing;
	public bool isDashing { get { return _isDashing; } }
	private bool isDashDecellerating = false;
	private Vector3 dashDirection;
	private CapsuleCollider capsuleCollider;
	public float scaleColliderDuringDash = 2.0f;
	public Vector3 moveColliderDuringDash;
	private float normalColliderRadius;
	private Vector3 normalColliderCenter;
	
	public delegate void moveDelegate();
	public event moveDelegate onMove;
	public delegate void dashDelegate();
	public event dashDelegate onDash;
	
	private bool frozen = false;
	public bool isFrozen { get { return frozen; } }
	public void freeze() { this.frozen = true; }
	public void thaw() { this.frozen = false; }

	public float getDashPercent()
	{
		return Mathf.Clamp01((dashRechargeTime - dashRechargeTimeRemaining) / dashRechargeTime);
	}
	public float currentDashSpeed
	{ get {
		if (_isDashing)
		{
			if (isDashDecellerating)
				return 1f - Mathf.Clamp01((dashDecellerationTime - dashDecellerationTimeRemaining) / dashDecellerationTime);
			else
				return 1f;
		}
		return 0;
	} }
	public bool dashIsAvailable
	{ get {
		return dashEnabled && !_isDashing && ((dashRechargeTime - dashRechargeTimeRemaining) >= allowDashAfter);
	}}
	
	public ParticleSystem dashParticleSystem;
	
	public float normalCollisionForce = 10f;
	public float dashCollisionForce = 20f;
	private Vector3 bumpDirection, relativeEnemyVelocity, bumpForce;
	
	private float inputSpeed;
	private Vector2 movementDirection;
	private float hBank, vBank;
	private float currentRotation, newRotation;
	
	private Vector3 newPosition;
	
	public Transform bankTarget;
	public float maxBankAngle = 15f;
	public float rotationSpeed = 270f;
	
	public Animation animator;
	
	private PlayerShip playerShip;
	
	void Start ()
	{
		//NEW COLLIDER/RIGIDBODY INITIALIZATION INFO
		this.rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		this.rigidbody.isKinematic = false;
		this.rigidbody.freezeRotation = true;
		this.collider.isTrigger = false;
		//OLD COLLIDER/RIGIDBODY INITIALIZATION INFO
		//this.rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		//this.rigidbody.isKinematic = true;
		//this.collider.isTrigger = true;
		capsuleCollider = (this.collider as CapsuleCollider);
		normalColliderRadius = capsuleCollider.radius;
		normalColliderCenter = capsuleCollider.center;
		
		playerShip = this.gameObject.GetComponent<PlayerShip>();
		
		if (!bankTarget)
			bankTarget = this.transform;
		PlayerShip.MainRotationTransform = bankTarget;
		movementDirection = new Vector2(0, 0);
		motorVelocity = new Vector3(0, 0, 0);
		
		_isDashing = false;
		isDashDecellerating = false;
		if (dashParticleSystem)
			dashParticleSystem.Stop();
		dashTimeRemaining = 0;
		dashDecellerationTimeRemaining = 0;
		dashRechargeTimeRemaining = 0;
		
		if (dashBar)
			dashBar.updateValue( this.getDashPercent(), this.dashIsAvailable );
		
		if (animator)
		{
			animator["player_moving"].wrapMode = WrapMode.Loop;
			animator["player_moving"].speed = 0.5f;
		}
	}
	
	void Update ()
	{
		if (this.isFrozen || GameState.IsPaused || PlayerShip.IsDead)
		{
			movementDirection = Vector2.zero;
		}
		else
		{
			if (dashRechargeTimeRemaining > 0)
			{
				dashRechargeTimeRemaining -= Time.deltaTime;
				if (dashRechargeTimeRemaining <= 0)
					dashCoolDownComplete();
			}
			
			if (dashDecellerationTimeRemaining > 0)
			{
				dashDecellerationTimeRemaining -= Time.deltaTime;
				if (dashDecellerationTimeRemaining <= 0)
					endDash();
			}
			
			if (dashTimeRemaining > 0)
			{
				dashTimeRemaining -= Time.deltaTime;
				if (dashTimeRemaining <= 0)
				{
					beginDashDecelleration();
				}
			}
			
			if (dashBar)
				dashBar.updateValue( this.getDashPercent(), this.dashIsAvailable );

			
			if (!_isDashing && dashIsAvailable && InputManager.DashButtonDown)
			{
				if (!EnemySpawner.Main || !EnemySpawner.Main.isCollectionTime)
					beginDash();
			}
			
			if (_isDashing)
			{
				motorVelocity.x = dashDirection.x * ((dashSpeed-movementSpeed) * currentDashSpeed + movementSpeed);
				motorVelocity.y = dashDirection.y * ((dashSpeed-movementSpeed) * currentDashSpeed + movementSpeed);
				//Debug.Log("is dashing motor update: " + motorVelocity);
			}
			else
			{
				movementDirection.x = InputManager.HorizontalMoveInput;
				movementDirection.y = InputManager.VerticalMoveInput;
				motorVelocity.x = movementDirection.x * movementSpeed;
				motorVelocity.y = movementDirection.y * movementSpeed;
			}
			
			if (movementDirection.magnitude > 0)
			{
				if (onMove != null)
					onMove();
			}
		}
		if (_isDashing)
			speedOfMaxSpeed = 1.0f;
		else
			speedOfMaxSpeed = movementDirection.magnitude;// / movementSpeed;
		
		if (!useFixedUpdate)
			UpdateFunction();
	}
	
	void FixedUpdate ()
	{
		if (useFixedUpdate)
			UpdateFunction();
	}
	
	private void UpdateFunction()
	{
		if (GameState.IsPaused || PlayerShip.IsDead || isFrozen)
		{
			
		}
		else
		{
			this.rigidbody.velocity = Vector3.zero;
			newPosition.x = this.transform.position.x + motorVelocity.x * Time.deltaTime;
			newPosition.y = this.transform.position.y + motorVelocity.y * Time.deltaTime;
			newPosition.z = 0;
			this.rigidbody.MovePosition(FlyZone.Clamp(newPosition));
			//this.rigidbody.AddForce(FlyZone.Clamp(newPosition), ForceMode.VelocityChange);
			
			hBank = -maxBankAngle * movementDirection.x * speedOfMaxSpeed;
			vBank = maxBankAngle * movementDirection.y * speedOfMaxSpeed;
			
			if (!this._isDashing && movementDirection.magnitude > 0)
			{
				currentRotation = bankTarget.eulerAngles.z;
				newRotation = Mathm.Rotation2D(currentRotation, movementDirection, rotationSpeed * Time.deltaTime);
				bankTarget.eulerAngles = new Vector3(vBank, hBank, newRotation);
			}
			
			if (CameraManager.Main)
				CameraManager.Main.PlayerUpdate();
		}
	}
	
	void OnCollisionEnter(Collision other)
	{
		Enemy enemy = other.gameObject.GetComponent<Enemy>();
		Hull enemyHull = other.gameObject.GetComponent<Hull>();
		if (enemy && enemyHull)
		{
			bumpDirection = (enemy.transform.position - this.transform.position).normalized;
			// add force to negate ship movement
			relativeEnemyVelocity = Vector3.Project(enemy.motor.rigidbody.velocity, bumpDirection);
			if (relativeEnemyVelocity.magnitude > 0)
				enemy.motor.rigidbody.AddForce(relativeEnemyVelocity * -1, ForceMode.VelocityChange);
			// add force from player ship repulsion.
			bumpForce = bumpDirection * (_isDashing ? dashCollisionForce : normalCollisionForce);
			enemy.motor.rigidbody.AddForce(bumpForce, ForceMode.Impulse);
			
			enemy.ai.playerCollisionResponse();
			if (_isDashing)
			{
				enemyHull.applyDamage(0, 0, 0, baseDashDamage + (enemyHull.getMaxDurability() * scaledDashDamage));
			}
			else
			{
				enemyHull.applyDamage(0, 0, 0, enemy.collisionDamageFromPlayer);
				if (enemy.cargo && enemyHull.durability <= 0)
				{
					Destroy(enemy.cargo);
				}
				playerShip.applyDamage(enemy.collisionDamageToPlayer);
			}
		}
	}
	
//	void OnTriggerEnter(Collider other)
//	{
//		Enemy enemy = other.gameObject.GetComponent<Enemy>();
//		Hull enemyHull = other.gameObject.GetComponent<Hull>();
//		if (enemy && enemyHull)
//		{
//			bumpDirection = (enemy.transform.position - this.transform.position).normalized;
//			// add force to negate ship movement
//			relativeEnemyVelocity = Vector3.Project(enemy.motor.rigidbody.velocity, bumpDirection);
//			if (relativeEnemyVelocity.magnitude > 0)
//				enemy.motor.rigidbody.AddForce(relativeEnemyVelocity * -1, ForceMode.VelocityChange);
//			// add force from player ship repulsion.
//			bumpForce = bumpDirection * (_isDashing ? dashCollisionForce : normalCollisionForce);
//			enemy.motor.rigidbody.AddForce(bumpForce, ForceMode.Impulse);
//			
//			enemy.ai.playerCollisionResponse();
//			if (_isDashing)
//			{
//				enemyHull.applyDamage(0, 0, 0, baseDashDamage + (enemyHull.getMaxDurability() * scaledDashDamage));
//			}
//			else
//			{
//				enemyHull.applyDamage(0, 0, 0, enemy.collisionDamageFromPlayer);
//				if (enemy.cargo && enemyHull.durability <= 0)
//				{
//					Destroy(enemy.cargo);
//				}
//				playerShip.applyDamage(enemy.collisionDamageToPlayer);
//			}
//		}
//	}
	
	public void stopDashing()
	{
		if (_isDashing)
			beginDashDecelleration();
	}
	
	private void beginDash()
	{
		//Debug.Log("begin dash");
		_isDashing = true;
		
		//dashDirection = Vector3.right;
		//dashDirection = Quaternion.AngleAxis(bankTarget.eulerAngles.z, Vector3.forward) * Vector3.right;
		dashDirection = InputManager.DashDirection;
		if (dashDirection.magnitude == 0)
		{
			dashDirection = Quaternion.AngleAxis(bankTarget.eulerAngles.z, Vector3.forward) * Vector3.right;
		}
		
		hBank = -maxBankAngle * dashDirection.x * speedOfMaxSpeed;
		vBank = maxBankAngle * dashDirection.y * speedOfMaxSpeed;
		
		currentRotation = bankTarget.eulerAngles.z;
		newRotation = Mathm.Rotation2D(currentRotation, dashDirection, 180f);
		bankTarget.eulerAngles = new Vector3(vBank, hBank, newRotation);
		
		dashTimeRemaining = dashTime * getDashPercent();
		if (dashParticleSystem)
			dashParticleSystem.Play();
		capsuleCollider.radius = normalColliderRadius * scaleColliderDuringDash;
		capsuleCollider.center = normalColliderCenter + moveColliderDuringDash;
		
		if (onDash != null)
			onDash();
	}
	
	public void forceDash(Vector3 dashDirection)
	{
		_isDashing = true;
		
		if (dashDirection.magnitude == 0)
		{
			dashDirection = Quaternion.AngleAxis(bankTarget.eulerAngles.z, Vector3.forward) * Vector3.right;
		}
		
		hBank = -maxBankAngle * dashDirection.x * speedOfMaxSpeed;
		vBank = maxBankAngle * dashDirection.y * speedOfMaxSpeed;
		
		currentRotation = bankTarget.eulerAngles.z;
		newRotation = Mathm.Rotation2D(currentRotation, dashDirection, 180f);
		bankTarget.eulerAngles = new Vector3(vBank, hBank, newRotation);
		
		dashTimeRemaining = dashTime * getDashPercent();
		if (dashParticleSystem)
			dashParticleSystem.Play();
		capsuleCollider.radius = normalColliderRadius * scaleColliderDuringDash;
		capsuleCollider.center = normalColliderCenter + moveColliderDuringDash;
	}
	
	private void beginDashDecelleration()
	{
		if (dashDecellerationTime > 0)
		{
			isDashDecellerating = true;
			dashDecellerationTimeRemaining = dashDecellerationTime;
		}
		else
		{
			endDash();
		}
	}
	
	private void endDash()
	{
		//Debug.Log("end dash");
		_isDashing = false;
		isDashDecellerating = false;
		dashRechargeTimeRemaining = dashRechargeTime;
		if (dashParticleSystem)
			dashParticleSystem.Stop();
		capsuleCollider.radius = normalColliderRadius;
		capsuleCollider.center = normalColliderCenter;
	}
	
	private void dashCoolDownComplete()
	{
		// do fancy ui stuff ?
	}
	
} // end of class