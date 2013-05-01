using UnityEngine;
using System.Collections;

public class DistortionSpawner : MonoBehaviour
{
	public Distortion distortionPrefab;
	private Distortion newDistortion;
	public float timeBetweenSpawns = 1f;
	private float spawnTimeRemaining;
	
	public Transform node1, node2;
	public float nodeLoopTime = 4f;
	private float timePassed;
	
	public bool isActive = false;
	public int distortionSpawnCount = 100;
	private int distortionsRemaining;
	
	
	public delegate void spawnCompleteDelegate(DistortionSpawner sender);
	public event spawnCompleteDelegate onSpawnComplete;
	
	public void reset()
	{
		isActive = false;
		distortionsRemaining = distortionSpawnCount;
		spawnTimeRemaining = timeBetweenSpawns;
	}
	
	
	void Start ()
	{
		reset();
	}
	
	void Update ()
	{
		if (isActive)
		{
			timePassed += Time.deltaTime;
			timePassed %= nodeLoopTime;
			
			if (node1 && node2)
			{
				this.transform.position = Vector3.Lerp(node1.position, node2.position, 0.5f * Mathf.Sin(Mathf.PI * 2f * timePassed / nodeLoopTime) + 0.5f);
			}
			
			if (distortionsRemaining > 0)
			{
				spawnTimeRemaining -= Time.deltaTime;
				if (spawnTimeRemaining <= 0)
				{
					newDistortion = Instantiate(distortionPrefab, this.transform.position, this.transform.rotation) as Distortion;
					//newDistortion.changeDirection(this.transform.forward);
					spawnTimeRemaining = timeBetweenSpawns;
					distortionsRemaining--;
					if (distortionsRemaining == 0)
					{
						this.isActive = false;
						if (onSpawnComplete != null)
							onSpawnComplete(this);
					}
				}
			}
		}
	}
	
}
