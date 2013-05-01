using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
	private static EnemySpawner main;
	public static EnemySpawner Main { get { return main; }}
	
	public WaveInfo[] waveInfo;
	private int waveIndex;
	private int portalIndex;
	private float spawnTime;
	private float nextSpawnTime;
	private int enemyThreshold;
	private int portalThreshold;
	private bool endOfWave;
	private bool firstWaveFlag;

	public float collectionTime = 3f;
	private float collectionTimeRemaining;
	private bool upgradeComplete;
	public bool turretRecallAtEndOfWave = true;
	
	public bool loopSpawner = false;
	[System.NonSerialized]
	public bool isActive = false;
	
	public bool beginActive = true;

	
	public string gameOverLevel = "MainMenu";
	public string nextLevel = "";
	[System.NonSerialized]
	public bool gameOver, levelComplete;
	public float gameOverTime = 5f;
	private float gameOverTimePassed;
	
	private bool isPaused = false;
	public void pause() { isPaused = true; }
	public void resume() { isPaused = false; }
	
	private Transform newEnemy;
	private Enemy newEnemyAI;
	
	public GUIText messageText;
	
	/*
	public void updateUpgradePromptText() {
		if (upgradeTimeRemaining > 0 && !UpgradeMenu.UpgradeMenuShowing) {
			if (upgradePromptText && !upgradePromptText.enabled)
				upgradePromptText.enabled = true;
			if (upgradePromptTime) {
				if (!upgradePromptTime.enabled)
					upgradePromptTime.enabled = true;
				upgradePromptTime.text = Mathf.FloorToInt(upgradeTimeRemaining).ToString();
			}
		}
		else {
			if (upgradePromptText && upgradePromptText.enabled)
				upgradePromptText.enabled = false;
			if (upgradePromptTime && upgradePromptTime.enabled)
				upgradePromptTime.enabled = false;
		}
	}
	*/
	
	public bool isCollectionTime { get { return (collectionTimeRemaining > 0); } }
	
	public string waveText()
	{
		if (isActive)
		{
			return (waveIndex+1).ToString() + "-" + waveInfo[waveIndex].waveName;
		}
		else
		{
			return "none";
		}
	}
	
	
	private bool noActiveWaves()
	{
		for (int w = 0; w < waveInfo.Length; w++)
			if (waveInfo[w] && waveInfo[w].portals.Length > 0)
				return false;
		return true;
	}
	
	private int firstWave()
	{
		for (int w = 0; w < waveInfo.Length; w++)
			if (waveInfo[w])
				return w;
		return -1;
	}
	
	void Awake ()
	{
		if (main == null)
		{
			main = this;
			if (waveInfo.Length > 0) {
				for (int w = 0; w < waveInfo.Length; w++)
				{
					if (waveInfo[w].portals.Length == 0)
					{
						Debug.Log("WARNING: Waves " + (w+1).ToString() + " (" + waveInfo[w].waveName + ") has no portals assigned.");
						break;
					}
				}
			}
			else
			{
				Debug.Log("WARNING: No waves assigned.");
				return;
			}
		}
		else
			Destroy(this.gameObject);
	}
	
	void Start ()
	{
		gameOver = false;
		levelComplete = false;
		gameOverTimePassed = 0;
		if (messageText)
			messageText.enabled = false;
		
		if (beginActive)
			activateSpawner();
	}
	
	void OnDestroy()
	{
		if (main == this)
			main = null;
	}

	public void activateSpawner()
	{
		waveIndex = firstWave();
		firstWaveFlag = true;
		portalIndex = 0;
		endOfWave = false;
		spawnTime = 0f;
		GameState.BuildMenuAvailable = false;
		if (waveIndex >= 0)
		{
			isActive = true;
			setNextSpawnTime();
		}
		if (StoryManager.Main != null)
			StoryManager.Main.triggerEventsByType(StoryEvent.StoryEventTrigger.EnemySpawnerActivated);
		
		collectionTimeRemaining = 2f;
		if (StoryManager.Main != null)
			StoryManager.Main.triggerEventsByType(StoryEvent.StoryEventTrigger.CollectionTimeBegin);
	}
	
	public void deactivateSpawner()
	{
		isActive = false;
		gameOver = true;
		if (messageText)
		{
			messageText.enabled = true;
			messageText.text = "You have repelled\nthe first wave of invaders.";
		}
		if (StoryManager.Main != null)
			StoryManager.Main.triggerEventsByType(StoryEvent.StoryEventTrigger.EnemySpawnerDeactivated);
	}
	
	private void setNextSpawnTime()
	{
		spawnTime = 0f;
		nextSpawnTime = waveInfo[waveIndex].portals[portalIndex].minWaitTime;
		enemyThreshold = waveInfo[waveIndex].portals[portalIndex].maxRemainingEnemies;
		portalThreshold = waveInfo[waveIndex].portals[portalIndex].maxRemainingPortals;
	}

	
	void Update ()
	{
		if (gameOver)
		{
			gameOverTimePassed += Time.deltaTime;
			if (gameOverTimePassed > gameOverTime)
			{
				if (levelComplete)
				{
					if (nextLevel.Length > 0)
						Application.LoadLevel(nextLevel);
				}
				else
					Application.LoadLevel(gameOverLevel);
			}
		}
		else if (GameState.BuildMenuReady)
		{
			if (GameState.BuildMenuAvailable)
				GameState.BuildMenuAvailable = false;
			if (!upgradeComplete)
				upgradeComplete = true;
		}
		else if (!GameState.IsPaused && !PlayerShip.IsDead)
		{
			if (isActive && !isPaused && !noActiveWaves())
			{
				if (upgradeComplete)
				{ // Upgrade Screen has Closed
					upgradeComplete = false;
					endOfWave = false;
					if (firstWaveFlag)
						firstWaveFlag = false;
					else
						nextWave();
					while (isActive && !waveInfo[waveIndex])
						nextWave();
					
					Turret.EnableTurretParking();
					setNextSpawnTime();
					if (StoryManager.Main != null)
						StoryManager.Main.triggerEventsByType(StoryEvent.StoryEventTrigger.WaveBegin);
				}
				else if (collectionTimeRemaining > 0)
				{ // Money Collection Phase
					collectionTimeRemaining -= Time.deltaTime;
					if (collectionTimeRemaining <= 0)
					{
						if (waveInfo[waveIndex].disableUpgradeBeforeWave)
						{
							upgradeComplete = true;
						}
						else
						{
							GameState.BuildMenuAvailable = true;
						}
						
						if (messageText)
						{
							messageText.enabled = false;
						}
						
						if (StoryManager.Main != null)
							StoryManager.Main.triggerEventsByType(StoryEvent.StoryEventTrigger.CollectionTimeEnd);
					}
				}
				else if (endOfWave)
				{ // Wave Phase has Ended
					if (Enemy.EnemyCount() + EnemyPortal.EnemiesInQueue == 0 && EnemyPortal.PortalCount == 0)
					{
						if (!loopSpawner && waveIndex == waveInfo.Length-1)
						{
							levelComplete = true;
							deactivateSpawner();
						}
						else
						{
							collectionTimeRemaining = collectionTime;
							if (messageText)
							{
								messageText.text = "Wave complete";
								messageText.enabled = true;
							}
							if (turretRecallAtEndOfWave)
							{
								//Turret.RecallAll();
								Turret.DisableTurretParking();
							}
							if (PlayerShip.Main)
								PlayerShip.Main.motor.stopDashing();
							if (StoryManager.Main != null)
							{
								StoryManager.Main.triggerEventsByType(StoryEvent.StoryEventTrigger.WaveEnd);
								StoryManager.Main.triggerEventsByType(StoryEvent.StoryEventTrigger.CollectionTimeBegin);
							}
						}
					}
				}
				else
				{ // Enemy Wave Phase
					if ((enemyThreshold < 0 || Enemy.EnemyCount() + EnemyPortal.EnemiesInQueue <= enemyThreshold) &&
						(portalThreshold < 0 || EnemyPortal.PortalCount <= portalThreshold))
					{
						spawnTime += Time.deltaTime;
					
						if (spawnTime >= nextSpawnTime)
						{
							EnemyPortal newPortal = openPortal();
							if (newPortal)
							{
								newPortal.spawnFacingPlayer = waveInfo[waveIndex].portals[portalIndex].spawnFacingPlayer;
								newPortal.activate(waveInfo[waveIndex].portals[portalIndex].optionalSpawnZone);
							}
							if (nextPortal())
							{
								setNextSpawnTime();
							}
							else
							{
								endOfWave = true;
							}
						}
					}
				}
			} // active check
		} // pause & spawn check
	}
	
	private void nextWave()
	{
		portalIndex = 0;
		waveIndex++;
		if (waveIndex >= waveInfo.Length)
		{
			if (loopSpawner)
				waveIndex = 0;
			else
				deactivateSpawner();
		}
	}
	
	private EnemyPortal openPortal()
	{
		if (portalIndex < waveInfo[waveIndex].portals.Length)
		{
			if (waveInfo[waveIndex].portals[portalIndex].triggerStoryEvent.Length > 0)
				StoryManager.Main.triggerEventsByName(waveInfo[waveIndex].portals[portalIndex].triggerStoryEvent);
			
			return Instantiate(waveInfo[waveIndex].portals[portalIndex].portalPrefab) as EnemyPortal;
		}
		return null;
	}
	
	private bool nextPortal()
	{
		portalIndex++;
		if (portalIndex < waveInfo[waveIndex].portals.Length)
		{
			return true;
		}
		portalIndex = 0;
		return false;
	}
	
	
	public int cycloneSpawnWarning1
	{
		get
		{
			if (isActive && waveInfo[waveIndex])
				return waveInfo[waveIndex].cycloneSpawnWarning1;
			else
				return -1;
		}
	}
	public int cycloneSpawnWarning2
	{
		get
		{
			if (isActive && waveInfo[waveIndex])
				return waveInfo[waveIndex].cycloneSpawnWarning2;
			else
				return -1;
		}
	}
	public int cycloneSpawnWarning3
	{
		get
		{
			if (isActive && waveInfo[waveIndex])
				return waveInfo[waveIndex].cycloneSpawnWarning3;
			else
				return -1;
		}
	}
	public int cycloneSpawnThreshold
	{
		get
		{
			if (isActive && waveInfo[waveIndex])
				return waveInfo[waveIndex].cycloneSpawnThreshold;
			else
				return -1;
		}
	}
	
	public void spawnCycloneEnemy(Vector3 spawnLocation)
	{
		if (isActive && waveInfo[waveIndex].cycloneEnemyPrefab)
		{
			newEnemy = Instantiate(waveInfo[waveIndex].cycloneEnemyPrefab, spawnLocation, Quaternion.identity) as Transform;
			newEnemy.transform.parent = this.transform;
		}
		else
			Debug.Log("WARNING: Spawner inactive or no cyclone enemy assigned");
	}
		
	
	/*
	private void startUpgradePrompt() {
		startUpgradePrompt(1f);
	}	
	private void startUpgradePrompt(float promptTimeScale) {
		if (upgradePrompt)
			Destroy(upgradePrompt.gameObject);
		
		upgradeTimeRemaining = upgradeTime * promptTimeScale;
		GameMenu.UpgradeMenuAvailable = true;
		upgradePrompt = Instantiate(upgradePromptPrefab) as UpgradePrompt;
		upgradePrompt.updateProgress(1f);
	}
	*/
		
	/*
	private void endUpgradeMenu() {
		endOfWave = false;
		if (firstWaveFlag)
			firstWaveFlag = false;
		else
			nextWave();
		while (spawnerActive && !waveInfo[wave])
			nextWave();
		
		if (spawnerActive)
			setNextSpawnTime();
	}
	*/
	/*
	private void endUpgradePrompt() {
		Destroy(upgradePrompt.gameObject);
		endOfWave = false;
		GameMenu.UpgradeMenuAvailable = false;
		if (firstWaveFlag)
			firstWaveFlag = false;
		else
			nextWave();
		while (spawnerActive && !waveInfo[wave])
			nextWave();
		
		if (spawnerActive)
			setNextSpawnTime();
		
	}
	
	public void endUpgradeTime() {
		upgradeTimeRemaining = 0;
		endUpgradePrompt();
	}
	*/
	
	
	
} // end of class