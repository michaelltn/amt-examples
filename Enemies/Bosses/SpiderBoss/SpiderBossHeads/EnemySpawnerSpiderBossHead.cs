using UnityEngine;
using System.Collections;

public class EnemySpawnerSpiderBossHead : SpiderBossHead
{
	public string spawnerInactiveAnim = "spawner head inactive";
	public string spawnerPlacingAnim = "spawner head placing";
	public string spawnerReturningAnim = "spawner head returning";
	
	private bool isPlacing = false;
	
	public Enemy enemyPrefab;
	public Transform launchPoint;
	private Enemy spawnedEnemy;
	private EnemyMotor spawnedEnemyMotor;
	private EnemyAI spawnedEnemyAI;
	
	protected override void init ()
	{
		isPlacing = false;
	}
	
	protected override void activeUpdate ()
	{
		if (animator)
		{
			if (!animator.isPlaying)
			{
				if (isPlacing)
				{
					releaseEnemy();
					animator.CrossFade(spawnerReturningAnim);
					isPlacing = false;
				}
				else
				{
					spawnEnemy();
					animator.CrossFade(spawnerPlacingAnim);
					isPlacing = true;
				}
			}
		}
	}
	
	protected override void inactiveUpdate ()
	{
		if (animator && !animator.IsPlaying(spawnerInactiveAnim))
		{
			animator.CrossFade(spawnerInactiveAnim);
		}
		
		if (isPlacing)
			isPlacing = false;
		
		if (spawnedEnemy)
		{
			Destroy(spawnedEnemy.gameObject);
		}
	}
	
	private void spawnEnemy()
	{
		spawnedEnemy = Instantiate(enemyPrefab, launchPoint.position, launchPoint.rotation) as Enemy;
		spawnedEnemy.transform.parent = launchPoint;
		spawnedEnemy.destroyCargo();
		spawnedEnemyMotor = spawnedEnemy.gameObject.GetComponent<EnemyMotor>();
		spawnedEnemyMotor.enabled = false;
		spawnedEnemyAI = spawnedEnemy.gameObject.GetComponent<EnemyAI>();
		spawnedEnemyAI.enabled = false;
	}
	
	private void releaseEnemy()
	{
		if (spawnedEnemyMotor)
		{
			spawnedEnemyMotor.enabled = true;
			spawnedEnemyMotor = null;
		}
		if (spawnedEnemyAI)
		{
			spawnedEnemyAI.enabled = true;
			spawnedEnemyAI = null;
		}
		if (spawnedEnemy)
		{
			spawnedEnemy.transform.parent = null;
			spawnedEnemy.transform.eulerAngles = new Vector3(0, 0, spawnedEnemy.transform.eulerAngles.z);
			spawnedEnemy = null;
		}
	}
	
}
