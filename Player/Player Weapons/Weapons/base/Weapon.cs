using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Weapon : MonoBehaviour
{
	public static int LowestPurchaseCost = 100;
	
	public static float DamageIncreasePerLevel = 0.1f;
	public static int BaseDamageUpgradeCost = 1000;
	public static int DamageUpgradeCostPerLevel = 100;
	
	public string weaponName;
	public Texture2D weaponIcon;
	public string description;
	public string featureList;
	
	public Weapon[] upgradeWeaponPrefabs;
	public int purchaseCost = 0;
	public int sellValue = 0;

	public Turret turret;
	
	
	public delegate void weaponFiredDelegate(Weapon weapon);
	public event weaponFiredDelegate onWeaponFired;
	
	public delegate void anyWeaponFiredDelegate();
	public static event anyWeaponFiredDelegate onAnyWeaponFired;
	
	
	public bool canUpgradeDamage = false;
	private int damageLevel = 0;
	public int damageUpgradeCost
	{
		get
		{
			return BaseDamageUpgradeCost + (DamageUpgradeCostPerLevel * damageLevel);
		}
	}
	public float damageMultiplier
	{
		get
		{
			return 1f + (DamageIncreasePerLevel * damageLevel);
		}
	}
	public bool increaseDamage()
	{
		if (canUpgradeDamage && PlayerShip.SpendMoney(damageUpgradeCost))
		{
			damageLevel++;
			return true;
		}
		return false;
	}
	
	protected bool firing = false;
	public bool isFiring { get { return firing || firingDelayFlag; } }
	
	public float randomFiringDelay = 0.25f;
	private float firingDelayRemaining;
	private bool firingDelayFlag = false;
	
	
	//public Transform shipTransform;
	//public Renderer[] upgradeMeshes;
	//protected int level;
	
	void Awake ()
	{
		Register(this);
	}
	
	void Start ()
	{
		if (!turret)
			turret = this.gameObject.GetComponent<Turret> ();
		if (!turret)
			turret = this.transform.parent.gameObject.GetComponent<Turret> ();
		if (!turret)
			Debug.Log ("WARNING: no turret assigned to weapon and failed to get one automatically.");
		
		firing = false;
		firingDelayFlag = false;
		firingDelayRemaining = 0;
		
		createShotPool();
		init ();
	}
	
	void Update ()
	{
		if (!GameState.IsPaused)
		{
			if (firingDelayRemaining > 0)
				firingDelayRemaining -= Time.deltaTime;
			
			update();
		}
	}
	
	void OnDestroy ()
	{
		stopFiring ();
		destroyShotPool();
		DeRegister(this);
	}
	
	virtual protected void createShotPool() {}
	virtual protected void destroyShotPool() {}
	
	virtual public void init() {}
	virtual public void update() {}
	virtual public void fire(Transform barrel)
	{
		if (!firing)
		{
			if (!firingDelayFlag)
			{
				if (Weapon.AnyWeaponIsFiring)
				{
					firingDelayRemaining = Random.Range(0f, randomFiringDelay);
				}
				else
				{
					firingDelayRemaining = 0;
				}
				firingDelayFlag = true;
			}
			if (firingDelayRemaining <= 0)
			{
				firing = true;
						
				if (this.onWeaponFired != null)
					this.onWeaponFired(this);
				if (Weapon.onAnyWeaponFired != null)
					Weapon.onAnyWeaponFired();
			}
		}
	}
	virtual public void stopFiring()
	{
		if (firing) firing = false;
		if (firingDelayFlag) firingDelayFlag = false;
	}
	
	public Weapon upgrade(int index)
	{
		if (index < 0 || index >= upgradeWeaponPrefabs.Length)
			return this;
		if (upgradeWeaponPrefabs[index] == null)
			return this;
		if (!PlayerShip.SpendMoney(upgradeWeaponPrefabs[index].purchaseCost))
			return this;
		
		Weapon upgradeWeapon = Instantiate(upgradeWeaponPrefabs[index], this.transform.position, this.transform.rotation) as Weapon;
		upgradeWeapon.turret.returnPosition = this.turret.returnPosition;
		upgradeWeapon.turret.returnRotation = this.turret.returnRotation;
		upgradeWeapon.turret.upgradePosition = this.turret.upgradePosition;
		upgradeWeapon.turret.upgradeRotation = this.turret.upgradeRotation;
		upgradeWeapon.turret.guardAngle = this.turret.guardAngle;
		if (this.turret.followTarget)
		{
			upgradeWeapon.turret.followTarget = Instantiate(this.turret.followTarget, this.turret.followTarget.position, this.turret.followTarget.rotation) as Transform;
			upgradeWeapon.turret.followTarget.parent = this.turret.followTarget.parent;
		}
		if (this.turret.oldFollowTarget)
		{
			upgradeWeapon.turret.oldFollowTarget = Instantiate(this.turret.oldFollowTarget, this.turret.oldFollowTarget.position, this.turret.oldFollowTarget.rotation) as Transform;
			upgradeWeapon.turret.oldFollowTarget.parent = this.turret.oldFollowTarget.parent;
		}
		//int slot = PlayerShip.TurretParkControllerComponent.getSlot(this.turret);
		upgradeWeapon.turret.assignAI(this.turret.aiIndex, this.turret.color);
//		//int index = this.turret.aiIndex;
//		if (slot >= 0)
//			PlayerShip.TurretParkControllerComponent.assignParkTurret(upgradeWeapon.turret, slot);
		Destroy(this.gameObject);
		return upgradeWeapon;
	}
	
	
	/*
	Static functions
	*/
	private static List<Weapon> weaponList;
	
	static Weapon()
	{
		weaponList = new List<Weapon>();
	}
	
	private static void Register(Weapon weapon)
	{
		if (!weaponList.Contains(weapon))
		{
			weaponList.Add(weapon);
		}
	}
	
	private static void DeRegister(Weapon weapon)
	{
		if (weaponList.Contains(weapon))
		{
			weaponList.Remove(weapon);
		}
	}
	
	public static Weapon[] WeaponList()
	{
		return weaponList.ToArray();
	}
	
	public static bool AnyWeaponIsFiring
	{
		get
		{
			foreach (Weapon w in weaponList)
				if (w.isFiring) return true;
			return false;
		}
	}
	

	
} // end of class