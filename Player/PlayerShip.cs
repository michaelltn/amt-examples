using UnityEngine;
using System.Collections;

/*
	Required Scripts:
		GameMenu
		
	This script manages details about the player including score, money, shields,
	respawn status, etc...
	
	The list of static components references including PlayerShip, TurretBase and
	reference transforms are set when the gameobject starts and used to allow
	other scripts to reference key aspects of the player.
	
	spawning is private while isSpawning is the public read-only accessor to spawning
	for other scripts to determine if the player is currently spawning.
	
	main reference stores the reference to the only player.  If a second player is added
	to the scene, because main already has a player registered it will delete itself
	in the Start() method.
	
	Since movement is handled by PlayerControls and firing is handled by Turrets,
	the Update() method for this class only manages the spawn timer and shield
	dissipation timer.
*/

[RequireComponent (typeof(PlayerMotor))]
public class PlayerShip : MonoBehaviour
{
	private PlayerMotor playerMotor;
	public PlayerMotor motor { get { return playerMotor; } }
	public PlayerShield playerShield;
	public Renderer currentAIGroupHighlighter;
	
	public PlayerShieldBar shieldBar;
	public float displayShieldAfterHitFor = 1f;
	private float shieldDisplayTimeRemaining;
	public bool invulnerableWhileShieldIsDisplayed = true;
	public int shieldDurability = 100;
	private int shieldLevel;
	private static bool dead;
	
	public int startingMoney = 300;
	private static int money;
	private static int score;
	
	
	public delegate void collectShieldDelegate();
	public event collectShieldDelegate onCollectShield;
	public delegate void collectMoneyDelegate();
	public event collectMoneyDelegate onCollectMoney;

	
	public PlayerCompass playerCompassPrefab;
	public void addCompass(Transform target, Color color, Texture customTexture = null)
	{
		// in case of duplication, remove the target first.
		removeCompass(target);
		
		PlayerCompass newCompass = Instantiate(playerCompassPrefab, this.transform.position, this.transform.rotation) as PlayerCompass;
		
		newCompass.transform.parent = this.transform;
		newCompass.assignTarget(target, color, customTexture);
	}
	public void removeCompass(Transform target)
	{
		foreach (PlayerCompass pc in this.gameObject.GetComponentsInChildren<PlayerCompass>())
		{
			if (pc.target == target)
			{
				pc.removeTarget();
			}
		}
	}
	
	public GuardWedge guardWedgePrefab;
	public void addGuardWedge(Turret targetTurret)
	{
		GuardWedge newGuardWedge = Instantiate(guardWedgePrefab, TurretBaseTransform.position, TurretBaseTransform.rotation) as GuardWedge;
		newGuardWedge.transform.parent = TurretBaseTransform;
		newGuardWedge.assignTurret(targetTurret);
	}
	
	
	public AudioClip hitAudio;
	public Transform shipExplosionPrefab;
	
	public ParticleSystem lightDamageSmokeTrail;
	public float lightDamageThreshold = 0.5f;
	public ParticleSystem heavyDamageSmokeTrail;
	public float heavyDamageThreshold = 0.25f;
	
	private void updateSmokeTrail()
	{
		if (getShieldPercent() > lightDamageThreshold)
		{
			if (lightDamageSmokeTrail)
			{
				lightDamageSmokeTrail.enableEmission = false;
				foreach (ParticleSystem ps in lightDamageSmokeTrail.gameObject.GetComponentsInChildren<ParticleSystem>())
					ps.enableEmission = false;
			}
			if (heavyDamageSmokeTrail)
			{
				heavyDamageSmokeTrail.enableEmission = false;
				foreach (ParticleSystem ps in heavyDamageSmokeTrail.gameObject.GetComponentsInChildren<ParticleSystem>())
					ps.enableEmission = false;
			}
		}
		else if (getShieldPercent() > heavyDamageThreshold)
		{
			if (lightDamageSmokeTrail)
			{
				lightDamageSmokeTrail.enableEmission = true;
				foreach (ParticleSystem ps in lightDamageSmokeTrail.gameObject.GetComponentsInChildren<ParticleSystem>())
					ps.enableEmission = true;
			}
			if (heavyDamageSmokeTrail)
			{
				heavyDamageSmokeTrail.enableEmission = false;
				foreach (ParticleSystem ps in heavyDamageSmokeTrail.gameObject.GetComponentsInChildren<ParticleSystem>())
					ps.enableEmission = false;
			}
		}
		else
		{
			if (lightDamageSmokeTrail)
			{
				lightDamageSmokeTrail.enableEmission = true;
				foreach (ParticleSystem ps in lightDamageSmokeTrail.gameObject.GetComponentsInChildren<ParticleSystem>())
					ps.enableEmission = true;
			}
			if (heavyDamageSmokeTrail)
			{
				heavyDamageSmokeTrail.enableEmission = true;
				foreach (ParticleSystem ps in heavyDamageSmokeTrail.gameObject.GetComponentsInChildren<ParticleSystem>())
					ps.enableEmission = true;
			}
		}
	}
	
	
	public string gameOverLevel = "MainMenu";
	public bool gameOver;
	public GUIText gameOverText;
	public float gameOverTime = 5f;
	private float gameOverTimePassed;
	
	
	public static PlayerShip Main;
	public static Transform MainPositionTransform;
	public static Transform MainRotationTransform;
	public static Transform TurretBaseTransform;
	public static TurretBase TurretBaseComponent;
	public static TurretParkController TurretParkControllerComponent;
	
	// used by the upgrade menu to remember where to put it back
	public static Vector3 returnPosition, upgradePosition;
	public static Quaternion returnRotation, returnTurretBaseRotation;
	
	//public AudioSource audioSource;
	//public AudioClip shieldLoss;
	public AudioClip hitSound;
	
	/*
	public Vector3 respawnLocation {
		get {
			return (spawnFX ? spawnFX.transform.position : Vector3.zero);
		}
	}
	*/
	/*
	public static bool isSpawning {
		get {
			return spawning;
		}
	}
	*/
	
	public static bool IsDead { get { return dead; } }
	
	// Use this for initialization
	void Start ()
	{
		if (!Main)
		{
			Main = this;
			
			if (currentAIGroupHighlighter != null)
				currentAIGroupHighlighter.enabled = false;
			
			dead = false;
			shieldLevel = shieldDurability;
			if (shieldBar)
				shieldBar.updateValue( this.getShieldPercent() );
			
			gameOverTimePassed = 0;
			shieldDisplayTimeRemaining = 0;
			
			ResetMoney();
			score = 0;
			
			if (playerShield == null)
				playerShield = this.gameObject.GetComponentInChildren<PlayerShield>();
			
			playerMotor = this.gameObject.GetComponent<PlayerMotor>();
			MainPositionTransform = this.transform;
			TurretBaseComponent = this.transform.GetComponentInChildren<TurretBase>();
			TurretParkControllerComponent = this.transform.GetComponentInChildren<TurretParkController>();
			TurretBaseTransform = TurretBaseComponent.transform;
			
			this.transform.rotation = Quaternion.identity;
			
			updateSmokeTrail();
		}
		else
			Destroy(this.gameObject);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!GameState.IsPaused)
		{
			if (gameOver)
			{
				gameOverTimePassed += Time.deltaTime;
				if (gameOverTimePassed > gameOverTime)
					Application.LoadLevel(gameOverLevel);
			}
			else
			{
				if (shieldDisplayTimeRemaining > 0)
				{
					shieldDisplayTimeRemaining -= Time.deltaTime;
					if (shieldDisplayTimeRemaining <= 0)
					{
						//this.collider.enabled = true;
						//shieldRenderer.enabled = false;
						playerShield.disable();
					}
				}
			}
		}
	}
	
	public float getShieldPercent() { return Mathf.Clamp01((float)shieldLevel / (float)shieldDurability); }
	
	public void setShieldLevel(int newLevel)
	{
		shieldLevel = Mathf.Clamp(newLevel, 0, shieldDurability) + 1;
		if (shieldBar)
			shieldBar.updateValue( this.getShieldPercent() );
		updateSmokeTrail();
	}
	public void collectShield(int restore)
	{
		if (restore > 0)
		{
			if (shieldLevel < shieldDurability)
			{
				shieldLevel += restore;
				if (shieldLevel > shieldDurability)
					shieldLevel = shieldDurability;
				if (shieldBar)
					shieldBar.updateValue( this.getShieldPercent() );
				updateSmokeTrail();
			}
			
			if (onCollectShield != null)
				onCollectShield();
			
			if (StoryManager.Main != null)
				StoryManager.Main.triggerEventsByType(StoryEvent.StoryEventTrigger.CollectShield);
		}
	}
	
	public void applyDamage(int damage)
	{
		if (!motor.isDashing && !invulnerableWhileShieldIsDisplayed || shieldDisplayTimeRemaining <= 0)
		{
			if (damage > 0)
			{
				if (shieldLevel > 0)
				{
					shieldLevel -= damage;
					if (shieldLevel < 0)
						shieldLevel = 0;
					if (shieldBar)
						shieldBar.updateValue( this.getShieldPercent() );
					//shieldRenderer.enabled = true;
					playerShield.enable();
					shieldDisplayTimeRemaining = displayShieldAfterHitFor;
					//if (invulnerableWhileShieldIsDisplayed)
					//	this.collider.enabled = false;
					updateSmokeTrail();
					if (AudioManager.Main && hitAudio)
						AudioManager.Main.playSFX(hitAudio);
				}
				else
				{
					kill();
				}
			}
		}
	}
	
	public static int Score() { return score; }
	public static void ResetScore() { score = 0; }
	public static void IncScore(int amount) { score += amount; }
	
	public static int Money() { return money; }
	public static void ResetMoney() { money = (PlayerShip.Main ? PlayerShip.Main.startingMoney : 0); }
	public static void CollectMoney(int amount)
	{
		money += amount;
		
		if (PlayerShip.Main != null && PlayerShip.Main.onCollectMoney != null)
			PlayerShip.Main.onCollectMoney();
			
		if (StoryManager.Main != null)
			StoryManager.Main.triggerEventsByType(StoryEvent.StoryEventTrigger.CollectMoney);
	}
	public static bool SpendMoney(int amount)
	{
		if (amount > 0 && money >= amount)
		{
			money -= amount;
			return true;
		}
		return false;
	}
	
	public void kill()
	{
		if (!dead)
		{
			setChildVisibility(this.transform, false);
			Instantiate(shipExplosionPrefab, this.transform.position, Quaternion.identity);
			
			this.collider.enabled = true;
			//shieldRenderer.enabled = false;
			playerShield.disable();
			
			dead = true;
			gameOver = true;
			if (gameOverText) {
				gameOverText.enabled = true;
				gameOverText.text = "You have failed.\nYour children weep.";
			}
		}
	}
	
	/*
	public void respawn() {
		spawning = false;
		if (spawnFX)
			spawnFX.completeSpawn();
		
		EnemySpawner.main.resetCurrentCluster();
		
		this.transform.position = this.respawnLocation;
		this.transform.rotation = Quaternion.identity;
		hasShield = true;
		destroyShield();
		//this.collider.enabled = true;
		
		foreach (Enemy e in Enemy.EnemyList()) {
			if (Vector3.Distance(this.transform.position, e.transform.position) <= respawnDestroyDistance) {
				Hull h = e.transform.gameObject.GetComponent<Hull>();
				if (h)
					h.kill();
			}
		}
	

		setChildVisibility(this.transform, true);
		
		//hasShield = PlayerUpgrades.SpawnWithShield;
		updateShield();
	}
	*/
	
	public void setChildVisibility(Transform root, bool visiblilty)
	{
		foreach (Transform child in root)
			setChildVisibility(child, visiblilty);
		if (root.renderer)
			root.renderer.enabled = visiblilty;
	}
	
	/*
	public void updateShield() {
		shieldTransform.renderer.enabled = hasShield;
	}
	
	public void destroyShield() {
		if (hasShield) {
			hasShield = false;
			shieldDissipationTimeRemaining = shieldDissipationTime;
			this.collider.enabled = false;
			if (shieldLoss)
				AudioManager.main.playSFX(shieldLoss);
		}
	}
	*/
	
	
} // end of class