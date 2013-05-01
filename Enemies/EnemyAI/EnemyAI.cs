using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Enemy))]
[RequireComponent (typeof(EnemyMotor))]
public abstract class EnemyAI: MonoBehaviour
{
	protected Enemy enemy;
	protected EnemyMotor enemyMotor;
	
	public static bool FreezeAll = false;
	
	protected delegate void actionDelegate();
	Dictionary<string, actionDelegate> actions;
	protected bool addState(string stateName, actionDelegate function)
	{
		if (actions.ContainsKey(stateName)) return false;
		actions.Add(stateName, function);
		return true;
	}
	
	string state;
	public string getState() { return state; }
	public bool isInState(string state) { return this.state.Equals(state); }
	public bool setState(string newState)
	{
		if (actions.ContainsKey(newState))
		{
			onStateChange(state, newState);
			state = newState;
			return true;
		}
		return false;
	}
	virtual protected void onStateChange(string oldState, string newState) {}


	public EnemyAI()
	{
		collidersInSight = new List<Collider>();
		actions = new Dictionary<string, actionDelegate>();
		state = "";
	}
	
	
	protected float playerDistance;
	protected Vector3 playerDirection;
	protected float playerAngle;

	public Animation animator;
	protected int pauseFrame;

	protected Hull hull;
	//public Breakable destructionPrefab;
	//private Breakable breakMesh;

	// common configurable attributes
	public float attackAngle = 90f;
	
	public LayerMask sightSphereLayerMask;
	private List<Collider> collidersInSight;
	public Collider[] getCollidersInSight() { return collidersInSight.ToArray(); }
	public float minimumSightSphereRadius = 1.0f;
	public float maximumSightSphereRadius = 5.0f;
	private float sightSphereRadius
	{
		get
		{
			if (enemyMotor)
			{
				return minimumSightSphereRadius + 
					enemyMotor.currentSpeedClamped01 * (maximumSightSphereRadius - minimumSightSphereRadius);
			}
			return 0;
		}
	}
	private Vector3 sightSpherePosition
	{
		get
		{
			if (enemyMotor)
			{
				return this.transform.position +
					this.transform.right * enemyMotor.currentSpeedClamped01 * 0.5f * (maximumSightSphereRadius - minimumSightSphereRadius);
						
			}
			return Vector3.zero;
		}
	}
	
	/* AVOIDANCE ANGLE PENETRATION
	 * 
	 * Represents how close the closest point of a given collider is
	 * to being directly in front of this where 0 is not at all and
	 * 1 is directly in front.
	 * 
	 * */
	
	Vector3 relativePosition, closestPoint;
	float otherAngle;
	protected float getAvoidanceAnglePenetration(Collider other, float avoidanceAngle = 30f)
	{
		closestPoint = other.ClosestPointOnBounds(this.transform.position);
		
		relativePosition = closestPoint - this.transform.position;
		
		if (relativePosition.sqrMagnitude == 0)
			return 0;
		
		if (Vector3.Dot(this.transform.right, relativePosition) < 0)
			return 0;
		
		otherAngle = Vector3.Angle(this.transform.right, relativePosition);
		
		return Mathf.Clamp01(1f - (otherAngle/avoidanceAngle));
	}
	
	Collider colliderToAvoid;
	float avoidanceFactor, highestAvoidanceFactor;
	protected Collider getColliderToAvoid(Collider[] otherColliders, float avoidanceAngle = 75f)
	{
		colliderToAvoid = null;
		int avoidancePriority;
		foreach (Collider c in otherColliders)
		{
			if (c != null)
			{
				avoidancePriority = this.getAvoidancePriority(c);
				if (avoidancePriority < 0)
					continue;
				
				avoidanceFactor = getAvoidanceAnglePenetration(c, avoidanceAngle);
				if (avoidanceFactor > 0)
				{
					if (colliderToAvoid != null)
					{
						// check if the current target is not a priority but the new target is
						if (avoidancePriority > this.getAvoidancePriority(colliderToAvoid))
						{
							Debug.Log(c.name + " is a priority.");
							colliderToAvoid = c;
							highestAvoidanceFactor = avoidanceFactor;
							continue;
						}
					}
					
					if (colliderToAvoid == null ||
						(highestAvoidanceFactor < avoidanceFactor))
					{
						colliderToAvoid = c;
						highestAvoidanceFactor = avoidanceFactor;
					}
				}
			}
		}
		return colliderToAvoid;
	}
	
	
	void Start ()
	{
		enemy = this.gameObject.GetComponent<Enemy>();
		enemyMotor = this.gameObject.GetComponent<EnemyMotor>();
		if (enemyMotor) enemyMotor.CollisionEvent += collisionResponse;
		
		hull = this.GetComponent<Hull>();
		if (hull)
		{
			hull.hitEvent += hitResponse;
			hull.killEvent += killResponse;
		}
		if (PlayerShip.MainPositionTransform)
		{
			playerDistance = Vector3.Distance(PlayerShip.MainPositionTransform.position, this.transform.position);
			playerDirection = PlayerShip.MainPositionTransform.position - this.transform.position;
			playerAngle = Vector3.Angle(this.transform.right, playerDirection);
		}
		
		init();
	}
	
	
	void Update ()
	{
		if (!FreezeAll && !GameState.IsPaused)
		{
			if (PlayerShip.MainPositionTransform)
			{
				playerDistance = Vector3.Distance(PlayerShip.MainPositionTransform.position, this.transform.position);
				playerDirection = PlayerShip.MainPositionTransform.position - this.transform.position;
				playerAngle = Vector3.Angle(this.transform.right, playerDirection);
			}
			else
			{
				playerDistance = -1;
				playerDirection = Vector3.zero;
			}
			
			determineMovement();
			if (!PlayerShip.IsDead)
				manageWeapons();
			
			if (animator && !animator.isPlaying)
				animator.Play();
		}
		else
		{
			if (animator && animator.isPlaying)
				animator.Stop();
		}
	}
	
	void FixedUpdate()
	{
		collidersInSight.Clear();
		collidersInSight.AddRange
		(
			Physics.OverlapSphere
			(
				this.sightSpherePosition,
				this.sightSphereRadius,
				sightSphereLayerMask
			)
		);
	}

	virtual protected void init() {}
	virtual protected int getAvoidancePriority(Collider collider) { return 0; }
	virtual protected void manageWeapons() {}
	private void determineMovement()
	{
		if (!PlayerShip.IsDead)
		{
			if (actions.ContainsKey(state))
				actions[state].Invoke();
		}
		else
		{
			enemyMotor.stopMovement();
		}
	}
	
	protected void killResponse()
	{
		if (animator)
			animator.Stop();
		
		enemy.dropCargo();
		// move this behaviour to the breakable script.
		/*
		if (destructionPrefab) {
			breakMesh = Instantiate(destructionPrefab, this.transform.position, this.transform.rotation) as Breakable;
			breakMesh.BreakApart();
		}
		*/
	}
	virtual protected void hitResponse(float damageTaken) {}
	virtual protected void collisionResponse(Collision collision) {}
	virtual public void playerCollisionResponse() {}
	
	
	
	void OnDrawGizmos ()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(this.sightSpherePosition, this.sightSphereRadius);
		
//		Gizmos.color = Color.green;
//		foreach (Collider c in collidersInSight)
//		{
//			if (c)
//				Gizmos.DrawLine(this.transform.position, c.transform.position);
//		}
	}
}
