using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Turret))]
public class TurretMotor : MonoBehaviour
{
	public bool useFixedUpdate = true;
	
	private Turret turret;
	
	public float followTargetInaccuracy = 0.05f;
	private Vector3 targetPosition;
	public float minTargetUpdateTime = 0.5f;
	public float maxTargetUpdateTime = 2f;
	private float targetOffsetUpdateTime, targetOffsetUpdateTimePassed;
	public float driftSpeed = 1f;
	private Vector3 driftDirection;

	public float minSpringRatio = 8f;
	public float maxSpringRatio = 25f;
	private float targetSpringRatio;
	private float currentSpringRatio, previousSpringRatio;
	public float minSpringRatioUpdateTime = 0.5f;
	public float maxSpringRatioUpdateTime = 1f;
	private float springRatioUpdateTime, springRatioUpdateTimePassed;
	
	
	public static bool FreezeAll = false;
	public static bool DriftAll = false;
	
	
	void Start ()
	{
		turret = this.gameObject.GetComponent<Turret>();
		
		springRatioUpdateTimePassed = 0;
		springRatioUpdateTime = 0;
		
		targetOffsetUpdateTimePassed = 0;
		targetOffsetUpdateTime = 0;
	
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
		if (!FreezeAll && !GameState.IsPaused)
		{
			if (PlayerShip.IsDead || DriftAll)
			{
				driftDirection = this.transform.position - PlayerShip.MainPositionTransform.position;
				driftDirection.Normalize();
				this.transform.Translate(driftDirection * driftSpeed * Time.deltaTime);
			}
			else
			{
				if (turret.followTarget != null)
				{
					updateTargetPosition();
					updateSpringRatio();
					Vector3 targetVector = targetPosition - this.transform.position; //followTarget.position - this.transform.position;
					Vector3 movementVector = targetVector * currentSpringRatio * Time.deltaTime;
					
					//if (movementVector.sqrMagnitude > targetVector.sqrMagnitude)
					//	movementVector = targetVector;
					//Debug.Log("movement vector: " + movementVector);
					//this.transform.Translate(movementVector, Space.World);
					this.transform.position += movementVector;
				}
			}
		}
		turret.update();
	}
	
	void updateTargetPosition()
	{
		if (turret.followTarget != null)
		{
			targetPosition = turret.followTarget.position;
			targetOffsetUpdateTimePassed += Time.deltaTime;
			if (targetOffsetUpdateTimePassed > targetOffsetUpdateTime)
			{
				float dist = Random.Range(0, followTargetInaccuracy);
				float angle = Random.Range(0, 2f*Mathf.PI);
				targetPosition.x += (dist * Mathf.Cos(angle));
				targetPosition.y += (dist * Mathf.Sin(angle));
				targetOffsetUpdateTimePassed = 0;
				targetOffsetUpdateTime = Random.Range(minTargetUpdateTime, maxTargetUpdateTime);
				//Debug.Log("update target position: " + targetOffsetUpdateTime);
			}
		}
	}
	
	void updateSpringRatio()
	{
		springRatioUpdateTimePassed += Time.deltaTime;
		if (springRatioUpdateTimePassed > springRatioUpdateTime)
		{
			previousSpringRatio = targetSpringRatio;
			targetSpringRatio = Random.Range(minSpringRatio, maxSpringRatio);
			springRatioUpdateTimePassed = 0;
			springRatioUpdateTime = Random.Range(minSpringRatioUpdateTime, maxSpringRatioUpdateTime);
			//Debug.Log("update spring ratio: " + springRatioUpdateTime);
		}
		currentSpringRatio = Mathf.Lerp(previousSpringRatio, targetSpringRatio, springRatioUpdateTimePassed/springRatioUpdateTime);
	}
	
} // end of class