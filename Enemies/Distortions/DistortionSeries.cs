using UnityEngine;
using System.Collections;


[System.Serializable]
public class DistortionSpawnerArray
{
	public DistortionSpawner[] distortionSpawners;
	public delegate void allSpawnersComplete();
	public event allSpawnersComplete onAllSpawnersComplete;
	
	private int spawnersComplete;
	public bool begin()
	{
		bool anySpawnerStarted = false;
		spawnersComplete = 0;
		foreach (DistortionSpawner ds in distortionSpawners)
		{
			ds.reset();
			ds.onSpawnComplete += spawnComplete;
			ds.isActive = true;
			anySpawnerStarted = true;
		}
		return anySpawnerStarted;
	}
	
	private void spawnComplete(DistortionSpawner sender)
	{
		sender.onSpawnComplete -= spawnComplete;
		spawnersComplete++;
		if (spawnersComplete >= distortionSpawners.Length && onAllSpawnersComplete != null)
			onAllSpawnersComplete();
	}
}

public class DistortionSeries : MonoBehaviour
{
	public DistortionSpawnerArray[] distortionSpawnerList;
	private int distortionSpawnerIndex;
	
	[System.NonSerialized]
	public bool isActive = false;
	
	public void beginSeries()
	{
		isActive = true;
		
		distortionSpawnerIndex = 0;
		if (distortionSpawnerList.Length > 0 && distortionSpawnerList[0].begin())
		{
			isActive = true;
			distortionSpawnerList[0].onAllSpawnersComplete += spawnComplete;
			distortionSpawnerList[0].begin();
			
		}
		else
		{
			isActive = false;
		}
	}
	
	private void spawnComplete()
	{
		distortionSpawnerList[distortionSpawnerIndex++].onAllSpawnersComplete -= spawnComplete;
		if (distortionSpawnerIndex < distortionSpawnerList.Length)
		{
			distortionSpawnerList[distortionSpawnerIndex].onAllSpawnersComplete += spawnComplete;
			distortionSpawnerList[distortionSpawnerIndex].begin();
		}
		else
		{
			isActive = false;
		}
	}
	
}
