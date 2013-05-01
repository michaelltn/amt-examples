using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BeamHit
{
	public BeamHit(Enemy e, Beam b)
	{
		enemy = e;
		beam = b;
		position = e.transform.position;
	}
	public BeamHit(Enemy e, Beam b, Vector3 pos)
	{
		enemy = e;
		beam = b;
		position = pos;
	}
	public Enemy enemy;
	public Beam beam;
	public Vector3 position;
}

public abstract class BeamWeapon : Weapon
{
	public float beamOffTime = 0.25f;
	private float beamOffTimeRemaining;
	public float beamOnTime = 0.25f;
	private float beamOnTimeRemaining;
	
	public float beamTimeVariance = 0.025f;
	public float minOffTime { get { return beamOffTime * (1f - beamTimeVariance); } }
	public float maxOffTime { get { return beamOffTime * (1f + beamTimeVariance); } }
	private float randomOffTime { get { return Random.Range(minOffTime, maxOffTime); } }
	public float minOnTime { get { return beamOnTime * (1f - beamTimeVariance); } }
	public float maxOnTime { get { return beamOnTime * (1f + beamTimeVariance); } }
	private float randomOnTime { get { return Random.Range(minOnTime, maxOnTime); } }
	
	public Beam beamPrefab = null;
	private Beam currentBeam = null;
	
	public int beamHops = 0;
	public float maxHopDistance = 15f;
	public float maxHopAngle = 90f;
	public int beamSplitsPerHop = 0;
	private Beam[] additionalBeams;
	
	private Enemy[] availableEnemies;
	private int availableEnemiesRemaining;
	private void fillAvailableEnemies()
	{
		availableEnemies = Enemy.EnemyList();
		availableEnemiesRemaining = availableEnemies.Length;
	}
	private int getClosestAvailableEnemyIndex(Vector3 location)
	{
		int index = -1;
		float closestDistSq = 0f, checkDistSq;
		for (int i = 0; i < availableEnemiesRemaining; i++)
		{
			if (availableEnemies[i] != null)
			{
				checkDistSq = Vector3.SqrMagnitude(availableEnemies[i].transform.position - location);
				if (index < 0 || checkDistSq < closestDistSq)
				{
					index = i;
					closestDistSq = checkDistSq;
				}
			}
		}
		return index;
	}
	private int getAvailableEnemyIndex(Enemy enemy)
	{
		if (availableEnemies != null)
		{
			for (int i = 0; i < availableEnemiesRemaining; i++)
			{
				if (availableEnemies[i] != null && availableEnemies[i] == enemy)
					return i;
			}
		}
		return -1;
	}
	private Enemy popAvailableEnemy(int index)
	{
		if (availableEnemies != null && index >= 0 && index < availableEnemiesRemaining)
		{
			Enemy returnEnemy = availableEnemies[index];
			availableEnemies[index] = availableEnemies[--availableEnemiesRemaining];
			return returnEnemy;
		}
		return null;
	}
	private bool enemyIsAvailable(Enemy enemy)
	{
		if (availableEnemies != null)
		{
			for (int i = 0; i < availableEnemiesRemaining; i++)
			{
				if (availableEnemies[i] != null && availableEnemies[i] == enemy)
					return true;
			}
		}
		return false;
	}
	
	private List<BeamHit> beamHitList;
	private Enemy[] enemiesHit;
	
	override public void init()
	{
		base.init();
		firing = false;
		if (audio && audio.clip)
		{
			if (audio.isPlaying)
				audio.Stop();
			audio.loop = false;
		}
	}
	
	override protected void createShotPool()
	{
		base.createShotPool();
		currentBeam = Instantiate(beamPrefab, this.turret.barrelTransform.position, this.turret.barrelTransform.rotation ) as Beam;
		currentBeam.assignSourceWeapon(this);
		currentBeam.setDamageMultiplier(this.damageMultiplier);
		currentBeam.disableImmediate();
		
		if (beamHops > 0)
		{
			additionalBeams = new Beam[beamHops * (beamSplitsPerHop + 1)];
			for (int i = 0; i < additionalBeams.Length; i++)
			{
				additionalBeams[i] = Instantiate(beamPrefab, this.turret.barrelTransform.position, this.turret.barrelTransform.rotation ) as Beam;
				additionalBeams[i].assignSourceWeapon(this);
				additionalBeams[i].setDamageMultiplier(this.damageMultiplier);
				additionalBeams[i].disableImmediate();
			}
		}
	}
	
	override protected void destroyShotPool()
	{
		base.destroyShotPool();
		if (currentBeam != null)
			Destroy(currentBeam.gameObject);
		if (additionalBeams != null)
			for (int i = 0; i < additionalBeams.Length; i++)
				if (additionalBeams[i] != null)
					Destroy(additionalBeams[i].gameObject);
	}
	
	override public void update()
	{
		if (firing)
		{
			if (beamOnTimeRemaining > 0)
			{
				beamOnTimeRemaining -= Time.deltaTime;
				if (beamOnTimeRemaining <= 0)
				{
					beamOffTimeRemaining = randomOffTime;// beamOffTime;
					turnBeamOff();
				}
				else
					turnBeamOn();
			}
			else
			{
				if (beamOffTimeRemaining > 0)
				{
					turnBeamOff();
					beamOffTimeRemaining -= Time.deltaTime;
				}
				else
				{
					beamOnTimeRemaining = randomOnTime; //beamOnTime;
					turnBeamOn();
				}
			}
		}
		else
		{
			turnBeamOff();
			if (beamOffTimeRemaining > 0)
				beamOffTimeRemaining -= Time.deltaTime;
		}
	}
	
	private void turnBeamOn()
	{
		if (!currentBeam.isEnabled)
		{
			currentBeam.enable();
			if (audio && audio.clip)
			{
				if (!audio.isPlaying)
				{
					audio.loop = true;
					audio.Play();
				}
			}
		}
	}
	
	private void turnBeamOff()
	{
		if (currentBeam.isEnabled)
		{
			currentBeam.disable();
			if (audio && audio.clip)
			{
				audio.loop = false;
				//if (audio.isPlaying)
				//	audio.Stop();
			}
		}
		if (additionalBeams != null)
			for (int i = 0; i < additionalBeams.Length; i++)
				if (additionalBeams[i].isEnabled)
					additionalBeams[i].disable();
	}
	
	// called by the Turret
	private Vector3[] hitPoints;
	public void updateBeam()
	{
		int i, iEnemy, iBeam, iHop, iSplit;
		if (currentBeam != null && currentBeam.isEnabled)
		{
			if (beamHitList == null)
				beamHitList = new List<BeamHit>();
			else
				beamHitList.Clear();
			
			fillAvailableEnemies();
			
			enemiesHit = currentBeam.updateBeam(this.turret.barrelTransform, out hitPoints);
			for (i = 0; i < enemiesHit.Length; i++)
			{
				if (enemiesHit[i] != null)
				{
					iEnemy = getAvailableEnemyIndex(enemiesHit[i]);
					if (iEnemy >= 0)
					{
						beamHitList.Add(new BeamHit(popAvailableEnemy(iEnemy), currentBeam, hitPoints[i]));
					}
				}
			}
			
			iBeam = 0;
			if (beamHitList.Count > 0)
			{
				if (beamHops > 0)
				{
					Vector3 currentDirection = currentBeam.transform.right;
					Vector3 nextDirection = Vector3.zero;
					//Vector3 sourcePosition = beamHitList[0].enemy.transform.position;
					Vector3 sourcePosition = beamHitList[0].position;
					//Debug.Log("hit pos: " + sourcePosition + ", enemy pos" + beamHitList[0].enemy.transform.position);
					Vector3 nextSource = Vector3.zero;
					bool hopped;
					for (iHop = 0; iHop < beamHops; iHop++)
					{
						hopped = false;
						iSplit = 0;
						
						while (iSplit < beamSplitsPerHop+1 && availableEnemiesRemaining > 0)
						{
							iEnemy = getClosestAvailableEnemyIndex(sourcePosition);
							Enemy targetEnemy = popAvailableEnemy(iEnemy);
							if (targetEnemy == null) break;
							
							float testDistance = Vector3.Distance(targetEnemy.transform.position, sourcePosition);
							float testAngle = Vector3.Angle(currentDirection, targetEnemy.transform.position - sourcePosition);
							
							if (testDistance <= maxHopDistance && testAngle <= maxHopAngle)
							{
								additionalBeams[iBeam].enable();
								additionalBeams[iBeam].updateBeamWithoutRaycast(sourcePosition, targetEnemy.transform.position);
								
								beamHitList.Add(new BeamHit(targetEnemy, additionalBeams[iBeam]));
								if (!hopped)
								{
									nextDirection = targetEnemy.transform.position - sourcePosition;
									nextSource = targetEnemy.transform.position;
									hopped = true;
								}
								iBeam++;
								iSplit++;
							}
						}
						
						if (hopped)
						{
							currentDirection = nextDirection;
							sourcePosition = nextSource;
						}
						else
							break;
					} // hop index
				}
				// apply damage to everything in the beamHitList
				foreach (BeamHit beamHit in beamHitList)
				{
					beamHit.beam.applyDamage(beamHit.enemy);
				}
			}
			if (additionalBeams != null)
			{
				while (iBeam < additionalBeams.Length)
				{
					if (additionalBeams[iBeam].isEnabled)
						additionalBeams[iBeam].disable();
					iBeam++;
				}
			}
		}
	}
	
	override public void fire(Transform barrel)
	{
		base.fire(barrel);
		//if (!firing)
		//{
		//	base.fire(barrel);
			/*
			if (randomFiringDelay && beamOffTime > 0)
				beamOffTimeRemaining = Random.Range(beamOffTimeRemaining, beamOffTime);
				*/
			/*
			if (beamOffTime > 0)
			{
				beamOffTimeRemaining += Random.Range(0, (beamOffTime - beamOffTimeRemaining) * randomFiringDelay);
			}
			*/
		//}
		
		//if (currentBeam != null)
		//{
		//	currentBeam = Instantiate(beamPrefab, this.turret.barrelTransform.position, this.turret.barrelTransform.rotation ) as Beam;
		//	currentBeam.assignSourceWeapon(this);
		//}
	}
	
	override public void stopFiring()
	{
		base.stopFiring();
		turnBeamOff();
	}
	
}