using UnityEngine;
using System.Collections;


[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (Collider))]
public class EnemyMotor : MonoBehaviour
{
	public bool useFixedUpdate = true;
	
	public static bool FreezeAll = false;
	
	public float playerRelativeMovementSpeed = 1.0f;
	public float movementSpeed { get { return playerRelativeMovementSpeed * PlayerUpgrades.ShipSpeed; } }
	public float relativeAcceleration = 2f;
	public float acceleration { get { return movementSpeed * relativeAcceleration; } }
	public float relativeDeceleration = 5f;
	public float deceleration { get { return movementSpeed * relativeDeceleration; } }
	private float forwardSpeed, strafeSpeed;
	public Vector3 targetVelocity { get { return ((this.transform.right * forwardSpeed) + (this.transform.up * strafeSpeed)); } }
	public float currentSpeedClamped01
	{
		get
		{
			return Mathf.Clamp01(rigidbody.velocity.magnitude/movementSpeed);
		}
	}
	
	public float maxRotationSpeed = 180f;
	public float rotationAcceleration = 90f;
	private float rotationSpeed;
	private Vector2 targetDirection;
	private float currentRotation, newRotation;
	
	public float targetOffsetToDistanceRatio = 0.1f;
	private float targetOffsetUpdateTime;
	private float previousOffsetUpdateTime;
	public float minTargetOffsetUpdateTime = 0.25f;
	public float maxTargetOffsetUpdateTime = 0.75f;
	private float offsetUpdateTimeRemaining;
	
	float offsetAngle = 0;
	Vector2 targetOffset = Vector2.zero;
	float ratioRadius;
	private void setTargetOffset(out Vector2 targetOffset, float distanceToTarget)
	{
		ratioRadius = distanceToTarget * targetOffsetToDistanceRatio;
		targetOffset.x = ratioRadius * Mathf.Cos(offsetAngle * Mathf.Deg2Rad);
		targetOffset.y = ratioRadius * Mathf.Sin(offsetAngle * Mathf.Deg2Rad);
	}
	
	//public float playerBounceFactor = 1.0f;
	public delegate void CollisionHandler(Collision collision);
	public event CollisionHandler CollisionEvent;

	void Start ()
	{
		this.rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		this.rigidbody.isKinematic = false;
		this.rigidbody.freezeRotation = true;
		this.collider.isTrigger = false;
		
		targetDirection = new Vector2(0, 0);
		targetOffsetUpdateTime = Random.Range(minTargetOffsetUpdateTime, maxTargetOffsetUpdateTime);
		previousOffsetUpdateTime = Time.time;
		
		forwardSpeed = strafeSpeed = rotationSpeed = 0;
	}
	
	void Update ()
	{
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
		if (FreezeAll)
		{
			if (this.rigidbody.velocity.sqrMagnitude > 0)
			{
				this.rigidbody.AddForce(this.rigidbody.velocity * -1f, ForceMode.VelocityChange);
			}
			
		}
		else
		{
			float maxSpeedChange;
			Vector3 velocityDifference;
			velocityDifference = this.targetVelocity - this.rigidbody.velocity;
			if (this.rigidbody.velocity.sqrMagnitude < this.targetVelocity.sqrMagnitude)
			{ // speeding up
				maxSpeedChange = this.acceleration * Time.deltaTime;
			}
			else
			{ // slowing down
				maxSpeedChange = this.deceleration * Time.deltaTime;
			}
			
			if (maxSpeedChange < velocityDifference.magnitude)
			{
				velocityDifference.Normalize();
				velocityDifference *= maxSpeedChange;
			}
			
			this.rigidbody.AddForce(velocityDifference, ForceMode.VelocityChange);
			//this.rigidbody.angularVelocity = Vector3.zero;
		}
	}
	
	/*
	private Vector3 tempPos;
	void LateUpdate()
	{
		tempPos = this.transform.position;
		tempPos.z = 0;
		this.transform.position = tempPos;
		// reset z
	}
	*/
	
	public void OnCollisionEnter(Collision collision)
	{
		if (CollisionEvent != null)
			CollisionEvent(collision);
	}
	
	/*
	public void applyPlayerCollision()
	{
		Vector3 collisionDirection = this.transform.position - PlayerShip.MainPositionTransform.position;
		collisionDirection.Normalize();
		
		Vector3 newForward = Vector3.Project(collisionDirection, this.transform.right);
		newForward *= (playerBounceFactor * PlayerShip.Main.motor.movementSpeed);
		Vector3 newStrafe = Vector3.Project(collisionDirection, this.transform.up);
		newStrafe *= (playerBounceFactor * PlayerShip.Main.motor.movementSpeed);
		
		if (Vector3.Dot(collisionDirection, PlayerShip.Main.motor.velocity) > 0)
		{
			Vector3 playerPush = Vector3.Project(PlayerShip.Main.motor.velocity, collisionDirection);
			this.rigidbody.AddForce(playerPush - this.rigidbody.velocity, ForceMode.VelocityChange);
		}
	}
	*/
	
	private void updateTargetOffsetAngle(Vector3 targetPosition)
	{
		if (minTargetOffsetUpdateTime > 0 && maxTargetOffsetUpdateTime > 0)
		{
			if (Time.time - previousOffsetUpdateTime >= targetOffsetUpdateTime)
			{
				offsetAngle = Random.Range(0f, 360f);
				//float offsetDistance = Vector3.Distance(this.transform.position, targetPosition);
				//targetOffset.x = Random.Range( -1f, 1f );
				//targetOffset.y = Random.Range( -(1-targetOffset.x)/1f, (1-targetOffset.x)/1f );
				//targetOffset *= offsetDistance * targetOffsetToDistanceRatio;
				
				targetOffsetUpdateTime = Random.Range(minTargetOffsetUpdateTime, maxTargetOffsetUpdateTime);
				previousOffsetUpdateTime = Time.time;
			}
		}
	}
	
	public void turnToward(Transform targetTransform, float rotationSpeedMultiplier = 1f, bool ignoreTargetOffsetValue = false)
	{
		turnToward(targetTransform.position, rotationSpeedMultiplier, ignoreTargetOffsetValue);
	}
	public void turnToward(Vector3 targetPosition, float rotationSpeedMultiplier = 1f, bool ignoreTargetOffsetValue = false)
	{
		if (targetOffsetToDistanceRatio > 0)
			updateTargetOffsetAngle(targetPosition);
		
		if (ignoreTargetOffsetValue)
			targetOffset = Vector2.zero;
		else
			setTargetOffset(out targetOffset, Vector3.Distance(this.transform.position, targetPosition));
		targetDirection.x  = targetPosition.x - this.transform.position.x + targetOffset.x;
		targetDirection.y  = targetPosition.y - this.transform.position.y + targetOffset.y;
		currentRotation = this.transform.eulerAngles.z;
		newRotation = Mathm.Rotation2D(currentRotation, targetDirection, maxRotationSpeed * rotationSpeedMultiplier * Time.deltaTime);
//		newRotation = Mathm.Rotation2D(currentRotation, targetDirection,
//			ref rotationSpeed, maxRotationSpeed * rotationSpeedMultiplier, rotationAcceleration, Time.deltaTime);
		this.rigidbody.MoveRotation(Quaternion.Euler(new Vector3(0, 0, newRotation)));
	}
	
	
	public void turnAway(Transform targetTransform, float rotationSpeedMultiplier = 1f, bool ignoreTargetOffsetValue = false)
	{
		turnAway(targetTransform.position, rotationSpeedMultiplier, ignoreTargetOffsetValue);
	}
	public void turnAway(Vector3 targetPosition, float rotationSpeedMultiplier = 1f, bool ignoreTargetOffsetValue = false)
	{
		if (targetOffsetToDistanceRatio > 0)
			updateTargetOffsetAngle(targetPosition);
		
		if (ignoreTargetOffsetValue)
			targetOffset = Vector2.zero;
		else
			setTargetOffset(out targetOffset, Vector3.Distance(this.transform.position, targetPosition));
		targetDirection.x  = targetPosition.x - this.transform.position.x + targetOffset.x;
		targetDirection.y  = targetPosition.y - this.transform.position.y + targetOffset.y;
		targetDirection = -targetDirection;
		currentRotation = this.transform.eulerAngles.z;
		newRotation = Mathm.Rotation2D(currentRotation, targetDirection, maxRotationSpeed * rotationSpeedMultiplier * Time.deltaTime);
//		newRotation = Mathm.Rotation2D(currentRotation, targetDirection,
//			ref rotationSpeed, maxRotationSpeed, rotationAcceleration * rotationSpeedMultiplier, Time.deltaTime);
		this.rigidbody.MoveRotation(Quaternion.Euler(new Vector3(0, 0, newRotation)));
	}
	
	public void moveForward(float speedMultiplier = 1f)
	{
		forwardSpeed = movementSpeed * speedMultiplier;
	}
	
	public void moveBackward(float speedMultiplier = 1f)
	{
		moveForward(-speedMultiplier);
	}
	
	public void strafeLeft(float speedMultiplier = 1f)
	{
		strafeSpeed = movementSpeed * speedMultiplier;
	}
	public void strafeRight(float speedMultiplier = 1f)
	{
		strafeLeft(-speedMultiplier);
	}
	
	public void stopMovement()
	{
		stopForwardMovement();
		stopStrafeMovement();
	}
	public void stopForwardMovement()
	{
		forwardSpeed = 0;
	}
	public void stopStrafeMovement()
	{
		strafeSpeed = 0;
	}
	
	
} // end of class