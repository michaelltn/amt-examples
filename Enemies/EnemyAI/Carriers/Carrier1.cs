using UnityEngine;
using System.Collections;

public class Carrier1 : EnemyAI
{
	
	public float minPlayerDistance = 30f;
	public float maxPlayerDistance = 45f;
	public float maxAngleFromPlayerBack = 60f;
	private Vector3 buildLocation;
	public float minBuildLocationDistance = 3f;
	private Vector3 buildDirection;
	
	public Enemy smallChildShip;
	public Transform smallLaunchPoint;
	public int maxSmallChildShips = 25;
	private int numSmallChildShips = 0;
	public float defaultShipLaunchTime = 1f;
	public float buildShipLaunchTime = 0.2f;
	private float shipLaunchTimeRemaining;
	
	public EnemyPortal[] portalPrefabList;
	public Transform portalBuildPoint;
	public float buildTime = 3f;
	private float buildTimeRemaining;
	public int maxEnemiesForPortalBuild = 200;
	
	public float interuptDamage = 500f;
	private float interuptDamageTaken;
	
	
	
	public string forwardAnimRef = "CarrierForward";
	public string forwardToBuildAnimRef = "CarrierForwardToBuild";
	public string buildAnimRef = "CarrierBuild";
	public string buildToForwardAnimRef = "CarrierBuildToForward";
	
	
	override protected void init()
	{
		this.addState("default", defaultMovement);
		this.addState("building", buildMovement);
		this.setState("default");
		
		shipLaunchTimeRemaining = 0;
		buildTimeRemaining = 0;
		
		returnToDefault();
		
		if (!smallLaunchPoint)
			smallLaunchPoint = this.transform;
		if (!portalBuildPoint)
			portalBuildPoint = this.transform;
	}
	
	// "CarrierForwardToBuild"
	// "CarrierBuildToForward"
	
	private void returnToDefault()
	{
		//currentState = AIState.Default;
		
		if (PlayerShip.MainPositionTransform)
		{
			//Debug.Log("calculating build location...");
			//Debug.Log("  player position " + PlayerShip.MainPositionTransform.position);
			//Debug.Log("  player forward " + PlayerShip.MainPositionTransform.right);
			buildLocation = 
				Quaternion.Euler(0, 0, Random.Range(-maxAngleFromPlayerBack, maxAngleFromPlayerBack)) *
				(-PlayerShip.MainPositionTransform.right * Random.Range(minPlayerDistance, maxPlayerDistance)) + 
				PlayerShip.MainPositionTransform.position;
			//Debug.Log("  unclamped build location: " + buildLocation);
			buildLocation = FlyZone.Clamp(buildLocation);
		}
		else
		{
			buildLocation = Vector3.zero;
		}
		
		//Debug.Log("build location: " + buildLocation);
		//Debug.Log("carrier location: " + this.transform.position);

		if (animator && !animator.IsPlaying(forwardAnimRef) && !animator.IsPlaying(buildToForwardAnimRef))
			animator.Play(buildToForwardAnimRef);
		
		this.setState("default");
	}
	
	private void build()
	{
		//currentState = AIState.Building;
		buildTimeRemaining = buildTime;
		
		interuptDamageTaken = 0;
		
		if (animator && !animator.IsPlaying(buildAnimRef) && !animator.IsPlaying(forwardToBuildAnimRef))
			animator.Play(buildToForwardAnimRef);
		
		this.setState("building");
	}
	
	
	//override protected void defaultMovement()
	private void defaultMovement()
	{
		if (animator)
		{
			if (!animator.isPlaying)
				animator.Play(forwardAnimRef);
			else if (animator.IsPlaying(buildAnimRef))
				animator.Play(buildToForwardAnimRef);
		}
		
		smallShipLaunchManager();
		
		buildDirection = buildLocation - this.transform.position;
		if (buildDirection.magnitude > minBuildLocationDistance)
		{
			enemyMotor.turnToward(buildLocation);
			if (Vector3.Dot(buildDirection, this.transform.right) > 0)
			{
				enemyMotor.moveForward( 1f - Vector3.Angle(this.transform.right, buildDirection) / 90.0f );
			}
			else
			{
				enemyMotor.stopForwardMovement();
			}
			enemyMotor.stopStrafeMovement();
		}
		else
		{
			enemyMotor.stopMovement();
			if (PlayerShip.Main && !PlayerShip.IsDead)
			{
				int totalEnemies = Enemy.EnemyCount() + EnemyPortal.EnemiesInQueue;
				
				if (maxEnemiesForPortalBuild > totalEnemies)
					build();
				else
					returnToDefault();
			}
		}
	}
	
	//override protected void buildMovement()
	private void buildMovement()
	{
		if (animator)
		{
			if (!animator.isPlaying)
				animator.Play(buildAnimRef);
			else if (animator.IsPlaying(forwardAnimRef))
				animator.Play(forwardToBuildAnimRef);
		}
		
		smallShipLaunchManager();
		
		enemyMotor.stopMovement();
		
		buildTimeRemaining -= Time.deltaTime;
		if (buildTimeRemaining <= 0)
		{
			if (portalPrefabList.Length > 0)
			{
				int portalIndex = Random.Range(0, portalPrefabList.Length);
				if (portalPrefabList[portalIndex])
				{
					EnemyPortal newPortal = Instantiate(portalPrefabList[portalIndex]) as EnemyPortal;
					newPortal.cargoMultiplier = 0;
					newPortal.activate(portalBuildPoint.position);
				}
			}
			returnToDefault();
		}
	}
	
	private void smallShipLaunchManager()
	{
		shipLaunchTimeRemaining -= Time.deltaTime;
		if (shipLaunchTimeRemaining <= 0 && numSmallChildShips < maxSmallChildShips)
		{
			Enemy newEnemy = Instantiate(smallChildShip, smallLaunchPoint.position, smallLaunchPoint.rotation) as Enemy;
			newEnemy.transform.parent = this.transform.parent;
			Hull hull = newEnemy.gameObject.GetComponent<Hull>();
			if (hull)
			{
				numSmallChildShips++;
				hull.killEvent += childDestroyed;
			}
			newEnemy.destroyCargo();
			//if (this.isRetreating)
			if (this.getState().Equals("building"))
				shipLaunchTimeRemaining = buildShipLaunchTime;
			else
				shipLaunchTimeRemaining = defaultShipLaunchTime;
		}
	}
	
	override protected void hitResponse(float damageTaken)
	{
		interuptDamageTaken += damageTaken;
		if (interuptDamage > 0 && interuptDamageTaken >= interuptDamage)
		{
			if (this.getState().Equals("building"))
				returnToDefault();
		}
	}
	
	private void childDestroyed()
	{
		numSmallChildShips--;
	}
	
} // end of class