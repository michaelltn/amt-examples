using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (Hull))]
public class Enemy : MonoBehaviour
{
	private EnemyAI enemyAI;
	public EnemyAI ai { get { return enemyAI; } }
	private EnemyMotor enemyMotor;
	public EnemyMotor motor { get { return enemyMotor; } }
	
	public string enemyType = "Basic Enemy";
	
	private bool flyZoneFlag;
	public bool hasEnteredFlyZone { get { return flyZoneFlag; } }
	
	protected Hull hull;
	public int collisionDamageToPlayer = 10;
	public float collisionDamageFromPlayer = 10f;
	public float collisionPlayerSpeedReduction = 0.25f;
	
	public bool isDead { get { return (hull && hull.durability <= 0); } }
	
	[System.NonSerialized]
	public Cargo cargo;
	private bool cargoDestroyed = false;
	public void destroyCargo() { cargoDestroyed = true; }
	
	private static List<Enemy> enemyList;
	private static List<string> enemyTypeList;
	public delegate void ListUpdate();
	public static event ListUpdate updateEvent;
	
	static Enemy()
	{
		enemyList = new List<Enemy>();
		enemyTypeList = new List<string>();
	}
	
	void Awake ()
	{
		hull = this.gameObject.GetComponent<Hull>();
		cargo = this.gameObject.GetComponent<Cargo>();
		enemyAI = this.gameObject.GetComponent<EnemyAI>();
		enemyMotor = this.gameObject.GetComponent<EnemyMotor>();
	}
	
	void Start ()
	{
		flyZoneFlag = false;
		
		Register(this);
	}
	
	void OnDestroy ()
	{
		DeRegister(this);
	}
	
	void Update ()
	{
		if (!GameState.IsPaused)
		{
			// once the enemy enters the flyzone, auto-turrets can target it.
			if (!flyZoneFlag && FlyZone.ContainsTransform(this.transform)) {
				flyZoneFlag = true;
			}
		}
	}
	
	public void dropCargo()
	{
		if (cargo && !cargoDestroyed) {
			cargo.drop(this.transform.position);
			cargoDestroyed = true;
		}
	}

	
	/*
		Static functions
	*/
	private static void Register(Enemy enemy)
	{
		if (!enemyList.Contains(enemy))
		{
			enemyList.Add(enemy);
			if (!enemyTypeList.Contains(enemy.enemyType))
				enemyTypeList.Add(enemy.enemyType);
			updateEvent();
		}
	}
	
	private static void DeRegister(Enemy enemy)
	{
		if (enemyList.Contains(enemy))
		{
			enemyList.Remove(enemy);
			if (Enemy.EnemyCount(enemy.enemyType) == 0 && enemyTypeList.Contains(enemy.enemyType))
				enemyTypeList.Remove(enemy.enemyType);
			updateEvent();
		}
	}
	
	public static Enemy[] EnemyList()
	{
		return enemyList.ToArray();
	}
	
	public static Enemy[] EnemyList(string filterType)
	{
		Enemy[] enemyArray = new Enemy[EnemyCount(filterType)];
		
		for (int e = 0, i = 0; e < enemyList.Count; e++)
		{
			if (enemyList[e].enemyType == filterType)
			{
				enemyArray[i++] = enemyList[e];
			}
		}
		
		return enemyArray;
	}
	
	public static string[] EnemyTypeList()
	{
		return enemyTypeList.ToArray();
	}
	
	public static int EnemyCount()
	{
		return EnemyCount("");
	}
	public static int EnemyCount(string filterType)
	{
		if (filterType == "")
			return enemyList.Count;
		else
		{
			int count = 0;
			for (int e = 0; e < enemyList.Count; e++)
			{
				if (enemyList[e].enemyType == filterType)
					count++;
			}
			return count;
		}
	}
	
	public static Enemy ClosestEnemy(Vector3 location)
	{
		return ClosestEnemy(location, "");
	}
	public static Enemy ClosestEnemy(Vector3 location, string filterType)
	{
		Enemy closestEnemy = null;
		float closestDistance = 0f;
		float checkDistance = 0f;
		
		for (int e = 0; e < enemyList.Count; e++)
		{
			if (filterType == "" || filterType == enemyList[e].enemyType)
			{
				checkDistance = Vector3.Distance( location, enemyList[e].transform.position );
				if (!closestEnemy)
				{
					closestDistance = checkDistance;
					closestEnemy = enemyList[e];
				}
				else
				{
					if (checkDistance < closestDistance)
					{
						closestDistance = checkDistance;
						closestEnemy = enemyList[e];
					}
				}
			}
		}
		
		return closestEnemy;
	}
	
	public static Enemy WeakestEnemy()
	{
		return WeakestEnemy("");
	}
	public static Enemy WeakestEnemy(string filterType)
	{
		Enemy weakestEnemy = null;
		float lowestDurability = 0f;
		
		Hull checkHull;
		for (int e = 0; e < enemyList.Count; e++)
		{
			if (filterType == "" || filterType == enemyList[e].enemyType)
			{
				checkHull = enemyList[e].gameObject.GetComponent<Hull>();
				if (checkHull)
				{
					if (!weakestEnemy || checkHull.durability < lowestDurability)
					{
						weakestEnemy = enemyList[e];
						lowestDurability = checkHull.durability;
					}
				}
			}
		}
		
		return weakestEnemy;	
	}
	
	public static Enemy StrongestEnemy()
	{
		return StrongestEnemy("");
	}
	public static Enemy StrongestEnemy(string filterType)
	{
		Enemy strongestEnemy = null;
		float highestDurability = 0f;
		
		Hull checkHull;
		for (int e = 0; e < enemyList.Count; e++)
		{
			if (filterType == "" || filterType == enemyList[e].enemyType)
			{
				checkHull = enemyList[e].gameObject.GetComponent<Hull>();
				if (checkHull)
				{
					if (!strongestEnemy || checkHull.durability > highestDurability)
					{
						highestDurability = checkHull.durability;
						strongestEnemy = enemyList[e];
					}
				}
			}
		}
		
		return strongestEnemy;	
	}
	
	public static Enemy GetAITarget(bool preferStrongest, bool preferVulnerable, Transform barrelTransform, Weapon turretWeapon)
	{
		Enemy bestEnemy = null;
		bool bestIsWeak = false;
		float bestDurability = 0;
		float bestDistance = -1f;
		
		Hull checkHull;
		bool checkIsWeak = false;
		float checkDistance = 0f;
		Vector3 enemyDirection = Vector3.zero;
		float enemyAngle = 0f, turretAngle = 0f;
		float checkAngle = 0;

		/*
		 * 1) get the array of enemies
		 * 2) based on settings and aiWeaponRange, pick the target enemy
		 * 		a) If "vulnerable" is checked and potential is vulnerable while current is not, potential becomes current.
				b) Else if "strongest" is selected and one has a higher hull value, the one with the most hull becomes the current.
				c) Else the enemy closest to the turret becomes the current.
		 * */
		
		for (int e = 0; e < enemyList.Count; e++)
		{
			if (enemyList[e].flyZoneFlag) {
				if (!turretWeapon.turret.isParked)
				{
					//enemyDirection = enemyList[e].transform.position - turretWeapon.transform.position;
					enemyDirection = enemyList[e].transform.position - PlayerShip.MainPositionTransform.position;
					enemyAngle = Vector3.Angle(enemyDirection, Vector3.right);
					if (enemyDirection.y < 0)
						enemyAngle = 360f - enemyAngle;
					turretAngle = PlayerShip.TurretBaseTransform.localEulerAngles.z + turretWeapon.turret.guardAngle;
					checkAngle = Mathf.Abs(Mathf.DeltaAngle(turretAngle, enemyAngle));
				}
				if (turretWeapon.turret.isParked || checkAngle <= Turret.MaxAITargetingAngle)
				{
					checkDistance = Vector3.Distance( barrelTransform.position, enemyList[e].transform.position );
					if (checkDistance <= Turret.AIRange)
					{
						checkHull = enemyList[e].gameObject.GetComponent<Hull>();
						checkIsWeak = checkHull.isWeakAgainst(turretWeapon);
						
						if (checkHull)
						{
							if (!bestEnemy)
							{
								bestEnemy = enemyList[e];
								bestIsWeak = checkIsWeak;
								bestDurability = checkHull.durability;
								bestDistance = checkDistance;
							}
							else
							{
								if (preferVulnerable && !bestIsWeak && checkIsWeak)
								{
									bestEnemy = enemyList[e];
									bestIsWeak = checkIsWeak;
									bestDurability = checkHull.durability;
									bestDistance = checkDistance;
								}
								else if ( !preferVulnerable || (bestIsWeak == checkIsWeak) )
								{
									if (preferStrongest && bestDurability < checkHull.durability)
									{
										bestEnemy = enemyList[e];
										bestIsWeak = checkIsWeak;
										bestDurability = checkHull.durability;
										bestDistance = checkDistance;
									}
									else if ( !preferStrongest || (bestDurability == checkHull.durability) )
									{
										if (checkDistance < bestDistance)
										{
											bestEnemy = enemyList[e];
											bestIsWeak = checkIsWeak;
											bestDurability = checkHull.durability;
											bestDistance = checkDistance;
										}
									}
								}
							}
						}
					} // end distance check
				} // end angle check
			} // end flyzone check
		} // end for
		
		return bestEnemy;
	}

} // end of class