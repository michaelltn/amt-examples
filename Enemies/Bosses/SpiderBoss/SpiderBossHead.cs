using UnityEngine;
using System.Collections;

[System.Serializable]
public class DamageThresholdEffect
{
	public ParticleSystem explosionParticleSystem;
	public ParticleSystem smokeParticleSystem;
	public float damageThreshold01;
	
	public void init()
	{
		if (explosionParticleSystem != null)
		{
			explosionParticleSystem.Stop();
		}
		if (smokeParticleSystem != null)
		{
			smokeParticleSystem.Stop();
		}
	}
	
	public void damageApplied(Hull hull, float damage)
	{
		float previousDurability01 = hull.durability01 + damage;
		if (previousDurability01 >= damageThreshold01 && hull.durability01 < damageThreshold01)
		{
			if (explosionParticleSystem != null)
			{
				explosionParticleSystem.Play();
			}
			if (smokeParticleSystem != null)
			{
				smokeParticleSystem.Play();
			}
		}
	}
}


[RequireComponent (typeof(Hull))]
public abstract class SpiderBossHead : MonoBehaviour
{
	public Animation animator;
	public string inactiveAnim = "head inactive";
	public string activatingAnim = "head activating";
	public string deactivatingAnim = "head deactivating";
	public string deadAnim = "head dead";
	
	public SpiderBossArm spiderBossArm;
	
	public delegate void headInjuryDelegate(SpiderBossHead head);
	public event headInjuryDelegate onInjury;
	
	public delegate void headDestructionStartDelegate(SpiderBossHead head);
	public event headDestructionStartDelegate onDestructionStart;
	
	public delegate void headDestructionCompleteDelegate(SpiderBossHead head);
	public event headDestructionCompleteDelegate onDestructionComplete;
	
	
	public enum HeadState { Inactive, Activating, Active, Deactivating, Dead };
	private HeadState state;
	
	public bool isDead { get { return state == HeadState.Dead; } }
	public bool canActivate { get { return state == HeadState.Inactive || state == HeadState.Deactivating; } }
	public bool canDeactivate { get { return state == HeadState.Active || state == HeadState.Activating; } }

	
	private Hull hull;
	public DamageThresholdEffect[] damageThresholdEffects;
	
	public float rotationSpeed = 180f;
	

	
	void Start ()
	{
		if (spiderBossArm)
		{
			spiderBossArm.setHead(this);
			hull = this.gameObject.GetComponent<Hull>();
			if (hull != null)
				hull.hitEvent += hullDamaged;
			foreach (DamageThresholdEffect dte in damageThresholdEffects)
			{
				dte.init();
			}
		}
		else
		{
			Debug.Log("spider boss arm not assigned to head: " + this.name);
		}
		
		state = HeadState.Inactive;
		
		init();
	}
	
	
	private void hullDamaged(float damage)
	{
		foreach (DamageThresholdEffect dte in damageThresholdEffects)
		{
			dte.damageApplied(hull, damage);
		}
		
		float previousDurability01 = hull.durability01 + damage;
		if (previousDurability01 >= 0.5f && hull.durability01 < 0.5f)
		{
			if (onInjury != null)
				onInjury(this);
		}
	}
	
	
	public bool activate()
	{
		if (canActivate)
		{
			state = HeadState.Activating;
			if (animator != null)
			{
				animator.CrossFade(activatingAnim);
			}
			
			return true;
		}
		return false;
	}
	
	public bool deactivate()
	{
		if (canDeactivate)
		{
			state = HeadState.Deactivating;
			if (animator != null)
				animator.CrossFade(deactivatingAnim);
			
			return true;
		}
		return false;
	}
	
	public bool kill()
	{
		if (!isDead)
		{
			state = HeadState.Dead;
			if (animator != null)
				animator.CrossFade(deadAnim);
			
			if (onDestructionStart != null)
				onDestructionStart(this);
			
			return true;
		}
		return false;
	}
	
	
	
	void Update ()
	{
		switch (state)
		{
		case HeadState.Inactive:
			inactiveUpdate();
			break;
		case HeadState.Active:
			activeUpdate();
			break;
		case HeadState.Activating:
			activatingUpdate();
			break;
		case HeadState.Deactivating:
			deactivatingUpdate();
			break;
		case HeadState.Dead:
			deadUpdate();
			break;
		}
	}
	
	protected abstract void init();
	protected abstract void inactiveUpdate();
	protected abstract void activeUpdate();
	
	
	private void activatingUpdate()
	{
		if (animator != null)
		{
			if (!animator.IsPlaying(activatingAnim))
			{
				state = HeadState.Active;
			}
		}
		else
		{
			state = HeadState.Active;
		}
	}
	
	private void deactivatingUpdate()
	{
		if (animator != null)
		{
			if (!animator.IsPlaying(deactivatingAnim))
			{
				animator.CrossFade(inactiveAnim);
				state = HeadState.Inactive;
			}
		}
		else
		{
			state = HeadState.Inactive;
		}
	}
	
	private void deadUpdate()
	{
		if (animator != null)
		{
			if (!animator.IsPlaying(deadAnim))
			{
				if (onDestructionComplete != null)
					onDestructionComplete(this);
				Destroy(this.gameObject);
			}
		}
		else
		{
			if (onDestructionComplete != null)
				onDestructionComplete(this);
			Destroy(this.gameObject);
		}
	}
	
	Vector2 targetDirection = Vector2.zero;
	float newRotation, currentRotation;
	protected void turnToward(Vector3 targetPosition)
	{
		targetDirection.x  = targetPosition.x - this.transform.position.x;
		targetDirection.y  = targetPosition.y - this.transform.position.y;
		
		currentRotation = this.transform.eulerAngles.z;
		newRotation = Mathm.Rotation2D(currentRotation, targetDirection, rotationSpeed * Time.deltaTime);
		this.transform.eulerAngles = new Vector3(0, 0, newRotation);
	}
	
} // end of class