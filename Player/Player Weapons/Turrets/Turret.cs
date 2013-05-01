using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(TurretMotor))]
public class Turret : MonoBehaviour
{
	public Weapon weapon = null;
	private TurretMotor turretMotor;
	public TurretMotor motor { get { return turretMotor; } }
	
	public bool forceOneShotMinimum = true;
	private bool shotFlag;
	
	public float manualRotationSpeed = 360f;
	public float aiRotationSpeed = 180f;
	public float maxAIFiringAngle = 30f;
	public float snapAngle = 1f;
	
	private float currentRotation, newRotation;
//    public Quaternion baseRotation;
	
	public delegate void recallAllDelegate();
	public static event recallAllDelegate onRecallAll;
	public delegate void parkAllDelegate();
	public static event parkAllDelegate onParkAll;

	
	// if provided, the barrel is the transform that looks at the cursor.
	// if not, the transform of the game object is used.
	public Transform barrelTransform = null;
	
	[System.NonSerialized]
	public Transform followTarget, oldFollowTarget;
	
	private bool parked;

	// look direction of turret
	private Vector2 targetDirection; //, playerPoint;
	
	public bool armed = true;
	public static bool Armed = true;
	
	public static float AIRange             { get { return 45f; } }
	public static float MaxAITargetingAngle { get { return 45f; } }
	[System.NonSerialized]
	public float guardAngle = 0f;
	
	private bool aiIsEnabled = false;
	
	public bool aiEnabled
	{
		get { return aiIsEnabled; }
		set
		{
			if (value == true && PlayerShip.Main != null)
				PlayerShip.Main.addGuardWedge(this);
			
			aiIsEnabled = value;
		}
	}
	public bool aiStrongest = false;
	public bool aiVulnerable = false;
	private Enemy targetEnemy = null;
	private Vector3 targetLocation;
	
	public Color color;
	public Transform rangePlane;
	
	public Renderer aiHighlighter;
	private bool showHighlighter
	{
		get
		{
			if (aiIndex < 0) return false;
			
			if (GameState.IsPaused)
				return GameState.BuildMenuReady;
			
			return true;
		}
	}
	
	private bool showRangePlane
	{
		get
		{
			if (aiIndex < 0) return false;
			
			if (GameState.IsPaused)
				return false;
			
			return this.aiEnabled && this.isParked;
		}
	}
	
	private int _aiIndex = -1;
	public int aiIndex { get { return _aiIndex; } }
	public bool isInAIGroup { get { return this._aiIndex >= 0; } }
	
	public void assignAI(int index, Color color)
	{
		if (index < 0) return;
		
		this._aiIndex = index;
		this.color = color;
		if (aiHighlighter)
		{
			aiHighlighter.enabled = true;
			aiHighlighter.material.SetColor("_TintColor", color);
		}
		
		Color rangeColor = color;
		//rangeColor.a = 0.10f;
		if (rangePlane && rangePlane.renderer)
			rangePlane.renderer.material.SetColor("_TintColor", rangeColor);
		
		this.aiEnabled = true;
	}
	public void removeAI()
	{
		this.aiEnabled = false;
		_aiIndex = -1;
		if (aiHighlighter)
			aiHighlighter.enabled = false;
	}

	
	// used by the upgrade menu to remember where to put it back
	[System.NonSerialized]
	public Vector3 returnPosition, upgradePosition;
	[System.NonSerialized]
	public Quaternion returnRotation, upgradeRotation;
	
	private static bool TurretParkingEnabled = true;
	public static void EnableTurretParking()
	{
		if (!TurretParkingEnabled)
			TurretParkingEnabled = true;
	}
	public static void DisableTurretParking()
	{
		if (TurretParkingEnabled)
		{
			RecallAll();
			TurretParkingEnabled = false;
		}
	}
	
	private static List<Turret> turretList;
	
	static Turret()
	{
		turretList = new List<Turret>();
	}
	
	public static int WeaponCount(Weapon weaponType)
	{
		int c = 0;
		foreach (Turret t in turretList) {
			Weapon w = t.weapon;
			if (w is HypometricWeapon && weaponType is HypometricWeapon)
				c++;
			if (w is SuperluminalWeapon && weaponType is SuperluminalWeapon)
				c++;
			if (w is FusionWeapon && weaponType is FusionWeapon)
				c++;
		}
		
		return c;
	}
	
	public static int TurretCount
	{
		get
		{
			return turretList.Count;
		}
	}

    public void resetRotation()
    {
        barrelTransform.eulerAngles = new Vector3(0, 0, guardAngle);
    }
//    public void updateBaseRotation()
//    {
//        baseRotation = transform.rotation;
//    }
	
	void Awake ()
	{
		Register(this);
	}

	void Start ()
	{
		guardAngle = 0;
        //baseRotation = transform.rotation;
		turretMotor = this.gameObject.GetComponent<TurretMotor>();
		shotFlag = false;
		
		if (!barrelTransform)
			barrelTransform.eulerAngles = new Vector3(0, 0, guardAngle);
			//barrelTransform = this.transform;
		if (!weapon)
			weapon = this.gameObject.GetComponent<Weapon>();
		if (weapon)
			weapon.onWeaponFired += onWeaponFire;
			
		targetDirection = new Vector2(0, 0);
		
		parked = false;
		
		if (rangePlane)
		{
			rangePlane.localScale = new Vector3(AIRange*0.2f, AIRange*0.2f, AIRange*0.2f);
		}
		
		if (rangePlane && rangePlane.renderer && rangePlane.renderer.enabled != showRangePlane)
			rangePlane.renderer.enabled = showRangePlane;
		
		if (aiHighlighter && aiHighlighter.enabled != showHighlighter)
			aiHighlighter.enabled = showHighlighter;
	}
	
	private void AimingUpdateFunction()
	{
		if (!GameState.IsPaused)
		{
			if (Armed && armed)
			{
				if (!PlayerShip.IsDead)
					aim();
			}
		}
	}
	
	void Update()
	{
		if (rangePlane && rangePlane.renderer && rangePlane.renderer.enabled != showRangePlane)
			rangePlane.renderer.enabled = showRangePlane;
		
		if (aiHighlighter && aiHighlighter.enabled != showHighlighter)
			aiHighlighter.enabled = showHighlighter;
		
		
		if (!GameState.IsPaused)
		{
			if (Armed && armed)
			{
				if (!PlayerShip.IsDead)
					fire();
				else
					weapon.stopFiring();
			}
			else
			{
				weapon.stopFiring();
			}
		}
	}
	
	public void update()
	{
		AimingUpdateFunction();
		
		
	}
	
	void OnDestroy()
	{
		if (followTarget != null)
			Destroy(followTarget.gameObject);
		if (oldFollowTarget != null)
			Destroy(oldFollowTarget.gameObject);
		if (weapon)
			weapon.onWeaponFired -= onWeaponFire;
		
		DeRegister(this);
	}
	
	private void onWeaponFire(Weapon weapon)
	{
		if (shotFlag)
			shotFlag = false;
	}
	
	virtual protected void aim()
	{
		targetDirection = Vector2.zero;
		
		targetEnemy = null;
		if (aiEnabled && weapon)
		{
			// ----------------------------
			// manage aiming and firing automatically
			// ----------------------------
			targetEnemy = Enemy.GetAITarget(aiStrongest, aiVulnerable, barrelTransform, weapon);
			if (targetEnemy)
			{
				if (weapon is HypometricWeapon && targetEnemy.motor != null)
				{
					targetLocation = Mathm.FirstOrderIntercept(this.transform.position, Vector3.zero, 75f, targetEnemy.transform.position, targetEnemy.motor.rigidbody.velocity);
				}
				else
				{
					targetLocation = targetEnemy.transform.position;
				}
				Debug.DrawLine(this.transform.position, targetLocation, Color.yellow);
				
				targetDirection.x = targetLocation.x - barrelTransform.position.x;
				targetDirection.y = targetLocation.y - barrelTransform.position.y;
				if (targetDirection.magnitude > 0)
					targetDirection.Normalize();
			}
			else
			{
				if (parked)
				{
					targetDirection.x = PlayerShip.Main.transform.position.x - barrelTransform.position.x;
					targetDirection.y = PlayerShip.Main.transform.position.y - barrelTransform.position.y;
//					targetDirection.x = Mathf.Cos(guardAngle);
//					targetDirection.y = Mathf.Sin(guardAngle);
				}
				else
				{
					targetDirection.x = barrelTransform.position.x - PlayerShip.Main.transform.position.x;
					targetDirection.y = barrelTransform.position.y - PlayerShip.Main.transform.position.y;
//					Vector2 mouseVec = (Vector2)InputManager.PointerPosition - (Vector2)Camera.main.WorldToScreenPoint(PlayerShip.MainPositionTransform.position);
//					float angleOffset = Mathf.Atan2(mouseVec.y, mouseVec.x) * Mathf.Rad2Deg;
//					float finalAngle = guardAngle + angleOffset;
//					if(finalAngle > 360f)
//						finalAngle -= 360f;
//					else if(finalAngle < 0)
//						finalAngle += 360f;
					
//					targetDirection.x = Mathf.Cos(angleOffset);
//					targetDirection.y = Mathf.Sin(angleOffset);
//					targetDirection.x = Mathf.Cos(finalAngle);
//					targetDirection.y = Mathf.Sin(finalAngle);
//					targetDirection.x = Mathf.Cos(PlayerShip.TurretBaseTransform.eulerAngles.z);
//					targetDirection.y = Mathf.Sin(PlayerShip.TurretBaseTransform.eulerAngles.z);
				}
			}
		}
		
		currentRotation = barrelTransform.eulerAngles.z;
		//newRotation = Mathm.Rotation2D(currentRotation, PlayerShip.TurretBaseTransform.eulerAngles.z + guardAngle, manualRotationSpeed * Time.deltaTime);
		if (aiEnabled)
		{
			if(targetEnemy)
				newRotation = Mathm.Rotation2D(currentRotation, targetDirection, aiRotationSpeed * Time.deltaTime);
			else
				newRotation = Mathm.Rotation2D(currentRotation, PlayerShip.TurretBaseTransform.eulerAngles.z + guardAngle, aiRotationSpeed * Time.deltaTime);
		}
		else
		{
			newRotation = Mathm.Rotation2D(currentRotation, PlayerShip.TurretBaseTransform.eulerAngles.z + guardAngle, manualRotationSpeed * Time.deltaTime);
		}
		barrelTransform.eulerAngles = new Vector3(0, 0, newRotation);
		
		if (weapon)
		{
			if (weapon is BeamWeapon) {
				(weapon as BeamWeapon).updateBeam();
			}
		}
	}
	
	virtual protected void fire()
	{
		if (weapon)
		{
			if (aiEnabled)
			{
				if (targetEnemy && Vector3.Angle(targetLocation - barrelTransform.position, barrelTransform.right) < maxAIFiringAngle)
				{
					weapon.fire(barrelTransform);
				}
				else
				{
					weapon.stopFiring();
				}
			}
			else
			{
				if (shotFlag || InputManager.FireButton || (InputManager.UsingController && InputManager.FireWhenTilting && InputManager.LookEngaged))
				{
					if (!shotFlag && !weapon.isFiring)
						shotFlag = true;
					weapon.fire(barrelTransform);
				}
				else
				{
					weapon.stopFiring();
				}
			}
		}
	}
	
	virtual public void stopFiring()
	{
		if (weapon)
			weapon.stopFiring();
	}
	
	public bool isParked { get { return parked; } }
	
	public void park()
	{
		if (TurretParkingEnabled && !parked)
		{
			GameObject newFollowTarget = new GameObject();
			newFollowTarget.transform.position = this.transform.position;
			oldFollowTarget = followTarget;
			followTarget = newFollowTarget.transform;
			parked = true;
		}
	}
	
	public void recall()
	{
		if (parked)
		{
			Destroy(followTarget.gameObject);
			followTarget = oldFollowTarget;
			oldFollowTarget = null;
			parked = false;
		}
	}
	
	public void toggleParkedState()
	{
		if (parked)
			recall();
		else
			park();
	}
	
	
	/*
		Static functions
	*/
	private static void Register(Turret turret)
	{
		if (!turretList.Contains(turret))
		{
			turretList.Add(turret);
		}
	}
	
	private static void DeRegister(Turret turret)
	{
		if (turretList.Contains(turret))
		{
			turretList.Remove(turret);
		}
	}
	
	public static Turret[] TurretList()
	{
		return turretList.ToArray();
	}
	
	public static void StopFiringAll()
	{
		foreach (Turret t in turretList)
		{
			t.stopFiring();
		}
	}
	
	
	public static int NumberParkedInGroup(int index)
	{
		if (index < 0) return 0;
		int count = 0;
		foreach (Turret t in turretList)
		{
			if (t.aiIndex == index && t.isParked)
			{
				count++;
			}
		}
		return count;
	}
	
	public static void ParkGroup(int index)
	{
		if (index < 0) return;
		foreach (Turret t in turretList)
		{
			if (t.aiIndex == index)
			{
				t.park();
			}
		}
	}
	
	public static void RecallGroup(int index)
	{
		if (index < 0) return;
		foreach (Turret t in turretList)
		{
			if (t.aiEnabled && t.aiIndex == index)
			{
				t.recall();
			}
		}
	}
	
	
	public static int NumberParked()
	{
		int count = 0;
		foreach (Turret t in turretList)
		{
			if (t.isInAIGroup && t.isParked)
			{
				count++;
			}
		}
		return count;
	}
	
	public static void ParkAll()
	{
		foreach (Turret t in turretList)
		{
			if (t.isInAIGroup && !t.isParked)
				t.park();
		}
		
		if (Turret.onParkAll != null)
			Turret.onParkAll();
	}
	
	public static void RecallAll()
	{
		foreach (Turret t in turretList)
		{
			if (t.isInAIGroup && t.isParked)
				t.recall();
		}
		
		if (Turret.onRecallAll != null)
			Turret.onRecallAll();
	}
	
	
	public static void DisarmAll()
	{
		foreach (Turret t in turretList)
		{
			t.armed = false;
		}
	}
	
	public static void ArmAll()
	{
		foreach (Turret t in turretList)
		{
			t.armed = true;
		}
	}
	
	
} // end of class