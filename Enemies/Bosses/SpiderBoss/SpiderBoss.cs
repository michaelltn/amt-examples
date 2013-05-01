using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiderBoss : BossAI
{		
	
	public SpiderBoss()
	{
		arms = new List<SpiderBossArm>(6);
	}
	
	public ParticleSystem[] explosionParticleSystems;
	public float explosionTime = 5f;
	private float explosionTimeRemaining;

	public override void init ()
	{
		
	}
	
	public override void defaultMovement ()
	{
		throw new System.NotImplementedException ();
	}
	
	public override void deadMovement ()
	{
		// do stuff until death animation is complete
		/*
		if death animation is complete
		{
			this.endDeath();
		}
		*/
		
		explosionTimeRemaining -= Time.deltaTime;
		if (explosionTimeRemaining <= 0)
		{
			endDeath();
		}
	}
	
	public override void beginDeath ()
	{
		base.beginDeath ();
		
		foreach (ParticleSystem ps in explosionParticleSystems)
		{
			ps.Play();
		}
		
		if (explosionTime > 0)
			explosionTimeRemaining = explosionTime;
		else
			endDeath();
	}
	
	public override void endDeath ()
	{
		base.endDeath ();
	}
	
	
	private List<SpiderBossArm> arms;
	
	public void addArm(SpiderBossArm arm)
	{
		if (!arms.Contains(arm))
		{
			arms.Add(arm);
			arm.onDestructionStart += armDestructionStart;
			arm.onDestructionComplete += armDestructionComplete;
		}
	}
	
	private void armDestructionStart(SpiderBossArm arm)
	{
		// do an animation?
	}
	
	private void armDestructionComplete(SpiderBossArm arm)
	{
		if (arms.Contains(arm))
			arms.Remove(arm);
		
		if (arms.Count == 0)
		{
			beginDeath();
		}
	}
	
	
	
} // end of class