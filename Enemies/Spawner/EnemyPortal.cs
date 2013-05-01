using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(ParticleSystem))]
public class EnemyPortal : MonoBehaviour
{
	
	private static List<EnemyPortal> PortalList;
	public static EnemyPortal[] AllPortals
	{
		get { return PortalList.ToArray(); }
	}
	
	public static int EnemiesInQueue
	{
		get
		{
			int count = 0;
			foreach (EnemyPortal p in PortalList)
				count += p.remainingEnemies;
			return count;
		}
	}
	
	public static bool FreezeAll = false;
	
	public bool activateOnSpawn = false;
	
	private bool isActive = false;
	public bool spawnFacingPlayer = false;
	public float spawnSpeedFactor = 0.5f;
	
	// minimum distance is only used for random locations.
	public static float minimumDistanceFromPlayer { get { return 30.0f; } }
	private Vector3 spawnLocation;
	
	public float timeBeforeInitialSpawn = 3.0f;
	public float timeBetweenSpawns = 1.0f;
	private float timeUntilSpawn;
		
	public int numberOfEnemies;
	private int remainingEnemies;
	private bool finished;
	
	public Enemy enemyPrefab;
	private Enemy newEnemy;
	private EnemyMotor newMotor;
	private Cargo cargo;
	private Quaternion spawnRotation;
	private float zRotation;
	
	public float cargoMultiplier = 1.0f;
	
	static EnemyPortal()
	{
		PortalList = new List<EnemyPortal>();
	}
	
	private static void Register(EnemyPortal newPortal)
	{
		if (!PortalList.Contains(newPortal))
			PortalList.Add(newPortal);
	}
	
	private static void Deregister(EnemyPortal newPortal)
	{
		if (PortalList.Contains(newPortal))
			PortalList.Remove(newPortal);
	}
	
	public static int PortalCount
	{
		get {
			return PortalList.Count;
		}
	}
	

	// random location is used if spawnZone is null.
	public void activate(EnemySpawnZone spawnZone)
	{
		if (spawnZone)
		{
			spawnLocation = spawnZone.randomLocation();
		}
		else
		{
			spawnLocation = FlyZone.RandomLocation();
			Vector3 playerVector = spawnLocation - PlayerShip.MainRotationTransform.position;
			if (playerVector.magnitude < minimumDistanceFromPlayer)
			{
				spawnLocation += playerVector.normalized * (minimumDistanceFromPlayer - playerVector.magnitude);
			}
			if (!FlyZone.ContainsPosition(spawnLocation))
			{
				spawnLocation.x *= -1;
				spawnLocation.y *= -1;
				spawnLocation = FlyZone.Clamp(spawnLocation);
			}
		}
		
		this.transform.position = spawnLocation;
		
		remainingEnemies = numberOfEnemies;
		timeUntilSpawn = timeBeforeInitialSpawn;
		finished = false;
		particleSystem.enableEmission = true;
		
		isActive = true;
	}
	
	public void activate(Vector3 location)
	{
		spawnLocation = FlyZone.Clamp(location);
		
		this.transform.position = spawnLocation;
		
		remainingEnemies = numberOfEnemies;
		timeUntilSpawn = timeBeforeInitialSpawn;
		finished = false;
		particleSystem.enableEmission = true;
		
		isActive = true;
	}
	
	void Start ()
	{
		Register(this);
		
		if (activateOnSpawn)
		{
			this.activate(this.transform.position);
		}
	}
	
	void Update ()
	{
		if (!FreezeAll && isActive && !GameState.IsPaused)
		{
			if (finished)
			{
				if (particleSystem.particleCount == 0)
					Destroy(this.gameObject);
			}
			else
			{
				timeUntilSpawn -= Time.deltaTime;
				if (timeUntilSpawn <= 0) {
					if (spawnFacingPlayer)
						zRotation = Mathm.Rotation2D(0, PlayerShip.MainRotationTransform.position - this.transform.position);
					else
						zRotation = Random.Range(0f, 360f);
					spawnRotation.eulerAngles = new Vector3(0, 0, zRotation);
					newEnemy = Instantiate(enemyPrefab, this.transform.position, spawnRotation) as Enemy;
					newMotor = newEnemy.gameObject.GetComponent<EnemyMotor>();
					if (newMotor)
						newMotor.rigidbody.AddForce(newEnemy.transform.right * newMotor.movementSpeed * spawnSpeedFactor, ForceMode.VelocityChange);
					if (EnemySpawner.Main.transform)
						newEnemy.transform.parent = EnemySpawner.Main.transform;
					cargo = newEnemy.gameObject.GetComponent<Cargo>();
					if (cargo) {
						cargo.moneyDropMin = Mathf.RoundToInt(cargoMultiplier * cargo.moneyDropMin);
						cargo.moneyDropMax = Mathf.RoundToInt(cargoMultiplier * cargo.moneyDropMax);
					}
					remainingEnemies--;
					if (remainingEnemies > 0) {
						timeUntilSpawn = timeBetweenSpawns;
					}
					else {
						finished = true;
						particleSystem.enableEmission = false;
					}
				}
			}
		}
	}
	
	void OnDestroy()
	{
		Deregister(this);
	}
	
	
} // end of class