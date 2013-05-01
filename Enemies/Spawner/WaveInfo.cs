using UnityEngine;
using System.Collections;

[System.Serializable]
public class PortalInfo
{
	public string name = "(portal info)";
	public EnemyPortal portalPrefab;
	public EnemySpawnZone optionalSpawnZone;
	public bool spawnFacingPlayer = false;
	public int maxRemainingEnemies = -1;
	public int maxRemainingPortals = -1;
	public float minWaitTime = 3f;
	public string triggerStoryEvent;
} // end of class

public class WaveInfo : MonoBehaviour
{
	public string waveName = "Wave Information";
	public bool disableUpgradeBeforeWave = false;
	public Transform cycloneEnemyPrefab;
	public int cycloneSpawnWarning1 = 15;
	public int cycloneSpawnWarning2 = 20;
	public int cycloneSpawnWarning3 = 25;
	public int cycloneSpawnThreshold = 50;
	public PortalInfo[] portals;
		
} // end of class
